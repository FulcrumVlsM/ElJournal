using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Models;
using ElJournal.Providers;
using ElJournal.DBInteract;
using System.Text.RegularExpressions;
using NLog;

namespace ElJournal.Controllers
{
    public class CourseWorkController : ApiController
    {
        // получить список всех курсовых работ (администратор)
        // получить курсовую работу по id (все рег. пользователи)
        // получить курсовые работы по предмету
        // получить выполненные процентовки курсовых работ студентом (все рег. пользователи)
        // получить статус выполнения курсовой работы студентом (да, нет)
        // добавить курсовую работу (администратор, преподаватель)
        // добавить курсовую работу в план по предмету (администратор, преподаватель)
        // установить курсовую работу студенту (администратор, преподаватель)
        // добавить этап процентовки в план по курсовой работе (администратор, преподаватель)
        // поставить отметку о выполнении процентовки (администратор, преподаватель)
        // поставить отметки о выполнении курсовой работы (администратор, преподаватель)
        // изменить данные о курсовой работе (администратор, создатель)
        // изменить данные процентовки курсовой работы (администратор, преподаватель)
        // удалить курсовую работу (администратор, создатель)
        // удалить курсовую работу из плана (администратор, преподаватель)
        // удалить этап процентовки (преподаватель, администратор)


        // GET: api/CourseWork
        // получить список всех курсовых работ (администратор)
        public async Task<HttpResponseMessage> Get([FromUri]string name=null)
        {
            Response response = new Response();
            
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if(authProvider!= null)
            {
                if (authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION))
                {
                    try
                    {
                        if (string.IsNullOrEmpty(name))// если шаблон для поиска не задавался
                            response.Data = await LabWork.GetCollectionAsync();
                        else // если шаблон поиска был задан
                        {
                            Regex regex = new Regex(name);
                            response.Data = (await CourseWork.GetCollectionAsync())
                                .Where(x => regex.IsMatch(x.Name) || regex.IsMatch(x.Advanced)).ToList();
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
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }


        // GET: api/CourseWork/5
        // получить курсовую работу по id (все рег. пользователи)
        [HttpGet]
        [Route("api/CourseWork/{id}")]
        public async Task<HttpResponseMessage> GetById(string id)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                response.Data = await CourseWork.GetInstanceAsync(id);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }


        // GET: api/CourseWork/BySubject/5
        // получить курсовые работы по предмету (все)
        [HttpGet]
        [Route("api/CourseWork/BySubject/{id}")]
        public async Task<HttpResponseMessage> GetBySubj(string id)
        {
            Response response = new Response();
            string sqlQuery = "select * from dbo.PlannedCourseWorks(@subjectId)";
            var parameters = new Dictionary<string, string>
            {
                {"subjectId",id }
            };

            try
            {
                DB db = DB.GetInstance();
                var works = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                response.Data = CourseWork.ToCourseWork(works);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        // GET: api/CourseWork/Stage/5/5
        // получить выполненные процентовки курсовых работ студентом (все рег. пользователи)
        //TODO: метод еще пустой
        [HttpGet]
        [Route("api/CourseWork/stage/{studentId}/{subjectId}")]
        public async Task<HttpResponseMessage> GetStage(string studentId, string subjectId)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // GET: api/CourseWork/State/5
        // получить статус выполнения курсовой работы студентом (да, нет) (все рег. пользователи)
        //TODO: метод еще пустой
        [HttpGet]
        [Route("api/CourseWork/state/{studentId}")]
        public async Task<HttpResponseMessage> GetState(string studentId)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // POST: api/CourseWork
        // добавить курсовую работу (администратор, преподаватель)
        public async Task<HttpResponseMessage> Post([FromBody]CourseWork courseWork)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null) //если пользователь не отправил токен получить ошибку 401
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            try
            {
                //проверка наличия прав у пользователя
                bool commonRight = default(bool),
                    teacherRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION),
                                () => teacherRight = authProvider.CheckPermission(Permission.CRSWRK_PERMISSION));

                if (commonRight || teacherRight)
                {
                    if (await courseWork.Push(authProvider.PersonId)) //добавление работы в БД
                    {
                        var id = await getCWork(courseWork.Name, courseWork.Advanced);
                        response.Data = new { ID = id };
                        return Request.CreateResponse(HttpStatusCode.Created, response);
                    }
                    else
                    {
                        Logger logger = LogManager.GetCurrentClassLogger();
                        logger.Warn(string.Format("Не удалось добавить лабораторную работу. User: {0}, name: {1}",
                            authProvider.PersonId, courseWork.Name)); //запись лога с ошибкой
                        return Request.CreateResponse(HttpStatusCode.Conflict);
                    }
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString()); //запись лога с ошибкой
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }


        }


