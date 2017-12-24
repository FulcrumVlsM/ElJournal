using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Models;
using ElJournal.DBInteract;

namespace ElJournal.Controllers
{
    public class GroupsController : ApiController
    {
        // GET: api/Groups
        public async Task<dynamic> Get()
        {
            Response response = new Response();
            string sqlQuery = "select * from Groups";

            try
            {
                DB db = DB.GetInstance();
                response.Data = await db.ExecSelectQuery(sqlQuery);
                response.Succesful = true;
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }

        // GET: api/Groups/5
        public async Task<dynamic> Get(string id)
        {
            Response response = new Response();
            string sqlQuery = "select * from Groups where ID=@id";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            try
            {
                DB db = DB.GetInstance();
                parameters.Add("@id", id);
                response.Data = await db.ExecSelectQuery(sqlQuery, parameters);
                response.Succesful = true;
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }
            return response;
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
