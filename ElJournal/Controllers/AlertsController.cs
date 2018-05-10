using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ElJournal.DBInteract;
using ElJournal.Models;
using ElJournal.Providers;
using System.Threading.Tasks;

namespace ElJournal.Controllers
{
    public class AlertsController : ApiController
    {
        // вернуть список всех уведомлений (новостей). Без авторизации доступны только открытые (новостные)
        // вернуть уведомление по id (все рег. пользователи)
        // вернуть типы уведомлений (все)
        // добавить уведомление (общее, для кафедры, для факультета) (администратор, администратор кафедры, факультета)
        // добавить уведомление к указанному предмету (преподаватель)
        // Добавить новостное уведомление (администратор)
        // изменить уведомление (создатель, администратор)
        // удалить уведомление (создатель, администратор)


        // GET: api/Alerts
        // возвращает список всех уведомлений.
        [HttpGet]
        [Route("api/Alerts")]
        public async Task<HttpResponseMessage> Get([FromUri]string startDate=null, [FromUri]string endDate=null)
        {
            Response response = new Response();
            DateTime start = default(DateTime),
                end = default(DateTime);
            if (!DateTime.TryParse(startDate, out start))
                start = DateTime.Today.AddDays(-2);
            if (!DateTime.TryParse(endDate, out end))
                end = DateTime.Today.AddDays(1);

            var alerts = await Alert.GetCollectionAsync(start, end);

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                alerts = alerts.FindAll(x => x.Opened ?? false); // без авторизации остаются только публичные записи

            response.Data = alerts;
            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        // GET: api/Alerts/5
        // возвращает уведомление по id
        [HttpGet]
        [Route("api/Alerts/{id}")]
        public async Task<HttpResponseMessage> Get(string id)
        {
            Response response = new Response();

            Alert alert = await Alert.GetInstanceAsync(id);

            response.Data = alert;
            if (alert != null)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // GET: api/Alerts/types
        // возвращает типы уведомлений
        [HttpGet]
        [Route("api/Alerts/types")]
        public async Task<HttpResponseMessage> GetType()
        {
            Response response = new Response();
            var types = await AlertType.GetCollectionAsync();
            response.Data = types;
            if (types.Count > 0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NoContent, response);
        }


        // POST: api/Alerts
        // добавить уведомление (общее, для кафедры, для факультета) (администратор, администратор кафедры, факультета)
        [HttpPost]
        [Route("api/Alerts")]
        public async Task<HttpResponseMessage> Post([FromBody]Alert alert)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //проверка наличия прав
            bool commonRight = default(bool),
                depRight = default(bool),
                facRight = default(bool),
                subjRight = default(bool);
            Parallel.Invoke(
                // проверка наличия общих прав
                () => commonRight = authProvider.CheckPermission(Permission.EVENT_COMMON_PERMISSION),
                //если указана целевая кафедра, проверяется право на кафедру
                () =>
                {
                    if (alert?.DepartmentId != null)
                        depRight = authProvider.CheckPermission(Permission.EVENT_PERMISSION) ?
                                       authProvider.Departments.Contains(alert?.DepartmentId) : false;
                    else
                        depRight = true;
                },
                // если указан целевой факультет, проверяются права на факультет
                () =>
                {
                    if (alert?.FacultyId != null)
                        facRight = authProvider.CheckPermission(Permission.EVENT_PERMISSION) ?
                                       authProvider.Faculties.Contains(alert?.FacultyId) : false;
                    else
                        facRight = true;
                },
                // если указан целевой предмет, проверяется, является ли пользователь преподавателем по данному предмету
                () =>
                {
                    alert.Opened = false; //этот метод не создает новостных (публичных) постов
                    if (alert?.FlowSubjectId != null)
                        subjRight = authProvider.CheckPermission(Permission.EVENT_PERMISSION) ?
                                       authProvider.FlowsSubjects.Contains(alert?.FlowSubjectId) : false;
                    else
                        subjRight = true;
                }
                );

            if(commonRight || (depRight && facRight && subjRight))
            {
                alert.AuthorID = authProvider.PersonId;
                if (await alert.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // POST: api/Alerts/news
        // добавление новостного уведомления
        [HttpPost]
        [Route("api/Alerts/news")]
        public async Task<dynamic> PostNews([FromBody]Alert alert)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //проверка прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.EVENT_COMMON_PERMISSION);
            if (commonRight)
            {
                alert.Opened = true;
                alert.FlowSubjectId = null;
                alert.AuthorID = authProvider.PersonId;
                if (await alert.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // PUT: api/Alerts/5
        // изменение уведомления
        [HttpPut]
        [Route("api/Alerts/{id}")]
        public async Task<HttpResponseMessage> Put(string id, [FromBody]Alert alert)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск существующего уведомления
            Alert realAlert = await Alert.GetInstanceAsync(id);
            if (realAlert == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            bool commonRight = authProvider.CheckPermission(Permission.EVENT_COMMON_PERMISSION);
            bool authorRight = authProvider.PersonId == realAlert.AuthorID;

            if(commonRight || authorRight)
            {
                alert.ID = id;
                if (await alert.Update())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // DELETE: api/Alerts/5
        // удаление уведомления
        [HttpDelete]
        [Route("api/Alerts/{id}")]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск существующего уведомления
            Alert alert = await Alert.GetInstanceAsync(id);
            if (alert == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            bool commonRight = authProvider.CheckPermission(Permission.EVENT_COMMON_PERMISSION);
            bool authorRight = authProvider.PersonId == alert.AuthorID;

            if (commonRight || authorRight)
            {
                if (alert.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}
