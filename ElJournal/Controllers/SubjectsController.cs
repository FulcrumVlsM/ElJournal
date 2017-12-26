using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ElJournal.DBInteract;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Data.SqlClient;
using ElJournal.Models;

namespace ElJournal.Controllers
{
    public class SubjectsController : ApiController
    {
        // GET: api/Subjects
        public async Task<dynamic> Get()
        {
            //формат ответа
            Response response = new Response();
            try
            {
                DB db = DB.GetInstance();
                response.Succesful = true;
                return await db.ExecSelectQuery("select * from Subjects");
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }
            return response;
        }

        // GET: api/Subjects/5
        public async Task<dynamic> Get(string id)
        {
            Response response = new Response();
            try
            {
                DB db = DB.GetInstance();
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("@id", id);
                response.Succesful = true;
                response.Data = await db.ExecSelectQuery("select * from Subjects", parameters);
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }
            return response;
        }

        // POST: api/Subjects
        public async Task<dynamic> Post([FromBody]SubjectModels subject)
        {
            Response response = new Response(); //формат ответа
            string sqlQuery = "insert into Departments(DepartmentID,name,info) " +
                        "values(@DepartmentID,@name,@info)";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(subject.authorId, "SUBJECT_ALL_PERMISSION");

                if (right) //если у пользователя есть права на операцию
                {
                    //необходимые параметры запроса
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@DepartmentID", subject.DepartmentID);
                    parameters.Add("@name", subject.name);
                    parameters.Add("@info", subject.info);

                    //выполнение запроса
                    int res = db.ExecInsOrDelQuery(sqlQuery, parameters);
                    if (res != 0)
                    {
                        response.Succesful = true;
                        response.message = "New subject was added";
                    }
                    else
                        response.message = "Department not added";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }

        // PUT: api/Subjects/5
        public async Task<dynamic> Put(string id, [FromBody]SubjectModels subject)
        {
            Response response = new Response(); //формат результата запроса
            string sqlQuery = "dbo.UpdateSubjects";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(subject.authorId, "SUBJECT_ALL_PERMISSION");
                if (right)
                {
                    //выполнение операции
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@ID", id);
                    parameters.Add("@DepartmentID", subject.DepartmentID);
                    parameters.Add("@name", subject.name);
                    parameters.Add("@info", subject.info);
                    int res = db.ExecStoredProcedure(sqlQuery, parameters);
                    if (res == 0)
                        response.Succesful = true;
                    else
                        response.message = "Operation was false";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }

        // DELETE: api/Subjects/5
        public async Task<dynamic> Delete(string id, [FromBody]SubjectModels subject)
        {
            Response response = new Response(); //формат ответа
            string sqlQuery = "delete from Subjects where ID=@ID";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(subject.authorId, "DUBJECT_ALL_PERMISSION");
                if (right)
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@ID", id);
                    int result = db.ExecInsOrDelQuery(sqlQuery, parameters);
                    if (result == 1)
                    {
                        response.Succesful = true;
                        response.message = String.Format("Subject was deleted");
                    }
                    else
                        response.message = "Operation was failed";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }
    }
}
