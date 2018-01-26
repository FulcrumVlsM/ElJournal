using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ElJournal.Models;
using System.Threading.Tasks;

namespace ElJournal.Controllers
{
    public class SemesterController : ApiController
    {
        // GET: api/Semester
        public async Task<dynamic> Get()
        {
            return null;
        }

        // GET: api/Semester/5
        public async Task<dynamic> Get(string id)
        {
            return null;
        }

        // POST: api/Semester
        public async Task<dynamic> Post([FromBody]Semester semester)
        {
            return null;
        }

        // PUT: api/Semester/5
        public async Task<dynamic> Put(string id, [FromBody]Semester semester)
        {
            return null;
        }

        // DELETE: api/Semester/5
        public async Task<dynamic> Delete(string id)
        {
            return null;
        }
    }
}
