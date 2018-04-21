using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ElJournal.DBInteract;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ElJournal.Models;
using System.Data.SqlClient;
using ElJournal.Providers;

namespace ElJournal.Controllers
{
    //develop: Mikhail
    public class FacultiesController : ApiController
    {
        //TODO: проверка прав пользователя учитывает в данный момент только разрешения общего типа

        private const string FACULTY_COMMON_PERMISSION = "FACULTY_COMMON_PERMISSION"; //общее разрешение
        private const string FACULTY_PERMISSION = "FACULTY_PERMISSION"; //разрешение на связанный

        // GET: api/Faculties
        // возвращает полный список факультетов (все)
        public async Task<HttpResponseMessage> Get()
        {
            Response response = new Response();//формат ответа
            response.Data = await Faculty.GetCollectionAsync();
            if (response.Data.Count > 0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NoContent, response);
        }

        // GET: api/Faculties/guid
        public async Task<dynamic> Get(string id)
        {
            Response response = new Response();//формат ответа
            var faculty = await Faculty.GetInstanceAsync(id);
            response.Data = faculty;
            if(faculty != null)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }
        

        // POST: api/Faculties
        //TODO: запись в БД идет, но возвращается false
        public async Task<dynamic> Post([FromBody]Faculty faculty)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //Получение прав пользователя
            bool commonRight = authProvider.CheckPermission(Permission.FACULTY_COMMON_PERMISSION);

            if (commonRight)
            {
                if(await faculty.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);

        }

        // PUT: api/Faculties/guid
        public async Task<dynamic> Put(string id, [FromBody]Faculty faculty)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //Получение прав пользователя
            bool commonRight = authProvider.CheckPermission(Permission.FACULTY_COMMON_PERMISSION);

            if (commonRight)
            {
                faculty.ID = id;
                if(await faculty.Update())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }

        // DELETE: api/Faculties/guid
        public async Task<dynamic> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //Получение прав пользователя
            bool commonRight = authProvider.CheckPermission(Permission.FACULTY_COMMON_PERMISSION);

            if (commonRight)
            {
                Faculty faculty = await Faculty.GetInstanceAsync(id);

                if (faculty?.Delete() ?? false)
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}
