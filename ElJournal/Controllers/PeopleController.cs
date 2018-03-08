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
using ElJournal.Providers;

namespace ElJournal.Controllers
{
    //develop: Mikhail
    public class PeopleController : ApiController
    {
        //private const string PERSON_COMMON_PERMISSION = "PERSON_COMMON_PERMISSION";

        //история запросов: ip клиента - время последнего запроса
        private static Dictionary<string, DateTime> _clientsHistory = new Dictionary<string, DateTime>(30);
        private static int _timeOut = 10; //промежуток отправки запросов
        


        // GET: api/Persons
        // Authorization: token
        // Отправка запроса с одного клиента доступна не чаще чем 1 раз в _timeOut
        public async Task<dynamic> Get()
        {
            RespPerson response = new RespPerson();//формат ответа
            string clientIP = Request?.GetOwinContext()?.Request?.RemoteIpAddress;//ip клиента
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя
            string sqlQueryOutAuth = "select ID, name, info from People";//запрос для неавторизированного пользователя
            string sqlQueryWithAuth = "select * from People";//запрос для авторизированного пользователя

            if (!findLast(response, clientIP))//если не прошло время ожидания след. запроса
                return response;

            try
            {
                DB db = DB.GetInstance();
                string sqlQuery;
                bool right = await db.CheckPermission(token, Permission.PERSON_COMMON_PERMISSION);//проверка прав пользователя
                if (right)
                    sqlQuery = sqlQueryWithAuth;
                else
                    sqlQuery = sqlQueryOutAuth;

                response.Data = await db.ExecSelectQuery(sqlQuery);//выполнение sql запроса
                response.Succesful = true;

                _clientsHistory[clientIP] = DateTime.Now;    //добавление запроса в историю
                response.NextRequestTo = DateTime.UtcNow.AddMinutes(_timeOut);
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
                //TODO: add log
            }

            new Task(clearHistory).Start();//очистка истории (значения старше одного часа)
            return response;
        }

        // GET: api/Persons/5
        // Authorization: token
        public async Task<dynamic> Get(string id)
        {
            Response response = new Response();//формат ответа
            Dictionary<string, string> parameters = new Dictionary<string, string>();//параметры sql запроса
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            string sqlQueryWithAuth = "select * from People where ID=@id"; //запрос для авторизованного пользователя
            string sqlQueryOutAuth = "select ID name,info from" +
                " People where ID=@id";//запрос для неавторизованного пользователя

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(token, Permission.PERSON_COMMON_PERMISSION);//авторизация пользователя
                string sqlQuery;
                if (right) //если права имеются
                    sqlQuery = sqlQueryWithAuth;
                else
                    sqlQuery = sqlQueryOutAuth;

                parameters.Add("@id", id);
                response.Data = await db.ExecSelectQuery(sqlQuery, parameters);//запрос в бд
                response.Succesful = true;
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
                //TODO: add log
            }

            return response;
        }

        //TODO: метод еще пустой
        //GET: api/Persons?name=...
        //Authorization: token
        public async Task<dynamic> Get([FromUri]string name,[FromUri]int count=5)
        {
            Response response = new Response();//формат ответа
            Dictionary<string, string> parameters = new Dictionary<string, string>();//параметры sql запроса
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            string sqlQueryWithAuth = "select top @count " +
                "* from dbo.GetPeopleForName(@regex)"; //запрос для авторизованного пользователя
            string sqlQueryOutAuth = "select top @count ID name,info from" +
                " dbo.GetPeopleForName(@regex)";//запрос для неавторизованного пользователя

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(token, Permission.PERSON_COMMON_PERMISSION);//авторизация пользователя
                string sqlQuery;
                if (right) //если права имеются
                    sqlQuery = sqlQueryWithAuth;
                else
                    sqlQuery = sqlQueryOutAuth;

                parameters.Add("@regex", name);
                parameters.Add("@count", count.ToString());
                response.Data = await db.ExecSelectQuery(sqlQuery, parameters);//запрос в бд
                response.Succesful = true;
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
                //TODO: add log
            }

