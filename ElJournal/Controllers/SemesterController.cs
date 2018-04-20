using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ElJournal.Models;
using System.Threading.Tasks;
using ElJournal.Providers;
using ElJournal.DBInteract;

namespace ElJournal.Controllers
{
    public class SemesterController : ApiController
    {
        // вернуть список всех семестров (все)
        // вернуть семестр по id (все)
        // добавить новый семестр (администратор)
        // изменить данные для указанного семестра (администратор)
        // удалить семестр (администратор)


        // GET: api/Semester
        // вернуть список всех семестров (все)
        public async Task<HttpResponseMessage> Get()
        {
            Response response = new Response();
            response.Data = await Semester.GetCollectionAsync();
            if (response.Data.Count > 0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NoContent, response);
        }

        // GET: api/Semester/5
        // вернуть семестр по id (все)
        public async Task<HttpResponseMessage> Get(string id)
        {
            Response response = new Response();
            Semester semester = await Semester.GetInstanceAsync(id);
            response.Data = semester;
            if (semester != null)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NoContent, response);
        }

        // POST: api/Semester
        // добавить новый семестр (администратор)
        public async Task<HttpResponseMessage> Post([FromBody]Semester semester)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //првоерка наличия прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.FACULTY_COMMON_PERMISSION);

            if (commonRight)
            {
                if(await semester.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }

        // PUT: api/Semester/5
        // изменение данных о семестре
        public async Task<HttpResponseMessage> Put(string id, [FromBody]Semester semester)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //првоерка наличия прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.FACULTY_COMMON_PERMISSION);

            if (commonRight)
            {
                semester.ID = id;
                if (await semester.Update())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }

        // DELETE: api/Semester/5
        // удаление семестра
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            Semester semester = await Semester.GetInstanceAsync(id);
            if(semester == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //првоерка наличия прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.FACULTY_COMMON_PERMISSION);

            if (commonRight)
            {
                if (semester.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}
