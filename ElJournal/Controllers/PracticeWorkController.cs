using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ElJournal.Controllers
{
    public class PracticeWorkController : ApiController
    {
        // GET: api/PracticeWork
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/PracticeWork/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/PracticeWork
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/PracticeWork/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/PracticeWork/5
        public void Delete(int id)
        {
        }
    }
}
