using ElJournal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ElJournal.Controllers
{
    //вернуть список ролей

    public class RolesController : ApiController
    {
        // GET: api/Roles
        public async Task<HttpResponseMessage> Get()
        {
            Response response = new Response();
            var roles = await Role.GetCollectionAsync();
            response.Data = roles;
            if (roles.Count > 0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NoContent, response);
        }
    }
}
