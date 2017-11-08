using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ElJournal.Controllers
{
    public class GroupsController : ApiController
    {
        // GET: api/Groups
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Groups/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Groups
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Groups/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Groups/5
        public void Delete(int id)
        {
        }
    }
}
