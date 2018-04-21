using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Models;
using ElJournal.DBInteract;
using System.Text.RegularExpressions;
using NLog;
using ElJournal.Providers;

namespace ElJournal.Controllers
{
    //develop: Vilisov Mikhail
    public class GroupsController : ApiController
    {
        // получить полный список групп (все)
        // получить группу по id (все)
        // получить все группы существующие в указанном семестре (все)
        // добавить группу (администратор, администратор факультета)
        // добавить группу на указанный семестр (администратор, администратор факультета)
        // изменить группу (администратор, администратор факультета)
        // удалить группу из указанного семестра (администратор, администратор факультета)
        // удалить группу (администратор, администратор факультета)


        // GET: api/Groups
        // получить полный список групп (все)
        public async Task<HttpResponseMessage> Get([FromUri]string name = null)
        {
            Response response = new Response();
            try
            {
                if (string.IsNullOrEmpty(name))// если шаблон для поиска не задавался
                    response.Data = await Models.Group.GetCollectionAsync();
                else // если шаблон поиска был задан
                {
                    Regex regex = new Regex(name);
                    response.Data = (await Models.Group.GetCollectionAsync()).FindAll(x => regex.IsMatch(x.Name));
                }
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Info(e.ToString());//запись лога с ошибкой
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

        }


        // GET: api/Groups/5
        // получить группу по id (все)
        [HttpGet]
        [Route("api/Groups/{id}")]
        public async Task<HttpResponseMessage> GetConcrete(string id)
        {
            Response response = new Response();
            try
            {
                response.Data = await Models.Group.GetInstanceAsync(id);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Info(e.ToString());//запись лога с ошибкой
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        // получить все группы существующие в указанном семестре (все)
        [HttpGet]
        [Route("api/Groups/BySemester/{semesterId}")]
        public async Task<HttpResponseMessage> GetBySemester(string semesterId)
        {
            Response response = new Response();
            string sqlQuery = "select ID, name from dbo.GetGroupsFromSemester(@semesterId)";
            var parameters = new Dictionary<string, string>
            {
                {"@semesterId",semesterId }
            };

            try
            {
                DB db = DB.GetInstance();
                var list = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                response.Data = Models.Group.ToGroups(list);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Info(e.ToString());//запись лога с ошибкой
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
        

        // POST: api/Groups
        // добавить группу (администратор, администратор факультета)
        public async Task<HttpResponseMessage> Post([FromBody]Models.Group group)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                bool commonRight = default(bool),
                    facultyRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.GROUP_COMMON_PERMISSION),
                    () => facultyRight = authProvider.CheckPermission(Permission.GROUP_PERMISSION) ? 
                                         authProvider.Faculties.Contains(group.FacultyId) : false);

                if(commonRight || facultyRight)
                {
                    if (await group.Push())
                    {
                        response.Data = new { ID = await getGroup(group.Name) };
                        return Request.CreateResponse(HttpStatusCode.Created, response);
                    }
                    else
                    {
                        Logger logger = LogManager.GetCurrentClassLogger();
                        logger.Warn(string.Format("Не удалось добавить группу к факультету \"{0}\" и с куратором \"{1}\".",
                            group.FacultyId, group.CuratorId));
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                    }
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }


        // POST: api/Groups/5/5
        // добавить группу на указанный семестр (администратор, администратор факультета)
        [HttpPost]
        [Route("api/Groups/{groupId}/{semesterId}")]
        public async Task<HttpResponseMessage> PostOnSemester(string groupId, string semesterId)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск группы
            Models.Group group = await Models.Group.GetInstanceAsync(groupId);
            if(group==null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав на операцию
            bool commonRight = default(bool),
                    facultyRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.GROUP_COMMON_PERMISSION),
                () => facultyRight = authProvider.CheckPermission(Permission.GROUP_PERMISSION) ?
                                     authProvider.Faculties.Contains(group.FacultyId) : false);

            if(commonRight || facultyRight)
            {
                if(await group.AddToSemester(semesterId))
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // PUT: api/Groups/5
        // изменить группу (администратор, администратор факультета)
        [HttpPut]
        [Route("api/Groups/{id}")]
        public async Task<HttpResponseMessage> Put(string id, [FromBody]Models.Group group)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                //проверка наличия прав на операцию
                bool commonRight = default(bool),
                        facultyRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.GROUP_COMMON_PERMISSION),
                    () => facultyRight = authProvider.CheckPermission(Permission.GROUP_PERMISSION) ?
                                         authProvider.Faculties.Contains(group.FacultyId) : false);

                if(commonRight || facultyRight)
                {
                    group.ID = id;
                    if(await group.Update())
                        return Request.CreateResponse(HttpStatusCode.OK);
                    else
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }


        // удалить группу из указанного семестра (администратор, администратор факультета)
        [HttpDelete]
        [Route("api/Groups/{groupId}/{semesterId}")]
        public async Task<HttpResponseMessage> DeleteToSemester(string groupId, string semesterId)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if(authProvider != null)
            {
                //поиск группы
                Models.Group group = await Models.Group.GetInstanceAsync(groupId);
                if(group==null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest);

                //проверка наличия прав на операцию
                bool commonRight = default(bool),
                        facultyRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.GROUP_COMMON_PERMISSION),
                    () => facultyRight = authProvider.CheckPermission(Permission.GROUP_PERMISSION) ?
                                         authProvider.Faculties.Contains(group.FacultyId) : false);

                if (commonRight || facultyRight)
                {
                    if(group.DeleteToSemester(semesterId))
                        return Request.CreateResponse(HttpStatusCode.OK);
                    else
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }


        // DELETE: api/Groups/5
        // удалить группу (администратор, администратор факультета)
        [HttpDelete]
        [Route("api/Groups/{id}")]
        public async Task<dynamic> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                //поиск группы
                Models.Group group = await Models.Group.GetInstanceAsync(id);
                if (group == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest);

                //проверка наличия прав на операцию
                bool commonRight = default(bool),
                        facultyRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.GROUP_COMMON_PERMISSION),
                    () => facultyRight = authProvider.CheckPermission(Permission.GROUP_PERMISSION) ?
                                         authProvider.Faculties.Contains(group.FacultyId) : false);

                if(commonRight || facultyRight)
                {
                    if(group.Delete())
                        return Request.CreateResponse(HttpStatusCode.OK);
                    else
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }



        /// <summary>
        /// получает id группы по имени
        /// </summary>
        /// <param name="name">имя</param>
        /// <returns></returns>
        private async Task<string> getGroup(string name)
        {
            DB db = DB.GetInstance();
            string sqlQuery = "select ID from Groups " +
                "where name=@name";
            var parameters = new Dictionary<string, string>
            {
                { "@name", name }
            };
            return await db.ExecuteScalarQueryAsync(sqlQuery, parameters);
        }
    }
}
