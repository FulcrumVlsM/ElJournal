using ElJournal.DBInteract;
using ElJournal.Models;
using ElJournal.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ElJournal.Controllers
{
    public class FlowsController : ApiController
    {
        // GET: api/Flows
        public async Task<HttpResponseMessage> Get([FromUri]string faculty = null)
        {
            Response response = new Response();
            var flows = await Flow.GetCollectionAsync();
            if (!string.IsNullOrEmpty(faculty))
                flows = flows.FindAll(x => x.DepartmentId == faculty);
            response.Data = flows;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // GET: api/Flows/5
        [HttpGet]
        [Route("api/Flows/{id}")]
        public async Task<HttpResponseMessage> GetById(string id)
        {
            Response response = new Response();
            var flows = await Flow.GetInstanceAsync(id);
            response.Data = flows;
            if(response.Data != null)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // POST: api/Flows
        public async Task<HttpResponseMessage> Post([FromBody]Flow flow)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                bool commonRight = default(bool),
                    facultyRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.DEPARTMENT_COMMON_PERMISSION),
                    () => facultyRight = authProvider.CheckPermission(Permission.DEPARTMENT_PERMISSION) ?
                                         authProvider.Faculties.Contains(flow.DepartmentId) : false);

                if(commonRight || facultyRight)
                {
                    if (await flow.Push())
                        return Request.CreateResponse(HttpStatusCode.Created);
                    else
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }

        // PUT: api/Flows/5
        [HttpPut]
        [Route("api/Flows/{id}")]
        public async Task<HttpResponseMessage> Put(string id, [FromBody]Flow flow)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                flow.ID = id;

                bool commonRight = default(bool),
                    facultyRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.DEPARTMENT_COMMON_PERMISSION),
                    () => facultyRight = authProvider.CheckPermission(Permission.DEPARTMENT_PERMISSION) ?
                                         authProvider.Faculties.Contains(flow.DepartmentId) : false);

                if (commonRight || facultyRight)
                {
                    if (await flow.Update())
                        return Request.CreateResponse(HttpStatusCode.OK);
                    else
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }

        // DELETE: api/Flows/5
        [HttpDelete]
        [Route("api/Flows/{id}")]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if(authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск потока
            Flow flow = await Flow.GetInstanceAsync(id);
            if(flow == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //првоерка наличия прав на операцию
            bool commonRight = default(bool),
                    facultyRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.DEPARTMENT_COMMON_PERMISSION),
                () => facultyRight = authProvider.CheckPermission(Permission.DEPARTMENT_PERMISSION) ?
                                     authProvider.Faculties.Contains(flow.DepartmentId) : false);

            if (commonRight || facultyRight)
            {
                if (flow.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}
