using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Models;
using ElJournal.DBInteract;
using ElJournal.Providers;
using System.IO;
using System.Web;
using System.Configuration;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using NLog;

namespace ElJournal.Controllers
{
    //develop: Mikhail
    public class LabWorkController : ApiController
    {
        // получить весь список лабораторных (администратор, администратор кафедры, администратор факультета)
        // получить конкретную лабораторную по id (все)
        // получить файл, прикрепленный к лаб работе (все рег. пользователи)
        // получить список моих лабораторных работ (все рег. пользователи)
        // добавить лабораторную работу (преподаватель, администратор)
        // добавить файл к лабораторной работе (автор, администратор)
        // изменить лабораторную работу (автор, администратор)
        // удалить лабораторную работу (администратор)
        // удалить файл из лабораторной работы (автор, администратор)

        // получить план лабораторных по определенному предмету (все)
        // добавить лабораторную работу в план (автор, администратор)
        // удалить лаб работу из плана (преподаватель, администратор)

        // получить список выполненных студентом лаб работ по предмету (все рег. пользователи)
        // установить лаб работу из плана как выполненную студентом (преподаватель, администратор)
        // удалить факт выполнения лабораторной работы (преподаватель, администратор)


        // GET: api/LabWork
        // получить весь список лабораторных (все рег. пользователи)
        public async Task<HttpResponseMessage> Get([FromUri]string name = null, [FromUri]int count=50)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //token пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var works = await LabWork.GetCollectionAsync();
            works = (works.Take(count)).ToList();

            if (!string.IsNullOrEmpty(name))// если шаблон для поиска не задавался
            {
                Regex regex = new Regex(name);
                works = works.FindAll(x => regex.IsMatch(x.Name) || regex.IsMatch(x.Advanced));
            }

            response.Data = works;
            if (works.Count > 0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NoContent, response);
        }


