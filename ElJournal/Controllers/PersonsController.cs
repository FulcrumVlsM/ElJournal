using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Models;
using ElJournal.DBInteract;
using System.Web.Http.Controllers;

namespace ElJournal.Controllers
{
    public class PersonsController : ApiController
    {
        private const string PERSON_CRUD_PERMISSION = "PERSON_CRUD_PERMISSION";
        private const string STUDENT_FACULTY_CRUD_PERMISSION = "STUDENT_FACULTY_CRUD_PERMISSION";
        private const string TEACHER_DEPARTMENT_CRUD_PERMISSION = "TEACHER_DEPARTMENT_CRUD_PERMISSION";

        //история запросов: ip клиента - время последнего запроса
        private static Dictionary<string, DateTime> _clientsHistory = new Dictionary<string, DateTime>(30);


        // GET: api/Persons
        // Authorization: personId
        // Отправка запроса с одного клиента доступна не чаще чем 1 раз в 10 минут
        public async Task<dynamic> Get()
        {
            RespPerson response = new RespPerson();//формат ответа
            string clientIP = Request.GetOwinContext().Request.RemoteIpAddress;//ip клиента
            string authorId = Request.Headers.Authorization?.Scheme; //id пользователя
            DateTime last = default(DateTime);
            string sqlQueryOutAuth = "select name,info from Persons";
            string sqlQueryWithAuth = "select * from Persons";

            //TODO: вынести в отдельный метод
            try
            {
                last = _clientsHistory[clientIP];
                if (DateTime.Compare(last, DateTime.Now.AddMinutes(10)) > 0)//если не прошло время ожидания запроса
                {
                    response.NextRequestTo = last.ToUniversalTime();
                    response.Error = ErrorMessage.WAIT_YOUR_TIME;
                    return response;
                }
            }
            catch (KeyNotFoundException) { }

            try
            {
                DB db = DB.GetInstance();
                string sqlQuery;
                bool right = await db.CheckPermission(authorId, PERSON_CRUD_PERMISSION);
                if (right)
                    sqlQuery = sqlQueryWithAuth;
                else
                    sqlQuery = sqlQueryOutAuth;
                response.Data = await db.ExecSelectQuery(sqlQuery);
                response.Succesful = true;
                _clientsHistory.Add(clientIP, DateTime.Now);
                response.NextRequestTo = DateTime.UtcNow.AddMinutes(10);
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            new Task(clearHistory).Start();//очистка истории (значения старше одного часа)
            return response;
        }

        // GET: api/Persons/5
        // Authorization: personId
        public async Task<dynamic> Get(string id)
        {
            Response response = new Response();//формат ответа
            Dictionary<string, string> parameters = new Dictionary<string, string>();//параметры sql запроса
            string authorId = Request.Headers.Authorization?.Scheme; //id пользователя из заголовка http
            string sqlQueryWithAuth = "select * from Persons where ID=@id";
            string sqlQueryOutAuth = "select name,info from Persons where ID=@id";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(authorId, PERSON_CRUD_PERMISSION);
                string sqlQuery;
                if (right)
                    sqlQuery = sqlQueryWithAuth;
                else
                    sqlQuery = sqlQueryOutAuth;

                parameters.Add("@id", id);
                response.Data = await db.ExecSelectQuery(sqlQuery,parameters);
                response.Succesful = true;
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }

        // POST: api/Persons
        //Authorization: personId
        public async Task<dynamic> Post([FromBody]Person person)
        {
            Response response = new Response(); //формат ответа
            string authorId = Request.Headers.Authorization?.Scheme; //id пользователя из заголовка http
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string sqlQuery = "insert into Persons(RolesID,name,student_id,passport_id,avn_login,info) " +
                "values (@rolesId,@name,@student_id,@passport_id,@avn_login,@info)";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(authorId, PERSON_CRUD_PERMISSION);
                if (right)
                {
                    parameters.Add("@rolesId", person.RolesId);
                    parameters.Add("@name", person.name);
                    parameters.Add("@student_id", person.student_id);
                    parameters.Add("@passport_id", person.avn_login);
                    parameters.Add("@avn_login", person.avn_login);
                    parameters.Add("@info", person.info);
                    int res = db.ExecInsOrDelQuery(sqlQuery, parameters);
                    if (res != 0)
                    {
                        response.Succesful = true;
                        response.message = "Department was added";
                    }
                    else
                        response.message = "Department not added";
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

        // PUT: api/Persons/5
        //Authorization: personId
        public async Task<dynamic> Put(string id, [FromBody]Person person)
        {
            Response response = new Response(); //формат ответа
            string authorId = Request.Headers.Authorization?.Scheme; //id пользователя из заголовка http
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string sqlQuery = "dbo.UpdatePerson";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(authorId, PERSON_CRUD_PERMISSION);
                if (right)//если у пользователя есть права на операцию
                {
                    parameters.Add("@ID", id);
                    parameters.Add("@RolesID", person.RolesId);
                    parameters.Add("@name", person.name);
                    parameters.Add("@student_id", person.student_id);
                    parameters.Add("@passport_id", person.passport_id);
                    parameters.Add("@avn_login", person.avn_login);
                    parameters.Add("@info", person.info);
                    int res = db.ExecStoredProcedure(sqlQuery, parameters);
                    if (res == 0)
                    {
                        response.Succesful = true;
                        response.message = "Person data was changed";
                    }
                    else
                        response.message = "Person data was't changed";
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

        // DELETE: api/Persons/5
        //Authorization: personId
        public async Task<dynamic> Delete(string id)
        {
            Response response = new Response(); //формат ответа
            string authorId = Request.Headers.Authorization?.Scheme; //id пользователя из заголовка http
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string sqlQuery = "delete from Persons where ID=@id";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(authorId, PERSON_CRUD_PERMISSION);
                if (right)
                {
                    parameters.Add("@id", id);
                    int res = db.ExecInsOrDelQuery(sqlQuery, parameters);
                    if (res != 0)  //TODO: возможно лучше сделать условие res==1, чтобы гарантировать удаление одной записи
                    {
                        response.Succesful = true;
                        response.message = "Department was delete";
                    }
                    else
                        response.message = "Department wasn't delete";
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


        private void clearHistory()
        {
            foreach (var obj in _clientsHistory)
            {
                if (DateTime.Compare(obj.Value, DateTime.Now.AddHours(-1)) < 0)
                {
                    _clientsHistory.Remove(obj.Key);
                }

            }
        }
    }
}
