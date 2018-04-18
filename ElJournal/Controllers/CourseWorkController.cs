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
    public partial class CourseWorkController : ApiController
    {
        // получить список всех курсовых работ (администратор)
        // получить курсовую работу по id (все рег. пользователи)
        // получить курсовые работы по предмету
        // получить статус выполнения курсовой работы студентом (да, нет)
        // добавить курсовую работу (администратор, преподаватель)
        // добавить курсовую работу в план по предмету (администратор, преподаватель)
        // установить курсовую работу студенту (администратор, преподаватель)
        // поставить отметки о выполнении курсовой работы (администратор, преподаватель)
        // изменить данные о курсовой работе (администратор, создатель)
        // удалить курсовую работу (администратор, создатель)
        // удалить курсовую работу из плана (администратор, преподаватель)


        // GET: api/CourseWork?name=abc
        // получить список всех курсовых работ (администратор)
        public async Task<HttpResponseMessage> Get([FromUri]string name = null)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null) //если идентификация прошла успешно
            {
                if (authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION)) //если есть права доступа
                {
                    try
                    {
                        if (string.IsNullOrEmpty(name))// если шаблон для поиска не задавался
                            response.Data = await CourseWork.GetCollectionAsync();
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
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        // GET: api/CourseWork/State/5
        // получить статус выполнения курсовой работы студентом (да, нет) (все рег. пользователи)
        //TODO: метод еще пустой
        [HttpGet]
        [Route("api/CourseWork/state/{studentId}")]
        public async Task<HttpResponseMessage> GetState(string studentId)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                response.Data = await CourseWorkExecution.GetInstanceAsync(studentId);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

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
            string sqlQuery = "insert into CourseWorkPlan(SubjectGroupSemesterID,CourseWorkID) " +
                "values (@subjId,@workId)";

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            try
            {
                if (authProvider != null)
                {
                    //проверка наличия прав на операцию
                    bool commonRight = default(bool),
                        teacherRight = default(bool);
                    Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION),
                        () => teacherRight = authProvider.CheckPermission(Permission.CRSWRK_PERMISSION) ?
                                             authProvider.FlowsSubjects.Contains(subjectId) : false);

                    if (commonRight || teacherRight)
                    {
                        var parameters = new Dictionary<string, string>
                    {
                        {"@subjId",subjectId },
                        {"@workId",workId }
                    };
                        DB db = DB.GetInstance();
                        int result = await db.ExecInsOrDelQueryAsync(sqlQuery, parameters);
                        if (result == 1)
                            return Request.CreateResponse(HttpStatusCode.Created);
                        else
                        {
                            Logger logger = LogManager.GetCurrentClassLogger();
                            logger.Warn("Не удалось добавить в план по предмету \"{0}\" курсовую работу \"{1}\"");
                            return Request.CreateResponse(HttpStatusCode.BadRequest);
                        }
                    }
                    else
                        return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Warn(e.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        // POST: api/CourseWork/Exec/5/5/5
        // установить курсовую работу студенту (администратор, преподаватель)
        [HttpPost]
        [Route("api/CourseWork/exec/{subjectId}/{workId}/{studentId}")]
        public async Task<HttpResponseMessage> PostExec(string subjectId, string workId, string studentId)
        {
            string procName = "dbo.MountCourseWorkToExec"; //хранимая процедура, выполняющая необходимые действия

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                try
                {
                    //проверка наличия прав на операцию
                    bool commonRight = default(bool),
                        teacherRight = default(bool);
                    Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION),
                        () => teacherRight = authProvider.CheckPermission(Permission.CRSWRK_PERMISSION) ?
                                             authProvider.FlowsSubjects.Contains(subjectId) : false);

                    DB db = DB.GetInstance();
                    var parameters = new Dictionary<string, string>
                    {
                        {"@studentId",studentId },
                        {"@subjectId",subjectId },
                        {"@workId",workId }
                    };

                    bool result = Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
                    if (result)
                        return Request.CreateResponse(HttpStatusCode.Created);
                    else
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                catch (Exception e)
                {
                    Logger logger = LogManager.GetCurrentClassLogger();
                    logger.Warn(e.ToString()); //запись лога с ошибкой
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);


        }


        // POST: api/CourseWork/State/5
        // поставить отметку о выполнении курсовой работы (администратор, преподаватель)
        [HttpPost]
        [Route("api/CourseWork/state/{studentId}")]
        public async Task<HttpResponseMessage> PostState(string studentId, [FromBody]CourseWorkExecution advanced,
            [FromUri]bool state=true)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск курсовой работы, которую выполняет студент
            CourseWorkExecution execution = await CourseWorkExecution.GetInstanceAsync(studentId);
            if (execution == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //поиск предмета, к которому относится курсовая работа студента
            string subjectId = await getSubjectOfCourseWorkExec(execution.ID);
            if(string.IsNullOrEmpty(subjectId))
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            try
            {
                //проверка наличия прав на операцию
                bool commonRight = default(bool),
                    teacherRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION),
                    () => teacherRight = authProvider.CheckPermission(Permission.CRSWRK_PERMISSION) ?
                                         authProvider.FlowsSubjects.Contains(subjectId) : false);

                if(teacherRight || commonRight)
                {
                    execution.State = state;
                    execution.Info = advanced.Info;
                    if (await execution.Update())
                        return Request.CreateResponse(HttpStatusCode.OK);
                    else
                    {
                        Logger logger = LogManager.GetCurrentClassLogger();
                        logger.Warn(string.Format("Не найдена запись курс. работы студента \"{0}\"."));
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Warn(e.ToString()); //запись лога с ошибкой
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        // PUT: api/CourseWork/5
        // изменить данные о курсовой работе (администратор, создатель)
        [HttpPut]
        [Route("api/CourseWork/{id}")]
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


        // DELETE: api/CourseWork/5
        // удалить курсовую работу (администратор, создатель)
        [HttpDelete]
        [Route("api/CourseWork/{id}")]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            CourseWork cWork = new CourseWork { ID = id };

            try
            {
                if (authProvider?.CheckPermission(Permission.LBWRK_COMMON_PERMISSION) ?? false)//проверка прав на операцию
                {
                    if (cWork.Delete()) //удаление
                        return Request.CreateResponse(HttpStatusCode.OK);
                    else
                    {
                        Logger logger = LogManager.GetCurrentClassLogger();
                        logger.Warn(string.Format("При удалении лаб. работы \"{0}\" произошла ошибка.", id)); //запись лога с ошибкой
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
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


        // удалить курсовую работу из плана (администратор, преподаватель)
        [HttpDelete]
        [Route("api/CourseWork/plan/{subjectId}/{workId}")]
        public async Task<HttpResponseMessage> DeletePlan(string subjectId, string workId)
        {
            string sqlQuery = "delete from CourseWorkPlan where SubjectGroupSemesterID=@subjectId and CourseWorkID=@workId";
            var parameters = new Dictionary<string, string>
            {
                {"@subjectId",subjectId },
                {"@workId",workId }
            };

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                //проверка наличия прав на операцию
                bool commonRight = default(bool),
                    teacherRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION),
                    () => teacherRight = authProvider.CheckPermission(Permission.CRSWRK_PERMISSION) ?
                                         authProvider.FlowsSubjects.Contains(subjectId) : false);

                try
                {
                    if (commonRight || teacherRight)
                    {
                        DB db = DB.GetInstance();
                        int result = db.ExecInsOrDelQuery(sqlQuery, parameters);
                        if (result == 1)
                            return Request.CreateResponse(HttpStatusCode.OK);
                        else
                        {
                            Logger logger = LogManager.GetCurrentClassLogger();
                            logger.Warn(string.Format("Не удалось удалить из плана по предмету \"{0}\" курсовую работу \"{1}\".",
                                subjectId, workId));//запись лога
                            return Request.CreateResponse(HttpStatusCode.NotFound);
                        }
                    }
                    else
                        return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
                catch (Exception e)
                {
                    Logger logger = LogManager.GetCurrentClassLogger();
                    logger.Fatal(e.ToString());//запись лога
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
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

        private async Task<string> getSubjectOfCourseWorkExec(string CWorkExecId)
        {
            string sqlQuery = "select dbo.GetSubjectOfCourseWorkExec(@id)";
            var parameters = new Dictionary<string, string>
            {
                {"@id",CWorkExecId }
            };
            DB db = DB.GetInstance();
            string result = (await db.ExecuteScalarQueryAsync(sqlQuery, parameters)).ToString();
            if (string.IsNullOrEmpty(result))
                return null;
            else
                return result;
        }
    }
}
