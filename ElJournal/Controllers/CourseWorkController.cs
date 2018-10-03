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
using System.IO;
using System.Net.Http.Headers;

namespace ElJournal.Controllers
{
    public partial class CourseWorkController : ApiController
    {
        // получить курсовые работы по предмету
        // получить статус выполнения курсовой работы студентом (да, нет)
        // добавить курсовую работу (администратор, преподаватель)
        // добавить курсовую работу в план по предмету (администратор, преподаватель)
        // установить курсовую работу студенту (администратор, преподаватель)
        // поставить отметки о выполнении курсовой работы (администратор, преподаватель)
        // изменить данные о курсовой работе (администратор, создатель)
        // удалить курсовую работу (администратор, создатель)
        // удалить курсовую работу из плана (администратор, преподаватель)


        // получить весь список курсовых (администратор)
        // получить конкретную курсовую по id (все)
        // добавить курсовую работу (преподаватель, администратор)
        // изменить курсовую работу (автор, администратор)
        // удалить курсовую работу (администратор)

        // получить файл, прикрепленный к курсовой работе (все рег. пользователи)
        // добавить файл к курсовой работе (автор, администратор)

        //получить список моих курсовых работ (все рег. пользователи)

        // получить темы курсовых по определенному предмету (все)
        // добавить курсовую в план (автор, администратор)
        // удалить курсовую работу из плана (преподаватель, администратор)

        // получить статус выполнения студентом курсовой работы (все рег. пользователи)
        // установить курсовую работу студенту (администратор, преподаватель)
        // поставить отметки о выполнении курсовой работы (администратор, преподаватель)
        // удалить установленную курсовую для студента (администратор, преподаватель)


        // GET: api/CourseWork?name=abc
        // получить весь список курсовых (администратор)
        [HttpGet]
        [Route("api/CourseWork")]
        public async Task<HttpResponseMessage> Get([FromUri]string name = null)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var works = await CourseWork.GetCollectionAsync();
            if (!string.IsNullOrEmpty(name)) // если шаблон для поиска задавался
            {
                Regex regex = new Regex(name);
                works = works.FindAll(x => regex.IsMatch(x.Name) || regex.IsMatch(x.Advanced));
            }

            response.Data = works;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        //получить список моих курсовых работ (все рег. пользователи)
        [HttpGet]
        [Route("api/CourseWork/my")]
        public async Task<HttpResponseMessage> GetMy()
        {
            Response response = new Response();
            
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null) //если пользователь не отправил токен получить ошибку 401
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var works = await CourseWork.GetCollectionAsync(authProvider.PersonId);
            response.Data = works;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // GET: api/CourseWork/5
        // получить конкретную курсовую по id (все)
        [HttpGet]
        [Route("api/CourseWork/{id}")]
        public async Task<HttpResponseMessage> GetById(string id)
        {
            Response response = new Response();

            var work = await CourseWork.GetInstanceAsync(id);
            response.Data = work;

            if (work != null)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // получить файл, прикрепленный к курсовой работе (все рег. пользователи)
        [HttpGet]
        [Route("api/CourseWork/file/{id}")]
        public async Task<HttpResponseMessage> GetFile(string id)
        {
            //авторизация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            CourseWork courseWork = await CourseWork.GetInstanceAsync(id);// получение лабораторной работы
            if (courseWork == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            string path = courseWork.FileURL + courseWork.FileName; // физический путь к файлу
            try
            {
                if (string.IsNullOrEmpty(path))//если к работе не приложен файл, сгенерируется exception
                    throw new FileNotFoundException("This coursework don't have attachment file");

                var stream = new FileStream(path, FileMode.Open);

                //отправка файла
                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(stream)
                };
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = courseWork.FileName
                };
                result.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                return result;
            }
            catch (FileNotFoundException e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Error(e.ToString()); //запись лога с ошибкой
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }


        // GET: api/CourseWork/BySubject/5
        // получить курсовые работы по предмету (все)
        [HttpGet]
        [Route("api/CourseWork/BySubject/{flowSubjectId}")]
        public async Task<HttpResponseMessage> GetBySubj(string flowSubjectId)
        {
            Response response = new Response();
            string sqlQuery = "select * from dbo.PlannedCourseWorks(@subjectId)";
            var parameters = new Dictionary<string, string>
            {
                {"subjectId",flowSubjectId }
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
        [Route("api/CourseWork/exec/{studentId}")]
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


        // добавить курсовую работу (преподаватель, администратор)
        [HttpPost]
        [Route("api/CourseWork")]
        public async Task<HttpResponseMessage> Post([FromBody]CourseWork courseWork)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null) //если пользователь не отправил токен получить ошибку 401
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //проверка наличия прав у пользователя
            bool commonRight = default(bool),
                teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION),
                            () => teacherRight = authProvider.CheckPermission(Permission.CRSWRK_PERMISSION));

            if (commonRight || teacherRight)
            {
                if (await courseWork.Push(authProvider.PersonId)) //добавление работы в БД
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // добавить файл к курсовой работе (автор, администратор)
        [HttpPost]
        [Route("api/CourseWork/file/{id}")]
        public async Task<HttpResponseMessage> Post(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск лабораторной работы
            CourseWork cwork = await CourseWork.GetInstanceAsync(id);
            if (cwork == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка прав пользователя
            bool authorRight = default(bool), commonRight = default(bool);
            Parallel.Invoke(() => authorRight = authProvider.LabWorks.Contains(id),
                () => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION));

            if (Request.Content.IsMimeMultipartContent() && (commonRight || authorRight))
            {
                //запись файла
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);
                var file = provider.Contents[0];
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                byte[] fileArray = await file.ReadAsByteArrayAsync();

                if (await cwork.AttachFile(fileArray, filename))
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
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
        [Route("api/CourseWork/exec/{flowSubjectId}/{workId}/{studentId}")]
        public async Task<HttpResponseMessage> PostExec(string flowSubjectId, string workId, string studentId)
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
                                             authProvider.FlowsSubjects.Contains(flowSubjectId) : false);

                    DB db = DB.GetInstance();
                    var parameters = new Dictionary<string, string>
                    {
                        {"@studentId",studentId },
                        {"@subjectId",flowSubjectId },
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
        [Route("api/CourseWork/exec/{studentId}")]
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
        // изменить курсовую работу (автор, администратор)
        [HttpPut]
        [Route("api/CourseWork/{id}")]
        public async Task<HttpResponseMessage> Put(string id, [FromBody]CourseWork courseWork)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            //проверка наличия прав пользователя
            bool right = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION) ||
                authProvider.CourseWorks.Contains(id);

            if (right)
            {
                courseWork.ID = id;
                if (await courseWork.Update()) //обновление записи в БД
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // DELETE: api/CourseWork/5
        // удалить курсовую работу (администратор)
        [HttpDelete]
        [Route("api/CourseWork/{id}")]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            CourseWork cWork = new CourseWork { ID = id };

            if (authProvider?.CheckPermission(Permission.LBWRK_COMMON_PERMISSION) ?? false)//проверка прав на операцию
            {
                if (cWork.Delete()) //удаление
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        [HttpDelete]
        [Route("api/CourseWork/file/{id}")]
        public async Task<HttpResponseMessage> DeleteFile(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            CourseWork work = await CourseWork.GetInstanceAsync(id);
            if (work == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка прав пользователя
            bool authorRight = default(bool), commonRight = default(bool);
            Parallel.Invoke(() => authorRight = authProvider.LabWorks.Contains(id),
                () => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION));

            if (commonRight || authorRight)
            {
                if (await work.DetachFile())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
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
