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
            return await db.ExecSelectQuery("select * from Faculties");
        }

        // GET: api/Faculties/guid
        // возвращает данные по конкретному факультету
        public async Task<dynamic> Get(string id)
        {
            DB db = DB.GetInstance();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@id", id);
            return await db.ExecSelectQuery("select * from Faculties where ID=@id", parameters);
        }
        

        // POST: api/Faculties
        public async Task<dynamic> Post([FromBody]string authorId,
                                        [FromBody]string name,
                                        [FromBody]string dekanId,
                                        [FromBody]string description)
        {
            return null;
        }

        // PUT: api/Faculties/5
        public async Task<dynamic> Put(string id, [FromBody]string authorId, [FromBody]string name,
                                   [FromBody]string dekanId, [FromBody]string description)
        {
            return null;
        }

        // DELETE: api/Faculties/5
        public async Task<dynamic> Delete(string id, [FromBody]string authorId)
        {
            return null;
        }
    }
}
