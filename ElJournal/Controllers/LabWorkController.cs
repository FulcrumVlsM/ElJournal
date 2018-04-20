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

        string fileURL = ConfigurationManager.AppSettings["FileStorage"];


        // GET: api/LabWork
        // получить весь список лабораторных (администратор)
        public async Task<IHttpActionResult> Get([FromUri]string name = null)
        {
            Response response = new Response();
            string token = Request?.Headers?.Authorization?.Scheme; //token пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider == null) //клиент получит 401, если не добавит токен в запрос
                return Unauthorized(null);

            if (authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION))// требуются права адмниистратора
            {
                try
                {
                    if (string.IsNullOrEmpty(name))// если шаблон для поиска не задавался
                    {
                        response.Data = await LabWork.GetCollectionAsync();
                    }
                    else // если шаблон поиска был задан
                    {
                        Regex regex = new Regex(name);
                        response.Data = (await LabWork.GetCollectionAsync())
                            .Where(x => regex.IsMatch(x.Name) || regex.IsMatch(x.Advanced)).ToList();
                    }
                }
                catch (Exception e)
                {
                    Logger logger = LogManager.GetCurrentClassLogger();
                    logger.Info(e.ToString());
                    return InternalServerError();
                }

                return Ok(response);
            }
            else
                throw new HttpResponseException(HttpStatusCode.Forbidden);
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
            string sqlQuery = "select ID, name from LabWorks where authorID=@authorId";
            var parameters = new Dictionary<string, string>();

            if (authProvider != null)
            {
                try
                {
                    DB db = DB.GetInstance();
                    parameters.Add("@authorId", authProvider.PersonId);
                    var my = await db.ExecSelectQueryAsync(sqlQuery, parameters);//получение списка из БД
                    response.Data = LabWork.ToLabWork(my); //преобразование списка к модели
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                catch(Exception e)
                {
                    Logger logger = LogManager.GetCurrentClassLogger();
                    logger.Fatal(e.ToString()); //запись лога с ошибкой
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }


        // GET: api/LabWork/plan/5
        // получить план лабораторных по определенному предмету (все)
        [HttpGet]
        [Route("api/LabWork/plan/{flowSubjectId}")]
        public async Task<HttpResponseMessage> GetPlan(string flowSubjectId)
        {
            Response response = new Response();

            try
            {
                var list = await LabWorkPlan.GetCollectionAsync();
                list = list.FindAll(x => x.FlowSubjectId == flowSubjectId);
                response.Data = list;
                if(list.Count>0)
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                else
                    return Request.CreateResponse(HttpStatusCode.NoContent, response);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString()); //запись лога с ошибкой
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
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
        public async Task<dynamic> GetConcrete(string id)
        {
            Response response = new Response();
            try
            {
                response.Data = await LabWork.GetInstanceAsync(id);
                return Ok(response);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString()); //запись лога с ошибкой
                return InternalServerError();
            }
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

            if (authProvider != null)
            {
                LabWork labWork = await LabWork.GetInstanceAsync(id);// получение лабораторной работы

                string path = labWork.FileURL + labWork.FileName; // физический путь к файлу
                try
                {
                    if (string.IsNullOrEmpty(path))//если к работе не приложен файл, сгенерируется exception
                        throw new FileNotFoundException("This labwork don't have attachment file");

                    var stream = new FileStream(path, FileMode.Open);

                    //отправка файла
                    var result = new HttpResponseMessage(HttpStatusCode.OK);
                    result.Content = new StreamContent(stream);
                    result.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("attachment")
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
            else
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
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
        public async Task<IHttpActionResult> Post([FromBody]LabWork lab)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null) //если пользователь не отправил токен получить ошибку 401
                return Unauthorized(null);

            try
            {
                //проверка наличия прав у пользователя
                bool commonRight=default(bool),
                    teacherRight=default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION),
                                () => teacherRight = authProvider.CheckPermission(Permission.LBWRK_PERMISSION));

                if (commonRight || teacherRight)
                {
                    if (await lab.Push(authProvider.PersonId)) //добавление работы в БД
                    {
                        var id = await getLab(lab.Name, lab.Advanced);
                        response.Data = new { ID = id };
                        return Created(string.Format("api/LabWork/{0}", id), response);
                    }
                    else
                    {
                        Logger logger = LogManager.GetCurrentClassLogger();
                        logger.Warn(string.Format("Не удалось добавить лабораторную работу. User: {0}, name: {1}",
                            authProvider.PersonId, lab.Name)); //запись лога с ошибкой
                        return Conflict();
                    }
                }
                else
                    throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString()); //запись лога с ошибкой
                return InternalServerError();
            }
        }


        // POST: api/LabWork/5
        // добавить файл к лабораторной работе (автор, администратор)
        [HttpPost]
        [Route("api/LabWork/file/{id}")]
        public async Task<IHttpActionResult> Post(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            bool authorRight = default(bool), commonRight = default(bool);

            //проверка прав пользователя
            Parallel.Invoke(() => authorRight = authProvider.LabWorks.Contains(id),
                () => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION));

            if (Request.Content.IsMimeMultipartContent() && (commonRight || authorRight))
            {
                //запись файла
                var provider = new MultipartMemoryStreamProvider();
                string root = fileURL;
                await Request.Content.ReadAsMultipartAsync(provider);
                var file = provider.Contents[0];
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                byte[] fileArray = await file.ReadAsByteArrayAsync();

                try
                {
                    using (FileStream fs = new FileStream(root + filename, FileMode.CreateNew))
                    {
                        await fs.WriteAsync(fileArray, 0, fileArray.Length);
                    }
                }
                catch (IOException)// если файл с таким именем есть на сервере, возникнет конфликт
                {
                    Logger logger = LogManager.GetCurrentClassLogger();
                    logger.Warn(string.Format("Конфликт имени файла при добавлении на сервер. {0}",filename)); //запись лога с ошибкой
                    return Conflict();
                }

                //запись в бд ссылки на файл
                DB db = DB.GetInstance();
                string procName = "dbo.UpdateLabWork";
                var parameters = new Dictionary<string, string>();
                parameters.Add("@ID", id);
                parameters.Add("@fileName", filename);
                parameters.Add("@fileUrl", fileURL);
                bool result = Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
                if (result)
                    return Ok();
                else
                    return BadRequest();
            }
            else
                throw new HttpResponseException(HttpStatusCode.Forbidden);
        }


        // PUT: api/LabWork/5
        // измененить лабораторную работу (автор, администратор)
        public async Task<IHttpActionResult> Put(string id, [FromBody]LabWork lab)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            try
            {
                //проверка наличия прав пользователя
                bool right = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION) ||
                    authProvider.LabWorks.Contains(id);

                if (right)
                {
                    lab.ID = id;
                    if (await lab.Update()) //обновление записи в БД
                        return Ok();
                    else
                    {
                        Logger logger = LogManager.GetCurrentClassLogger();
                        logger.Error(string.Format("Лаб. работа \"{0}\" не была обновлена",id)); //запись лога с ошибкой
                        return InternalServerError();
                    }
                }
                else
                    throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString()); //запись лога с ошибкой
                return InternalServerError();
            }
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
        //TODO: метод еще пустой
        [HttpDelete]
        [Route("api/LabWork/file/{id}")]
        public async Task<HttpResponseMessage> DeleteFile(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                try
                {
                    LabWork work = await LabWork.GetInstanceAsync(id);
                    string path = work?.FileURL;
                    string filename = work?.FileName;

                    //проверка наличия прав на операцию
                    bool commonRight = default(bool),
                        authorRight = default(bool);
                    Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.LBWRK_COMMON_PERMISSION),
                                    () => authorRight = authProvider.LabWorks.Contains(id));

                    if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(filename) && (commonRight || authorRight))
                    {
                        work.FileURL = null;
                        work.FileName = null;
                        if (await work.Update()) //удаление ссылки на файл из БД
                        {
                            try
                            {
                                File.Delete(path + filename); //удаление файла
                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            catch (Exception e)
                            {
                                Logger logger = LogManager.GetCurrentClassLogger();
                                logger.Warn(e.Message);
                            }
                        }
                        else
                        {
                            Logger logger = LogManager.GetCurrentClassLogger();
                            logger.Warn(string.Format("Файл из лаб. работы \"{0}\" не был удален",id));
                        }
                        return Request.CreateResponse(HttpStatusCode.NotFound);

                    }
                    else
                        return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
                catch (Exception e)
                {
                    Logger logger = LogManager.GetCurrentClassLogger();
                    logger.Fatal(e.ToString());
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }




        /// <summary>
        /// получает предмет по id лаб. работы из плана
        /// </summary>
        /// <param name="labWorkPlan">лаб. работа из плана</param>
        /// <returns></returns>
        private async Task<string> getSubjectAsync(string labWorkPlan)
        {
            string sqlQuery = "select SubjectGroupSemesterID from LabWorksPlan where ID=@labWorkPlan";
            var parameters = new Dictionary<string, string>();
            parameters.Add("@labWorkPlan", labWorkPlan);
            DB db = DB.GetInstance();
            return await db.ExecuteScalarQueryAsync(sqlQuery, parameters);
        }

        /// <summary>
        /// получает id лаб. работы из имени и доп. информации
        /// </summary>
        /// <param name="name">имя</param>
        /// <param name="advanced">доп. информация</param>
        /// <returns></returns>
        private async Task<string> getLab(string name, string advanced)
        {
            DB db = DB.GetInstance();
            string sqlQuery = "select ID from LabWorks " +
                "where name=@name and advanced=@advanced";
            var parameters = new Dictionary<string, string>();
            parameters.Add("@name", name);
            parameters.Add("@advanced", advanced);
            return await db.ExecuteScalarQueryAsync(sqlQuery, parameters);
        }
    }
}
