using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Models;
using ElJournal.Providers;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Http.Headers;
using NLog;
using ElJournal.DBInteract;

namespace ElJournal.Controllers
{
    public class IndependentWorkController : ApiController
    {
        // получить весь список самостоятельных работ (администратор, администратор кафедры)
        // получить самостоятельную работу по id (все)
        // получить план самостоятельных работ по предмету (все)
        // получить состояние выполнения студентом (все рег. пользователи)
        // добавить самостоятельную работу (преподаватель, администратор)
        // добавить самостоятельную работу в план (преподаватель, администратор)
        // установить самостоятельную работу студенту (преподаватель, администратор)
        // установить выполнение самостоятельной работы студентом (преподаватель, администратор)
        // изменить самостоятельную работу (автор, администратор)
        // удалить самостоятельную работу из плана (преподаватель, администратор)
        // удалить самостоятельную работу (администратор)


        // GET: api/IndependentWork
        // получить весь список самостоятельных работ (администратор, администратор кафедры)
        [HttpGet]
        [Route("api/IndependentWork")]
        public async Task<HttpResponseMessage> Get([FromUri]string name=null)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var works = await IndependentWork.GetCollectionAsync();
            if (!string.IsNullOrEmpty(name)) // если шаблон для поиска задавался
            {
                Regex regex = new Regex(name);
                works = works.FindAll(x => regex.IsMatch(x.Name) || regex.IsMatch(x.Advanced));
            }

