using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ElJournal.Controllers
{
    public class CourseWorkController : ApiController
    {
        // GET: api/CourseWork
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/CourseWork/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/CourseWork
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/CourseWork/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/CourseWork/5
        public void Delete(int id)
        {
        }
    }
}