        // POST: api/CourseWork/Plan/5/5
        // добавить курсовую работу в план по предмету (администратор, преподаватель)
        [HttpPost]
        [Route("api/CourseWork/plan/{subjectId}/{workId}")]
        public async Task<HttpResponseMessage> PostPlan(string subjectId, string workId)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // POST: api/CourseWork/Exec/5/5
        // установить курсовую работу студенту (администратор, преподаватель)
        [HttpPost]
        [Route("api/CourseWork/exec/{studentId}/{planWorkId}")]
        public async Task<HttpResponseMessage> PostExec(string studentId, string planWorkId)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // POST: api/CourseWork/Stage/5
        // добавить этап процентовки в план по курсовой работе (администратор, преподаватель)
        [HttpPost]
        [Route("api/CourseWork/stage/{subjectId}")]
        public async Task<HttpResponseMessage> PostStage(string subjectId, [FromBody]CourseWorkStage stage)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // POST: api/CourseWork/Stage/5/5
        // поставить отметку о выполнении процентовки (администратор, преподаватель)
        [HttpPost]
        [Route("api/CourseWork/stage/{id}/{studentId}")]
        public async Task<HttpResponseMessage> PostStage(string id, string studentId, [FromUri]bool state=true)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // POST: api/CourseWork/State/5
        // поставить отметки о выполнении курсовой работы (администратор, преподаватель)
        [HttpPost]
        [Route("api/CourseWork/state/{studentId}")]
        public async Task<HttpResponseMessage> PostState(string studentId, [FromBody]CourseWorkExecutionModels advanced,
            [FromUri]bool state=true)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // PUT: api/CourseWork/5
        // изменить данные о курсовой работе (администратор, создатель)
        public async Task<HttpResponseMessage> Put(string id, [FromBody]CourseWork courseWork)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            try
            {
                //проверка наличия прав пользователя
                bool right = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION) ||
                    authProvider.CourseWorks.Contains(id);

                if (right)
                {
                    courseWork.ID = id;
                    if (await courseWork.Update()) //обновление записи в БД
                        return Request.CreateResponse(HttpStatusCode.OK);
                    else
                    {
                        Logger logger = LogManager.GetCurrentClassLogger();
                        logger.Error(string.Format("Лаб. работа \"{0}\" не была обновлена", id)); //запись лога с ошибкой
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString()); //запись лога с ошибкой
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        // PUT: api/CourseWork/Stage/5
        // изменить данные процентовки курсовой работы (администратор, преподаватель)
        [HttpPut]
        [Route("api/CourseWork/stage/{id}")]
        public async Task<HttpResponseMessage> PutStage(string id, [FromBody]CourseWorkStage stage)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // DELETE: api/CourseWork/5
        // удалить курсовую работу (администратор, создатель)
        public HttpResponseMessage Delete(string id)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // удалить курсовую работу из плана (администратор, преподаватель)
        [HttpDelete]
        [Route("api/CourseWork/plan/{id}")]
        public HttpResponseMessage DeletePlan(string id)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // удалить этап процентовки (преподаватель, администратор)
        [HttpDelete]
        [Route("api/CourseWork/stage/{id}")]
        public HttpResponseMessage DeleteStage(string id)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }



        /// <summary>
        /// получает id курс. работы из имени и доп. информации
        /// </summary>
        /// <param name="name">имя</param>
        /// <param name="advanced">доп. информация</param>
        /// <returns></returns>
        private async Task<string> getCWork(string name, string advanced)
        {
            DB db = DB.GetInstance();
            string sqlQuery = "select ID from CourseWorks " +
                "where name=@name and advanced=@advanced";
            var parameters = new Dictionary<string, string>
            {
                { "@name", name },
                { "@advanced", advanced }
            };
            return await db.ExecuteScalarQueryAsync(sqlQuery, parameters);
        }
    }
}
