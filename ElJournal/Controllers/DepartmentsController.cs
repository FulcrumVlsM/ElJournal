using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.DBInteract;

namespace ElJournal.Controllers
{
    public class DepartmentsController : ApiController
    {
        // GET: api/Departments
        public async Task<dynamic> Get()
        {
            DB db = DB.GetInstance();
            return await db.ExecSelectQuery("select * from Departments");
        }

        // GET: api/Departments/5
        public async Task<dynamic> Get(string id)
        {
            DB db = DB.GetInstance();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@id", id);
            return await db.ExecSelectQuery("select * from Departments",parameters);
        }

        // POST: api/Departments
        public void Post([FromBody]string authorId, [FromBody]string name,
                         [FromBody]string managerId,[FromBody]string descriotion)
        {
        }

        // PUT: api/Departments/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Departments/5
        public void Delete(int id)
        {
        }
    }
}
