using ElJournal.DBInteract;
using ElJournal.Models;
using ElJournal.Providers;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ElJournal.Controllers
{
    public partial class CourseWorkController
    {
        // получить список процентовок курсовых работ (администратор)
        // получить список процентовок курсовых работ по предмету (все рег. пользователи)
        // получить выполненные процентовки курсовых работ студентом (все рег. пользователи)
        // добавить этап процентовки в план по курсовой работе (администратор, преподаватель)
        // поставить отметку о выполнении процентовки (администратор, преподаватель)
        // изменить данные процентовки курсовой работы (администратор, преподаватель)
        // удалить этап процентовки (преподаватель, администратор)


        // GET: api/CourseWork/stage?name=abc
        // получить список процентовок курсовых работ (администратор)
        [HttpGet]
        [Route("api/CourseWork/stage")]
        public async Task<HttpResponseMessage> GetStages([FromUri]string name = null)
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
                            response.Data = await CourseWorkStage.GetCollectionAsync();
                        else // если шаблон поиска был задан
                        {
                            Regex regex = new Regex(name);
                            response.Data = (await CourseWorkStage.GetCollectionAsync())
                                .Where(x => regex.IsMatch(x.Name)).ToList();
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

        
        // получить список процентовок курсовых работ по предмету (все рег. пользователи)
        [HttpGet]
        [Route("api/CourseWork/stage/{subjectId}")]
        public async Task<HttpResponseMessage> GetStagesBySubject(string subjectId)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            try
            {
                if (authProvider != null)
                {
                    response.Data = await CourseWorkStage.GetCWorkStagesFromSubject(subjectId);
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Info(e.ToString());//запись лога с ошибкой
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        // GET: api/CourseWork/Stage/5/5
        // получить выполненные процентовки курсовых работ студентом (все рег. пользователи)
        //TODO: метод еще пустой
        [HttpGet]
        [Route("api/CourseWork/stage/{studentId}/state")]
        public async Task<HttpResponseMessage> GetStageExecution(string studentId)
        {
            Response response = new Response();
            string sqlQuery = "select * from ExecutedCourseWorkStages(@studentId)";

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                try
                {
                    DB db = DB.GetInstance();

                    var parameters = new Dictionary<string, string>
                    {
                        { "@studentId", studentId }
                    };
                    var exec = await db.ExecSelectQueryAsync(sqlQuery, parameters);//поулчение данных из БД
                    response.Data = CourseWorkStageExecution.ToCourseWorkStages(exec); // преобразование данных в модель
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                catch (Exception e)
                {
                    Logger logger = LogManager.GetCurrentClassLogger();
                    logger.Fatal(e.ToString()); //запись лога с ошибкой
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }


        // POST: api/CourseWork/Stage/5
        // добавить этап процентовки в план по курсовой работе (администратор, преподаватель)
        [HttpPost]
        [Route("api/CourseWork/stage/{subjectId}")]
        public async Task<HttpResponseMessage> PostStage(string subjectId, [FromBody]CourseWorkStage stage)
        {
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

                if (commonRight || teacherRight)
                {
                    try
                    {
                        stage.SubjectGroupSemesterId = subjectId;
                        if (await stage.Push())
                            return Request.CreateResponse(HttpStatusCode.Created);
                        else
                        {
                            Logger logger = LogManager.GetCurrentClassLogger();
                            logger.Warn(string.Format("Этап процентовки курсовой работы не был добавлен. Название: \"{0}\".",
                                stage.Name));
                            return Request.CreateResponse(HttpStatusCode.Conflict);
                        }
                    }
                    catch(Exception e)
                    {
                        Logger logger = LogManager.GetCurrentClassLogger();
                        logger.Fatal(e.ToString());//запись лога с ошибкой
                        return Request.CreateResponse(HttpStatusCode.InternalServerError);
                    }
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }


        // POST: api/CourseWork/Stage/5/5
        // поставить отметку о выполнении процентовки (администратор, преподаватель)
        [HttpPost]
        [Route("api/CourseWork/stage/{studentId}/{stageId}")]
        public async Task<HttpResponseMessage> PostStage(string studentId, string stageId, [FromUri]bool state = true)
        {
            string procName1 = "dbo.AddCWorkStageExecution"; //процедура, добавляющая выполнение этапа процентовки
            string sqlQuery2 = "delete from CourseWorkStagesExecution " +   //удаление этапа процентовки
                "where CWStageID=@stageId and StudentGroupSemester=@studentId";
            var parameters = new Dictionary<string, string>
            {
                {"@studentId",studentId },
                {"@stageId",stageId }
            };

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            CourseWorkStage stage = await CourseWorkStage.GetInstanceAsync(stageId);
            if(stage==null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            try
            {
                //проверка наличия прав на операцию
                bool commonRight = default(bool),
                    teacherRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION),
                    () => teacherRight = authProvider.CheckPermission(Permission.CRSWRK_PERMISSION) ?
                                         authProvider.FlowsSubjects.Contains(stage.SubjectGroupSemesterId ?? string.Empty) : false);

                if(commonRight || teacherRight)
                {
                    DB db = DB.GetInstance();
                    if (state) //если нужно добавить факт сдачи
                    {
                        bool result = Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName1, parameters));
                        if (result)
                            return Request.CreateResponse(HttpStatusCode.Created);
                        else
                        {
                            Logger logger = LogManager.GetCurrentClassLogger();
                            logger.Warn(string.Format("Не удалось поставить процент курсовой \"{0}\" студенту \"{1}\".",
                                stageId, studentId));
                            return Request.CreateResponse(HttpStatusCode.BadRequest);
                        }
                    }
                    else //если нужно удалить существующий факт сдачи
                    {
                        int result = db.ExecInsOrDelQuery(sqlQuery2, parameters);
                        if (result == 1)
                            return Request.CreateResponse(HttpStatusCode.OK);
                        else
                        {
                            Logger logger = LogManager.GetCurrentClassLogger();
                            logger.Warn(string.Format("Процента курсовой \"{0}\" у студента \"{1}\" не существует. Result={2}.",
                                stageId, studentId, result));
                            return Request.CreateResponse(HttpStatusCode.BadRequest);
                        }
                    }
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        // PUT: api/CourseWork/Stage/5
        // изменить данные процентовки курсовой работы (администратор, преподаватель)
        [HttpPut]
        [Route("api/CourseWork/stage/{id}")]
        public async Task<HttpResponseMessage> PutStage(string id, [FromBody]CourseWorkStage stage)
        {
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
                                         authProvider.FlowsSubjects.Contains(stage.SubjectGroupSemesterId ?? string.Empty) : false);

                stage.ID = id;
                try
                {
                    if (await stage.Update())
                        return Request.CreateResponse(HttpStatusCode.OK);
                    else
                    {
                        Logger logger = LogManager.GetCurrentClassLogger();
                        logger.Warn(string.Format("Не удалось изменить процент курсовой \"{0}\".", stage.ID));
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                    }
                }
                catch(Exception e)
                {
                    Logger logger = LogManager.GetCurrentClassLogger();
                    logger.Fatal(e.ToString());//запись лога с ошибкой
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }


        // DELETE: api/CourseWork/Stage/5
        // удалить этап процентовки (преподаватель, администратор)
        [HttpDelete]
        [Route("api/CourseWork/stage/{id}")]
        public async Task<HttpResponseMessage> DeleteStage(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                var stage = await CourseWorkStage.GetInstanceAsync(id);
                
                //проверка наличия прав на операцию
                bool commonRight = default(bool),
                    teacherRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION),
                    () => teacherRight = authProvider.CheckPermission(Permission.CRSWRK_PERMISSION) ?
                                         authProvider.FlowsSubjects.Contains(stage?.SubjectGroupSemesterId ?? string.Empty) : false);

                try
                {
                    if(stage.Delete())
                        return Request.CreateResponse(HttpStatusCode.OK);
                    else
                    {
                        Logger logger = LogManager.GetCurrentClassLogger();
                        logger.Warn(string.Format("Не удалось удалить процент курсовой \"{0}\".", stage.ID));
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                    }
                }
                catch (Exception e)
                {
                    Logger logger = LogManager.GetCurrentClassLogger();
                    logger.Fatal(e.ToString());//запись лога с ошибкой
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }
    }
}