            response.Data = works;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        [HttpGet]
        [Route("api/IndependentWork/my")]
        public async Task<HttpResponseMessage> GetMy()
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null) //если пользователь не отправил токен получить ошибку 401
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var works = await IndependentWork.GetCollectionAsync(authProvider.PersonId);
            response.Data = works;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // GET: api/IndependentWork/5
        // получить самостоятельную работу по id (все)
        [HttpGet]
        [Route("api/IndependentWork/{id}")]
        public async Task<HttpResponseMessage> GetConcrete(string id)
        {
            Response response = new Response();

            var work = await IndependentWork.GetInstanceAsync(id);
            response.Data = work;

            if (work != null)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // получить файл, прикрепленный к самостоятельной работе (все рег. пользователи)
        [HttpGet]
        [Route("api/IndependentWork/file/{id}")]
        public async Task<HttpResponseMessage> GetFile(string id)
        {
            //авторизация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            IndependentWork indWork = await IndependentWork.GetInstanceAsync(id);// получение самостоятельной работы
            if (indWork == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            string path = indWork.FileURL + indWork.FileName; // физический путь к файлу
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
                    FileName = indWork.FileName
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


        // GET: api/IndependentWork/Plan/5
        // получить план самостоятельных работ по предмету (все)
        [HttpGet]
        [Route("api/IndependentWork/plan/{flowSubjectId}")]
        public async Task<HttpResponseMessage> GetPlan(string flowSubjectId)
        {
            Response response = new Response();
            var workPlan = await IndependentWorkPlan.GetCollectionAsync(flowSubjectId);
            response.Data = workPlan;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // GET: api/IndependentWork/Exec/5
        // получить состояние выполнения студентом (все рег. пользователи)
        [HttpGet]
        [Route("api/IndependentWork/exec/{studentFlowSubjectId}")]
        public async Task<dynamic> GetExec(string studentFlowSubjectId)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if(authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            response.Data = await IndependentWorkExecution.GetInstanceAsync(studentFlowSubjectId);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // POST: api/IndependentWork
        // добавить самостоятельную работу (преподаватель, администратор)
        [HttpPost]
        [Route("api/IndependentWork")]
        public async Task<dynamic> Post([FromBody]IndependentWork independentWork)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null) //если пользователь не отправил токен получить ошибку 401
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //проверка наличия прав у пользователя
            bool commonRight = default(bool),
                teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.INDWRK_COMMON_PERMISSION),
                            () => teacherRight = authProvider.CheckPermission(Permission.INDWRK_PERMISSION));

            if (commonRight || teacherRight)
            {
                if (await independentWork.Push(authProvider.PersonId)) //добавление работы в БД
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // добавить файл к курсовой работе (автор, администратор)
        [HttpPost]
        [Route("api/IndependentWork/file/{id}")]
        public async Task<HttpResponseMessage> Post(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск лабораторной работы
            IndependentWork indWork = await IndependentWork.GetInstanceAsync(id);
            if (indWork == null)
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

                if (await indWork.AttachFile(fileArray, filename))
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // POST: api/IndependentWork/Plan/5/5
        // добавить самостоятельную работу в план (преподаватель, администратор)
        [HttpPost]
        [Route("api/IndependentWork/plan/{flowSubjectId}/{workId}")]
        public async Task<HttpResponseMessage> PostPlan(string flowSubjectId, string workId)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null) //если пользователь не отправил токен получить ошибку 401
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //проверка наличия прав у пользователя
            bool commonRight = default(bool),
                teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.INDWRK_COMMON_PERMISSION),
                            () => teacherRight = authProvider.CheckPermission(Permission.INDWRK_PERMISSION) ?
                                             authProvider.FlowsSubjects.Contains(flowSubjectId) : false);

            if (commonRight || teacherRight)
            {
                var plan = new IndependentWorkPlan
                {
                    FlowSubjectId = flowSubjectId,
                    ID = workId
                };

                if (await plan.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // POST: api/IndependentWork/Plan/5/5
        // установить самостоятельную работу студенту (преподаватель, администратор)
        [HttpPost]
        [Route("api/IndependentWork/exec/{studentId}/{workPlanId}")]
        public async Task<HttpResponseMessage> PostExec(string studentId, string workPlanId, 
            [FromBody]IndependentWorkExecution execution)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null) //если пользователь не отправил токен получить ошибку 401
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск работы в плане
            var plan = await IndependentWorkPlan.GetInstanceAsync(workPlanId);
            if (plan == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав у пользователя
            bool commonRight = default(bool),
                teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.INDWRK_COMMON_PERMISSION),
                            () => teacherRight = authProvider.CheckPermission(Permission.INDWRK_PERMISSION) ?
                                             authProvider.FlowsSubjects.Contains(plan.FlowSubjectId) : false);

            if (commonRight || teacherRight)
            {
                execution.IndependentWorkPlanId = workPlanId;
                execution.StudentFlowSubjectId = studentId;
                execution.State = false;

                if (await execution.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // POST: api/IndependentWork/Exec/5
        // установить выполнение самостоятельной работы студентом (преподаватель, администратор)
        [HttpPost]
        [Route("api/IndependentWork/exec/{studentId}")]
        public async Task<HttpResponseMessage> PostExec(string studentId, [FromBody]IndependentWorkExecution exec, 
            [FromUri]bool state = true)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null) //если пользователь не отправил токен получить ошибку 401
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var execution = await IndependentWorkExecution.GetInstanceAsync(studentId);
            if (execution == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            var plan = await IndependentWorkPlan.GetInstanceAsync(execution.IndependentWorkPlanId);
            if (plan == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав у пользователя
            bool commonRight = default(bool),
                teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.INDWRK_COMMON_PERMISSION),
                            () => teacherRight = authProvider.CheckPermission(Permission.INDWRK_PERMISSION) ?
                                             authProvider.FlowsSubjects.Contains(plan.FlowSubjectId) : false);

            if (commonRight || teacherRight)
            {
                execution.State = state;
                execution.Info = exec.Info;

                if (await execution.Update())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // PUT: api/IndependentWork/5
        // изменить самостоятельную работу (автор, администратор)
        [HttpPut]
        [Route("api/IndependentWork/{id}")]
        public async Task<HttpResponseMessage> Put(string id, [FromBody]IndependentWork independentWork)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //проверка наличия прав пользователя
            bool right = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION) ||
                authProvider.IndependentWorks.Contains(id);

            if (right)
            {
                independentWork.ID = id;
                if (await independentWork.Update()) //обновление записи в БД
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // DELETE: api/IndependentWork/Plan/5
        // удалить самостоятельную работу из плана (преподаватель, администратор)
        [HttpDelete]
        [Route("api/IndependentWork/plan/{id}")]
        public async Task<HttpResponseMessage> DeletePlan(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск работы из плана
            var plan = await IndependentWorkPlan.GetInstanceAsync(id);
            if (plan == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав на операцию
            bool commonRight = default(bool),
                teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.CRSWRK_COMMON_PERMISSION),
                () => teacherRight = authProvider.CheckPermission(Permission.CRSWRK_PERMISSION) ?
                                     authProvider.FlowsSubjects.Contains(plan.ID) : false);

            if (commonRight || teacherRight)
            {
                if (plan.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        [HttpDelete]
        [Route("api/IndependentWork/file/{id}")]
        public async Task<HttpResponseMessage> DeleteFile(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            IndependentWork work = await IndependentWork.GetInstanceAsync(id);
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


        // DELETE: api/IndependentWork/5
        // удалить самостоятельную работу (администратор)
        [HttpDelete]
        [Route("api/IndependentWork/{id}")]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            IndependentWork work = new IndependentWork { ID = id };

            if (authProvider?.CheckPermission(Permission.LBWRK_COMMON_PERMISSION) ?? false)//проверка прав на операцию
            {
                if (work.Delete()) //удаление
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}
