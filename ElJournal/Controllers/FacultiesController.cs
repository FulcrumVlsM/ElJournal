using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ElJournal.DBInteract;
using System.Threading.Tasks;

namespace ElJournal.Controllers
{
    public class FacultiesController : ApiController
    {
        // GET: api/Faculties
        // полный список всех факультетов
        public async Task<dynamic> Get()
        {
            DB db = DB.GetInstance();
            return db.ExecSelectQuery("select * from Faculties");
        }

        // GET: api/Faculties/guid
        // возвращает данные по конкретному факультету
        public dynamic Get(string id)
        {
            DB db = DB.GetInstance();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@id", id);
            return db.ExecSelectQuery("select * from Faculties where ID=@id", parameters);
        }

        // POST: api/Faculties
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Faculties/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Faculties/5
        public void Delete(int id)
        {
        }
    }
}
