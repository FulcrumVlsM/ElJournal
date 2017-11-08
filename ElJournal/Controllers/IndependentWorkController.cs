using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ElJournal.Controllers
{
    public class IndependentWorkController : ApiController
    {
        // GET: api/IndependentWork
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/IndependentWork/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/IndependentWork
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/IndependentWork/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/IndependentWork/5
        public void Delete(int id)
        {
        }
    }
}
