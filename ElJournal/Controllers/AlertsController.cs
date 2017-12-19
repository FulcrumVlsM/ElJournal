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
    public class AlertsController : ApiController
    {
        // GET: api/Alerts
        public async Task<dynamic> Get()
        {
            DB db = DB.GetInstance();
            return await db.ExecSelectQuery("select * from Events");

        }

        // GET: api/Alerts/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Alerts
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Alerts/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Alerts/5
        public void Delete(int id)
        {
        }
    }
}
