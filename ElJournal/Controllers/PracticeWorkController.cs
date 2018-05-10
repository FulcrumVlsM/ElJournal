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
    public class PracticeWorkController : ApiController
    {
        // получить весь список практических (все рег. пользователи)
        // получить список моих практических работ (все рег. пользователи)
        // получить план практических по определенному предмету (все)
        // получить список выполненных студентом практ работ по предмету (все рег. пользователи)
        // получить конкретную практическую по id (все)
        // получить файл, прикрепленный к практ работе (все рег. пользователи)
        // добавить практическую работу в план (автор, администратор)
        // установить практичскую работу из плана как выполненную студентом (преподаватель, администратор)
        // добавить практическую работу (преподаватель, администратор)
        // добавить файл к практической работе (автор, администратор)
        // изменить практическую работу (автор, администратор)
        // удалить практ работу из плана (преподаватель, администратор)
        // удалить факт выполнения практической работы (преподаватель, администратор)
        // удалить практическую работу (администратор)
        // удалить файл из практической работы (автор, администратор)

        // GET: api/PracticeWork
        // получить весь список практических (все рег. пользователи)
        public async Task<HttpResponseMessage> Get([FromUri]string name = null, [FromUri]int count = 50)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //token пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var works = await PracticeWork.GetCollectionAsync();

            if (!string.IsNullOrEmpty(name))// если шаблон для поиска не задавался
            {
                Regex regex = new Regex(name);
                works = works.FindAll(x => regex.IsMatch(x.Name) || regex.IsMatch(x.Advanced));
            }
            works = (works.Take(count)).ToList();

            response.Data = works;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // получить список моих практических работ (все рег. пользователи)
        [HttpGet]
        [Route("api/PracticeWork/my")]
        public async Task<HttpResponseMessage> GetMy()
        {
            Response response = new Response();

            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var works = await PracticeWork.GetCollectionAsync(authProvider.PersonId);
            response.Data = works;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // получить план практических по определенному предмету (все)
        [HttpGet]
        [Route("api/PracticeWork/plan/{flowSubjectId}")]
        public async Task<HttpResponseMessage> GetPlan(string flowSubjectId)
        {
            Response response = new Response();
            var list = await PracticeWorkPlan.GetCollectionAsync();
            list = list.FindAll(x => x.FlowSubjectId == flowSubjectId);
            response.Data = list;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // получить список выполненных студентом практ работ по предмету (все рег. пользователи)
        [HttpGet]
        [Route("api/PracticeWork/exec/{studentFlowId}/{subjectFlowId}")]
        public async Task<HttpResponseMessage> GetExec(string studentFlowId, string subjectFlowId)
        {
            Response response = new Response();
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск предмета
            FlowSubject fSubject = await FlowSubject.GetInstanceAsync(subjectFlowId);
            if (fSubject == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            var exLabs = await ExecutedPractWork.GetCollectionAsync(studentFlowId);
            exLabs = exLabs.FindAll(x =>
            {
                LabWorkPlan plan = (LabWorkPlan.GetInstanceAsync(x.PlanId)).Result;
                return plan.FlowSubjectId == fSubject.ID;
            });

            response.Data = exLabs;
            if (exLabs.Count > 0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NoContent, response);
        }


        // GET: api/PracticeWork/5
        // получить конкретную практическую по id (все)
        public async Task<HttpResponseMessage> Get(string id)
        {
            Response response = new Response();
            PracticeWork practWork = await PracticeWork.GetInstanceAsync(id);
            response.Data = practWork;
            if (practWork != null)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // получить файл, прикрепленный к практ работе (все рег. пользователи)
        [HttpGet]
        [Route("api/PracticeWork/file/{id}")]
        public async Task<HttpResponseMessage> GetFile(string id)
        {
            //авторизация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск лабораторной работы
            PracticeWork practWork = await PracticeWork.GetInstanceAsync(id);
            if (practWork == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            string path = practWork.FileURL + practWork.FileName; // физический путь к файлу
            try
            {
                if (string.IsNullOrEmpty(path))//если к работе не приложен файл, сгенерируется exception
                    throw new FileNotFoundException("This labwork don't have attachment file");

                var stream = new FileStream(path, FileMode.Open);

                //отправка файла
                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(stream)
                };
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = practWork.FileName
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


        // добавить практическую работу в план (автор, администратор)
        [HttpPost]
        [Route("api/PracticeWork/plan/{subjectFlowId}/{workId}")]
        public async Task<HttpResponseMessage> PostPlan(string subjectFlowId, string workId)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск предмета
            FlowSubject fSubject = await FlowSubject.GetInstanceAsync(subjectFlowId);
            if (fSubject == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //поиск лабораторной работы
            PracticeWork pract = await PracticeWork.GetInstanceAsync(workId);
            if (pract == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //Получение прав пользователя
            bool commonRight = default(bool);
            bool subjectRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION),
                () => subjectRight = authProvider.CheckPermission(Permission.LBWRK_PERMISSION) ?
                authProvider.FlowsSubjects.Contains(subjectFlowId) : false);

            if (commonRight || subjectRight)
            {
                PracticeWorkPlan plan = new PracticeWorkPlan
                {
                    FlowSubjectId = subjectFlowId,
                    Work = pract
                };
                if (await plan.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // установить практичскую работу из плана как выполненную студентом (преподаватель, администратор)
        [HttpPost]
        [Route("api/PracticeWork/exec")]
        public async Task<HttpResponseMessage> PostExec([FromBody]ExecutedPractWork executedPract)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            PracticeWorkPlan plan = await PracticeWorkPlan.GetInstanceAsync(executedPract.PlanId);
            if (plan == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //Получение прав пользователя
            bool commonRight = default(bool);
            bool subjectRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION),
                () => subjectRight = authProvider.CheckPermission(Permission.LBWRK_PERMISSION) ?
                authProvider.FlowsSubjects.Contains(plan.FlowSubjectId) : false);

            if (commonRight || subjectRight)
            {
                if (await executedPract.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // POST: api/PracticeWork
        // добавить практическую работу (преподаватель, администратор)
        [HttpPost]
        [Route("api/PracticeWork")]
        public async Task<HttpResponseMessage> Post([FromBody]PracticeWork work)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //проверка наличия прав у пользователя
            bool commonRight = default(bool),
                teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION),
                            () => teacherRight = authProvider.CheckPermission(Permission.LBWRK_PERMISSION));

            if (commonRight || teacherRight)
            {
                if (await work.Push(authProvider.PersonId)) //добавление работы в БД
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // добавить файл к практической работе (автор, администратор)
        [HttpPost]
        [Route("api/PracticeWork/file")]
        public async Task<HttpResponseMessage> Post(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск лабораторной работы
            PracticeWork work = await PracticeWork.GetInstanceAsync(id);
            if (work == null)
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
                work.FileName = filename;

                if (await work.AttachFile(fileArray))
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // PUT: api/PracticeWork/5
        // изменить практическую работу (автор, администратор)
        public async Task<HttpResponseMessage> Put(string id, [FromBody]PracticeWork work)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //проверка наличия прав пользователя
            bool right = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION) ||
                authProvider.LabWorks.Contains(id);

            if (right)
            {
                work.ID = id;
                if (await work.Update()) //обновление записи в БД
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // удалить практ работу из плана (преподаватель, администратор)
        [HttpDelete]
        [Route("api/PracticeWork/plan/{id}")]
        public async Task<HttpResponseMessage> DeletePlan(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск лабораторной работы из плана
            PracticeWorkPlan plan = await PracticeWorkPlan.GetInstanceAsync(id);
            if (plan == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //Получение прав пользователя
            bool commonRight = default(bool);
            bool subjectRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION),
                () => subjectRight = authProvider.CheckPermission(Permission.LBWRK_PERMISSION) ?
                authProvider.FlowsSubjects.Contains(plan?.FlowSubjectId) : false);

            if (commonRight || subjectRight)
            {
                if (plan.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // удалить факт выполнения практической работы (преподаватель, администратор)
        [HttpDelete]
        [Route("api/PracticeWork/exec/{id}")]
        public async Task<HttpResponseMessage> DeleteExec(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск выполненной лабораторной работы
            ExecutedPractWork exPract = await ExecutedPractWork.GetInstanceAsync(id);
            if (exPract == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //поиск лабораторной работы из плана
            PracticeWorkPlan plan = await PracticeWorkPlan.GetInstanceAsync(exPract.PlanId);

            //Получение прав пользователя
            bool commonRight = default(bool);
            bool subjectRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION),
                () => subjectRight = authProvider.CheckPermission(Permission.LBWRK_PERMISSION) ?
                authProvider.FlowsSubjects.Contains(plan?.FlowSubjectId) : false);

            if (commonRight || subjectRight)
            {
                if (exPract.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // DELETE: api/PracticeWork/5
        [HttpDelete]
        [Route("api/PracticeWork/{id}")]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск практической работы
            PracticeWork lab = await PracticeWork.GetInstanceAsync(id);
            if (lab == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            if (authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION))//проверка прав на операцию
            {
                if (lab.Delete()) //удаление
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                throw new HttpResponseException(HttpStatusCode.Forbidden);
        }


        // удалить файл из практической работы (автор, администратор)
        [HttpDelete]
        [Route("api/PracticeWork/file")]
        public async Task<HttpResponseMessage> DeleteFile(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            PracticeWork work = await PracticeWork.GetInstanceAsync(id);
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
    }
}
