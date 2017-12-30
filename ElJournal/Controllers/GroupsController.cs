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
    //develop: Vilisov Mikhail
    public class GroupsController : ApiController
    {
        private const string GROUP_ALL_PERMISSION = "GROUP_ALL_PERMISSION";

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
        public async Task<dynamic> Post([FromBody]Group group)
        {
            Response response = new Response();//формат ответа
            string sqlQuery = "dbo.AddGroup";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string authorId = Request.Headers.Authorization?.Scheme; //id пользователя из заголовка http

            try
            {
                DB db = DB.GetInstance();
                parameters.Add("@name", group.name);
                parameters.Add("@info", group.info);
                parameters.Add("@FacultyID", group.facultyId);
                parameters.Add("@curatorPersonID", group.curatorId);

                bool right = await db.CheckPermission(authorId, GROUP_ALL_PERMISSION);
                if (right)
                {
                    int res = db.ExecStoredProcedure(sqlQuery, parameters);
                    if (res == 0)
                    {
                        response.Succesful = true;
                        response.message = "Group was added";
                    }
                    else
                        response.message = "Group wasn't added";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }

        // PUT: api/Groups/5
        public async Task<dynamic> Put(string id, [FromBody]Group group)
        {
            Response response = new Response();
            string sqlQuery = "dbo.UpdateGroup";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string authorId = Request.Headers.Authorization?.Scheme; //id пользователя из заголовка http

            try
            {
                DB db = DB.GetInstance();

                bool right = await db.CheckPermission(authorId, GROUP_ALL_PERMISSION);
                if (right)
                {
                    parameters.Add("@ID", id);
                    parameters.Add("@FacultyID", group.facultyId);
                    parameters.Add("@name", group.name);
                    parameters.Add("@info", group.info);
                    parameters.Add("@curatorPersonID", group.curatorId);

                    int res = db.ExecStoredProcedure(sqlQuery, parameters);
                    if (res == 0)
                    {
                        response.Succesful = true;
                        response.message = "Group was changed";
                    }
                    else
                        response.message = "Group wasn't changed";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }

        // DELETE: api/Groups/5
        public async Task<dynamic> Delete(string id)
        {
            Response response = new Response();
            string sqlQuery = "delete from Groups where ID=@ID";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string authorId = Request.Headers.Authorization?.Scheme; //id пользователя из заголовка http

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(authorId, GROUP_ALL_PERMISSION);
                if (right)
                {
                    parameters.Add("@ID", id);
                    int res = db.ExecInsOrDelQuery(sqlQuery, parameters);
                    if (res == 1)
                    {
                        response.Succesful = true;
                        response.message = "Group was deleted";
                    }
                    else
                        response.message = "Group wasn't deleted";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }
    }
}
