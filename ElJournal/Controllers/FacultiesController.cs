using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ElJournal.DBInteract;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ElJournal.Models;
using System.Data.SqlClient;

namespace ElJournal.Controllers
{
    public class FacultiesController : ApiController
    {
        // GET: api/Faculties
        // полный список всех факультетов
        public async Task<dynamic> Get()
        {
            //формат ответа
            Response response = new Response();

            try
            {
                DB db = DB.GetInstance();
                response.Succesful = true;
                response.Data = await db.ExecSelectQuery("select * from Faculties");
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }
            return response;
        }

        // GET: api/Faculties/guid
        // возвращает данные по конкретному факультету
        public async Task<dynamic> Get(string id)
        {
            Response response = new Response();

            try
            {
                DB db = DB.GetInstance();
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("@id", id);
                response.Succesful = true;
                response.Data = await db.ExecSelectQuery("select * from Faculties where ID=@id", parameters);
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }
        

        // POST: api/Faculties
        public async Task<dynamic> Post([FromBody]Faculty faculty)
        {   
            Response response = new Response(); //формат ответа
            string sqlQuery = "insert into Faculties(dekanPersonID,name,description) " +
                        "values(@dekanPersonID,@name,@description";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(faculty.authorId, "FACULTY_ALL_PERMISSION");
                
                if (right) //если у пользователя есть права на операцию
                {
                    //необходимые параметры запроса
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@dekanPersonID", faculty.dekanId);
                    parameters.Add("@name", faculty.name);
                    parameters.Add("@description", faculty.description);

                    //выполнение запроса
                    int res = db.ExecInsOrDelQuery(sqlQuery, parameters);
                    if (res != 0)
                    {
                        response.Succesful = true;
                        response.message = "New faculty was added";
                    }
                    else
                        response.message = "Faculty not added";
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

        // PUT: api/Faculties/5
        public async Task<dynamic> Put(string id, [FromBody]Faculty faculty)
        {
            Response response = new Response(); //формат результата запроса
            string sqlQuery = "dbo.UpdateFaculty";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(faculty.authorId, "FACULTY_ALL_PERMISSION");
                if (right)
                {
                    //выполнение операции
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@ID", id);
                    parameters.Add("@dekanID", faculty.dekanId);
                    parameters.Add("@name", faculty.name);
                    parameters.Add("@description", faculty.description);
                    int res = db.ExecStoredProcedure(sqlQuery, parameters);
                    if (res == 0)
                        response.Succesful = true;
                    else
                        response.message = "Operation was false";
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

        // DELETE: api/Faculties/5
        public async Task<dynamic> Delete(string id, [FromBody]Faculty faculty)
        {
            Response response = new Response(); //формат ответа
            string sqlQuery = "delete from Faculties where ID=@ID";

            try
            {
                DB db = DB.GetInstance();

                bool right = await db.CheckPermission(faculty.authorId, "FACULTY_ALL_PERMISSION");

                if (right)
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@ID", id);
                    int result = db.ExecInsOrDelQuery(sqlQuery, parameters);
                    if (result == 1)
                    {
                        response.Succesful = true;
                        response.message = String.Format("Faculty was deleted");
                    }
                    else
                        response.message = "Operation was failed";
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
