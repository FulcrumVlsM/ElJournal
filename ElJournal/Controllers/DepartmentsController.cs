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
            //формат ответа
            Response response = new Response();
            try
            {
                DB db = DB.GetInstance();
                response.Succesful = true;
                return await db.ExecSelectQuery("select * from Departments");
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }
            return response;
        }
            

        // GET: api/Departments/5
        public async Task<dynamic> Get(string id)
        {
            Response response = new Response();
            try
            {
                DB db = DB.GetInstance();
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("@id", id);
                response.Succesful = true;
                response.Data = await db.ExecSelectQuery("select * from Departments", parameters);
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }
            return response;
        }

        // POST: api/Departments
        public async Task<dynamic> Post([FromBody]Departments department)
        {
            Response response = new Response(); //формат ответа
            string sqlQuery = "insert into Departments(managerPersonID,name,description) " +
                        "values(@managerPersonID,@name,@description)";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(department.authorId, "DEPARTMENT_ALL_PERMISSION");

                if (right) //если у пользователя есть права на операцию
                {
                    //необходимые параметры запроса
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@managerPersonID", department.managerPersonID);
                    parameters.Add("@name", department.name);
                    parameters.Add("@description", department.description);

                    //выполнение запроса
                    int res = db.ExecInsOrDelQuery(sqlQuery, parameters);
                    if (res != 0)
                    {
                        response.Succesful = true;
                        response.message = "New department was added";
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

        // PUT: api/Departments/5
        public void Put(int id, [FromBody]Departments department)
        {
            Response response = new Response(); //формат результата запроса
            string sqlQuery = "dbo.UpdateDepartment";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(department.authorId, "DEPARTMENT_ALL_PERMISSION");
                if (right)
                {
                    //выполнение операции
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@ID", id);
                    parameters.Add("@managerPersonID", department.managerPersonID);
                    parameters.Add("@name", department.name);
                    parameters.Add("@description", department.description);
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

        // DELETE: api/Departments/5
        public void Delete(int id)
        {
            Response response = new Response(); //формат ответа
            string sqlQuery = "delete from Departments where ID=@ID";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(faculty.authorId, "DEPARTMENT_ALL_PERMISSION");
                if (right)
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@ID", id);
                    int result = db.ExecInsOrDelQuery(sqlQuery, parameters);
                    if (result == 1)
                    {
                        response.Succesful = true;
                        response.message = String.Format("Department was deleted");
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