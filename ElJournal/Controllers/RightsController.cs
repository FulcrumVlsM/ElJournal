using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Models;

namespace ElJournal.Controllers
{
    public class RightsController : ApiController
    {
        //вернуть список прав (все)
        
        // GET: api/Rights
        public async Task<HttpResponseMessage> Get([FromUri]string roleId = null)
        {
            Response response = new Response();
            var rights = string.IsNullOrEmpty(roleId) ? 
                await Right.GetCollectionAsync() : await Right.GetCollectionAsync(roleId);
            response.Data = rights;
            if (rights.Count > 0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NoContent, response);
        }
    }
}