        // GET: api/LabWork/my
        // получить список моих лабораторных работ (все рег. пользователи)
        [HttpGet]
        [Route("api/LabWork/my")]
        public async Task<HttpResponseMessage> GetMy()
        {
            Response response = new Response();

            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if(authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            var works = await LabWork.GetCollectionAsync();
            works = works.FindAll(x => x.AuthorId == authProvider.PersonId);
            response.Data = works;

            if(works.Count>0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NoContent, response);
        }


        // GET: api/LabWork/plan/5
        // получить план лабораторных по определенному предмету (все)
        [HttpGet]
        [Route("api/LabWork/plan/{flowSubjectId}")]
        public async Task<HttpResponseMessage> GetPlan(string flowSubjectId)
        {
            Response response = new Response();
            var list = await LabWorkPlan.GetCollectionAsync();
            list = list.FindAll(x => x.FlowSubjectId == flowSubjectId);
            response.Data = list;
            if (list.Count > 0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NoContent, response);
        }


        // GET: api/LabWork/exec/5/5
        // получить список выполненных студентом лаб работ по предмету (все рег. пользователи)
        [HttpGet]
        [Route("api/LabWork/exec/{studentFlowId}/{subjectFlowId}")]
        public async Task<HttpResponseMessage> GetExec(string studentFlowId, string subjectFlowId)
        {
            Response response = new Response();
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск предмета
            FlowSubject fSubject = await FlowSubject.GetInstanceAsync(subjectFlowId);
            if(fSubject == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            var exLabs = await ExecutedLabWork.GetCollectionAsync();
            exLabs = exLabs.FindAll(x =>
            {
                LabWorkPlan plan = (LabWorkPlan.GetInstanceAsync(x.PlanId)).Result;
                return (plan.FlowSubjectId == subjectFlowId) && (x.StudentFlowSubjectId == studentFlowId);
            });

            response.Data = exLabs;
            if(exLabs.Count>0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NoContent, response);
        }


        // GET: api/LabWork/5
        // получить конкретную лабораторную по id (все)
        [HttpGet]
        [Route("api/LabWork/{id}")]
        public async Task<HttpResponseMessage> GetConcrete(string id)
        {
            Response response = new Response();
            LabWork labWork = await LabWork.GetInstanceAsync(id);
            response.Data = labWork;
            if (labWork != null)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // GET: api/LabWork/file/5
        // получить файл, прикрепленный к лаб работе (все рег. пользователи)
        [HttpGet]
        [Route("api/LabWork/file/{id}")]
        public async Task<HttpResponseMessage> GetFile(string id)
        {
            //авторизация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            LabWork labWork = await LabWork.GetInstanceAsync(id);// получение лабораторной работы

            string path = labWork.FileURL + labWork.FileName; // физический путь к файлу
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
                    FileName = labWork.FileName
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


        // POST: api/LabWork/plan/5/5
        // добавить лабораторную работу в план (автор, администратор)
        [HttpPost]
        [Route("api/LabWork/plan/{subjectFlowId}/{workId}")]
        public async Task<HttpResponseMessage> PostPlan(string subjectFlowId, string workId)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if(authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск предмета
            FlowSubject fSubject = await FlowSubject.GetInstanceAsync(subjectFlowId);
            if (fSubject == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //поиск лабораторной работы
            LabWork lab = await LabWork.GetInstanceAsync(workId);
            if (lab == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //Получение прав пользователя
            bool commonRight = default(bool);
            bool subjectRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION),
                () => subjectRight = authProvider.CheckPermission(Permission.LBWRK_PERMISSION) ?
                authProvider.FlowsSubjects.Contains(subjectFlowId) : false);

            if (commonRight || subjectRight)
            {
                LabWorkPlan plan = new LabWorkPlan
                {
                    FlowSubjectId = subjectFlowId,
                    labWork = lab
                };
                if (await plan.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // POST: api/LabWork/exec/5/5?state=true
        // установить лаб работу из плана как выполненную студентом (преподаватель, администратор)
        [HttpPost]
        [Route("api/LabWork/exec/{studentId}/{workId}/{subjectGroupId}")]
        public async Task<HttpResponseMessage> PostExec([FromBody]ExecutedLabWork exLab)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            LabWorkPlan plan = await LabWorkPlan.GetInstanceAsync(exLab.PlanId);
            if(plan == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //Получение прав пользователя
            bool commonRight = default(bool);
            bool subjectRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION),
                () => subjectRight = authProvider.CheckPermission(Permission.LBWRK_PERMISSION) ?
                authProvider.FlowsSubjects.Contains(plan.FlowSubjectId) : false);

            if (commonRight || subjectRight)
            {
                if (await exLab.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // POST: api/LabWork
        // добавить лабораторную работу (преподаватель, администратор)
        public async Task<HttpResponseMessage> Post([FromBody]LabWork lab)
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
                if (await lab.Push(authProvider.PersonId)) //добавление работы в БД
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // POST: api/LabWork/5
        // добавить файл к лабораторной работе (автор, администратор)
        [HttpPost]
        [Route("api/LabWork/file/{id}")]
        public async Task<HttpResponseMessage> Post(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск лабораторной работы
            LabWork lab = await LabWork.GetInstanceAsync(id);
            if(lab == null)
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
                lab.FileName = filename;

                if (await lab.AttachFile(fileArray))
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                throw new HttpResponseException(HttpStatusCode.Forbidden);
        }


        // PUT: api/LabWork/5
        // измененить лабораторную работу (автор, администратор)
        public async Task<HttpResponseMessage> Put(string id, [FromBody]LabWork lab)
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
                lab.ID = id;
                if (await lab.Update()) //обновление записи в БД
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // DELETE: api/LabWork/plan/5
        // удалить лаб работу из плана (преподаватель, администратор)
        [HttpDelete]
        [Route("api/LabWork/plan/{id}")]
        public async Task<HttpResponseMessage> DeletePlan(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск лабораторной работы из плана
            LabWorkPlan plan = await LabWorkPlan.GetInstanceAsync(id);
            if(plan == null)
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


        // удалить факт выполнения лабораторной работы (преподаватель, администратор)
        [HttpDelete]
        [Route("api/LabWork/exec/{id}")]
        public async Task<HttpResponseMessage> DeleteExec(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск выполненной лабораторной работы
            ExecutedLabWork exLab = await ExecutedLabWork.GetInstanceAsync(id);
            if(exLab == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //поиск лабораторной работы из плана
            LabWorkPlan plan = await LabWorkPlan.GetInstanceAsync(exLab.PlanId);

            //Получение прав пользователя
            bool commonRight = default(bool);
            bool subjectRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION),
                () => subjectRight = authProvider.CheckPermission(Permission.LBWRK_PERMISSION) ?
                authProvider.FlowsSubjects.Contains(plan?.FlowSubjectId) : false);

            if (commonRight || subjectRight)
            {
                if (exLab.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // DELETE: api/LabWork/5
        // удалить лабораторную работу (администратор)
        [HttpDelete]
        [Route("api/LabWork/{id}")]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if(authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск лабораторной работы
            LabWork lab = await LabWork.GetInstanceAsync(id);
            if(lab==null)
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


        // удалить файл из лабораторной работы (автор, администратор)
        [HttpDelete]
        [Route("api/LabWork/file/{id}")]
        public async Task<HttpResponseMessage> DeleteFile(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            LabWork work = await LabWork.GetInstanceAsync(id);
            if(work == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка прав пользователя
            bool authorRight = default(bool), commonRight = default(bool);
            Parallel.Invoke(() => authorRight = authProvider.LabWorks.Contains(id),
                () => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION));

            if(commonRight || authorRight)
            {
                if(await work.DetachFile())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}
