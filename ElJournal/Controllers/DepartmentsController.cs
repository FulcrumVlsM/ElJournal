using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ElJournal.DBInteract;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Data.SqlClient;
using ElJournal.Models;
using ElJournal.Providers;

namespace ElJournal.Controllers
{
    //develop: Elena
    public class DepartmentsController : ApiController
    {
        // GET: api/Departments
        //TODO: результат запроса должен записываться в Response.Data, а не возвращаться напрямую
        public async Task<HttpResponseMessage> Get()
        {
            Response response = new Response();
            var departments = await Department.GetCollectionAsync();
            response.Data = departments;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
            

        // GET: api/Departments/5
        public async Task<HttpResponseMessage> Get(string id)
        {
            Response response = new Response();
            Department department = await Department.GetInstanceAsync(id);
            response.Data = department;
            if(department!=null)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        //TODO: при добавлении кафедры нужно сделать, чтобы в ответе был id этой добавленной записи
        // POST: api/Departments
        public async Task<HttpResponseMessage> Post([FromBody]Department department)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //проверка прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.DEPARTMENT_COMMON_PERMISSION);

            if (commonRight)
            {
                if(await department.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // PUT: api/Departments/5
        public async Task<HttpResponseMessage> Put(string id, [FromBody]Department department)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //проверка прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.DEPARTMENT_COMMON_PERMISSION);

            if (commonRight)
            {
                department.ID = id;
                if (await department.Update())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // DELETE: api/Departments/5
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme; //токен пользователя
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            Department department = await Department.GetInstanceAsync(id);
            if(department == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.DEPARTMENT_COMMON_PERMISSION);

            if (commonRight)
            {
                if (department.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            return Request.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}