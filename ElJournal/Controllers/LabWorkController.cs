using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ElJournal.Controllers
{
    public class LabWorkController : ApiController
    {
        // GET: api/LabWork
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/LabWork/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/LabWork
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/LabWork/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/LabWork/5
        public void Delete(int id)
        {
        }
    }
}