            return response;
        }

        // POST: api/Persons
        //Authorization: token
        public async Task<dynamic> Post([FromBody]Person person)
        {
            Response response = new Response(); //формат ответа
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            var parameters = new Dictionary<string, string>();

            //
            string sqlAddQuery = "dbo.AddPerson";
            string sqlGetId = "select top 1 ID from Persons where ID=@name and passport_id=@passport_id";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(token, Permission.PERSON_COMMON_PERMISSION);//авторизация пользователя
                if (right)
                {
                    parameters.Add("@RolesID", person.RolesId); //добавление параметров к запросу
                    parameters.Add("@name", person.name);
                    parameters.Add("@student_id", person.student_id);
                    parameters.Add("@passport_id", person.avn_login);
                    parameters.Add("@avn_login", person.avn_login);
                    parameters.Add("@info", person.info);

                    int res = db.ExecStoredProcedure(sqlAddQuery, parameters); //получение данных из бд
                    if (res == 0)//процедура вернет 0 при успешном завершении операции
                    {
                        response.Succesful = true;
                        response.message = "Person was added";
                        response.Data = 
                            new { ID = await db.ExecuteScalarQuery(sqlGetId, parameters) };//получение id созданной записи
                    }
                    else
                        response.message = "Person not added";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
                //TODO: add log
            }

            return response;
        }

        // PUT: api/Persons/5
        //Authorization: token
        public async Task<dynamic> Put(string id, [FromBody]Person person)
        {
            Response response = new Response(); //формат ответа
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            var parameters = new Dictionary<string, string>();
            string sqlQuery = "dbo.UpdatePerson";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(token, Permission.PERSON_COMMON_PERMISSION);
                if (right)//если у пользователя есть права на операцию
                {
                    parameters.Add("@ID", id);
                    parameters.Add("@RolesID", person.RolesId);//добавление параметров к запросу
                    parameters.Add("@name", person.name);
                    parameters.Add("@student_id", person.student_id);
                    parameters.Add("@passport_id", person.passport_id);
                    parameters.Add("@avn_login", person.avn_login);
                    parameters.Add("@info", person.info);

                    int res = db.ExecStoredProcedure(sqlQuery, parameters);//выполнение запроса
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
                //TODO: add log
            }
            return response;
        }

        // DELETE: api/Persons/5
        //Authorization: token
        public async Task<dynamic> Delete(string id)
        {
            Response response = new Response(); //формат ответа
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http
            var parameters = new Dictionary<string, string>();
            string sqlQuery = "delete from People where ID=@id";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(token, Permission.PERSON_COMMON_PERMISSION);
                if (right)
                {
                    parameters.Add("@id", id);//добавление параметра к запросу
                    int res = db.ExecInsOrDelQuery(sqlQuery, parameters); //выполнение запроса
                    if (res != 0)  //TODO: возможно лучше сделать условие res==1, чтобы гарантировать удаление одной записи
                    {
                        response.Succesful = true;
                        response.message = "Person was delete";
                    }
                    else
                        response.message = "Person wasn't delete";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
                //TODO: add log
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
        private bool findLast(RespPerson response, string clientIP)
        {
            DateTime last = default(DateTime);//время предыдущего запроса
            try
            {
                last = _clientsHistory[clientIP];
                if (DateTime.Compare(last, DateTime.Now.AddMinutes(_timeOut)) < 0)//если не прошло время ожидания запроса
                {
                    response.NextRequestTo = last.ToUniversalTime();
                    response.Error = ErrorMessage.WAIT_YOUR_TIME;
                    return false;
                }
            }
            catch (KeyNotFoundException)
            {
                _clientsHistory.Add(clientIP, DateTime.Now);
            }
            return true;
        }
    }
}
