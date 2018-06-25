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
        // получить этап процентовки по id (все)
        // получить выполненные процентовки курсовых работ студентом (все рег. пользователи)
        // добавить этап процентовки в план по курсовой работе (администратор, преподаватель)
        // поставить отметку о выполнении процентовки (администратор, преподаватель)
        // изменить данные процентовки курсовой работы (администратор, преподаватель)
        // удалить этап процентовки (преподаватель, администратор)


        // получить список процентовок курсовых работ (администратор)
        [HttpGet]
        [Route("api/CourseWork/stage")]
        public async Task<HttpResponseMessage> GetStages([FromUri]string name = null)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            if (authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION)) //если есть права доступа
            {
                var stages = await CourseWorkStage.GetCollectionAsync();
                if (!string.IsNullOrEmpty(name))
                {
                    Regex regex = new Regex(name);
                    stages = stages.FindAll(x => regex.IsMatch(x.Name));
                }
                response.Data = stages;
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }

        
        // получить список процентовок курсовых работ по предмету (все рег. пользователи)
        [HttpGet]
        [Route("api/CourseWork/stage/subject/{subjectId}")]
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


        // получить этап процентовки по id (все)
        [HttpGet]
        [Route("api/CourseWork/stage/{id}")]
        public async Task<HttpResponseMessage> GetStageConcrete(string id)
        {
            Response response = new Response();
            var stage = await CourseWorkStage.GetInstanceAsync(id);
            response.Data = stage;
            if (stage != null)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        // GET: api/CourseWork/Stage/5/5
        // получить выполненные процентовки курсовых работ студентом (все рег. пользователи)
        //TODO: метод еще пустой
        [HttpGet]
        [Route("api/CourseWork/stage/{executionId}/state")]
        public async Task<HttpResponseMessage> GetStageExecution(string executionId)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var execList = await CourseWorkStage.GetExecuted(executionId);
            response.Data = execList;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // POST: api/CourseWork/Stage/5
        // добавить этап процентовки в план по курсовой работе (администратор, преподаватель)
        [HttpPost]
        [Route("api/CourseWork/stage/{flowSubjectId}")]
        public async Task<HttpResponseMessage> PostStage(string flowSubjectId, [FromBody]CourseWorkStage stage)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //проверка на существование указанного предмета
            FlowSubject subject = await FlowSubject.GetInstanceAsync(flowSubjectId);
            if (subject == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав на операцию
            bool commonRight = default(bool),
                teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION),
                () => teacherRight = authProvider.CheckPermission(Permission.CRSWRK_PERMISSION) ?
                                     authProvider.FlowsSubjects.Contains(flowSubjectId) : false);

            if (commonRight || teacherRight)
            {
                stage.FlowSubjectId = subject.ID;
                if (await stage.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // POST: api/CourseWork/Stage/5/5
        // поставить отметку о выполнении процентовки (администратор, преподаватель)
        [HttpPost]
        [Route("api/CourseWork/stage/{executionId}/{stageId}")]
        public async Task<HttpResponseMessage> PostStage(string executionId, string stageId, [FromUri]bool state = true)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //проверка существования факта выполнения КР и этапа процентовки
            CourseWorkStage stage = await CourseWorkStage.GetInstanceAsync(stageId);
            CourseWorkExecution execution = await CourseWorkExecution.GetInstanceAsync(executionId);
            if(execution == null || stage == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав на операцию
            bool commonRight = default(bool),
                teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION),
                () => teacherRight = authProvider.CheckPermission(Permission.CRSWRK_PERMISSION) ?
                                     authProvider.FlowsSubjects.Contains(stage.FlowSubjectId ?? string.Empty) : false);

            if (commonRight || teacherRight)
            {
                CourseWorkStageExecution cwExecution = new CourseWorkStageExecution();
                cwExecution.CourseWorkExecutionId = execution.ID;
                cwExecution.CourseWorkStageId = stage.ID;

                if (state) //если нужно добавить факт сдачи
                {
                    if (await cwExecution.Push())
                        return Request.CreateResponse(HttpStatusCode.OK);
                    else
                        return Request.CreateResponse(HttpStatusCode.Conflict);
                }
                else //если нужно удалить существующий факт сдачи
                {
                    if (cwExecution.Delete)
                        return Request.CreateResponse(HttpStatusCode.OK);
                    else
                        return Request.CreateResponse(HttpStatusCode.Conflict);
                }
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
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
            if(authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //проверка наличия прав на операцию
            bool commonRight = default(bool),
                teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION),
                () => teacherRight = authProvider.CheckPermission(Permission.CRSWRK_PERMISSION) ?
                                     authProvider.FlowsSubjects.Contains(stage.FlowSubjectId ?? string.Empty) : false);

            if (commonRight || teacherRight)
            {
                stage.ID = id;
                if (await stage.Update())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
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
            if(authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var stage = await CourseWorkStage.GetInstanceAsync(id);
            if (stage == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав на операцию
            bool commonRight = default(bool),
                teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION),
                () => teacherRight = authProvider.CheckPermission(Permission.CRSWRK_PERMISSION) ?
                                     authProvider.FlowsSubjects.Contains(stage?.FlowSubjectId ?? string.Empty) : false);

            if (commonRight || teacherRight)
            {
                if (stage.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}