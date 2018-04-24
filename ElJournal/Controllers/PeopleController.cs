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
using System.Text.RegularExpressions;

namespace ElJournal.Controllers
{
    //develop: Mikhail
    public class PeopleController : ApiController
    {
        //private const string PERSON_COMMON_PERMISSION = "PERSON_COMMON_PERMISSION";

        //история запросов: ip клиента - время последнего запроса
        private static Dictionary<string, DateTime> _clientsHistory = new Dictionary<string, DateTime>(30);
        private static int _timeOut = 10; //промежуток отправки запросов

        // получить список всех пользователей (все)
        // получить пользователя по ID (все рег. пользователи, админ видит всю инфу)
        // добавить пользователя (администратор)
        // изменить пользователя (администратор)
        // удалить пользователя (администратор)

        // добавить пользователя на указанный факультет (администратор)
        // удалить пользователя из указанного факультета (администратор)
        // добавить пользователя к указанной кафедре (администратор)
        // удалить пользователя из указанной кафедры (администратор)

        // обновить токен (все рег. пользователи)



        // GET: api/Persons
        // получить список всех пользователей (все)
        public async Task<HttpResponseMessage> Get([FromUri]string name = null, [FromUri]string roleId = null, [FromUri]int count = 50)
        {
            Response response = new Response();
            var people = await Person.GetCollectionAsync();
            if (!string.IsNullOrEmpty(roleId))
                people = people.FindAll(x => x.RoleId == roleId); //отбор по праву
            if (!string.IsNullOrEmpty(name))
            {
                Regex regex = new Regex(name); //поиск по ФИО
                people = people.FindAll(x => regex.IsMatch(x.Name) || regex.IsMatch(x.Surname) || regex.IsMatch(x.Patronymic));
            }
            people = people.GetRange(0, count); //отбор количества count
            response.Data = people;
            if (people.Count > 0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NoContent, response);

        }

        // GET: api/Persons/5
        // получить пользователя по ID (все, админ видит конфиденциальную инфу)
        public async Task<HttpResponseMessage> Get(string id)
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            Person person;
            bool commonRight = authProvider?.CheckPermission(Permission.PERSON_COMMON_PERMISSION) ?? false;

            if (commonRight)
                person = await Person.GetInstanceAsync(id);
            else
                person = await Person.GetPublicInstanceAsync(id);

            response.Data = person;
            if (person != null)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound, response);
        }


        // POST: api/Persons
        // добавить пользователя (администратор)
        public async Task<HttpResponseMessage> Post([FromBody]Person person)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider != null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //првоерка наличия прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.PERSON_COMMON_PERMISSION);
            if (commonRight)
            {
                if (await person.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }

        // PUT: api/Persons/5
        // изменить пользователя (администратор)
        public async Task<HttpResponseMessage> Put(string id, [FromBody]Person person)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider != null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //првоерка наличия прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.PERSON_COMMON_PERMISSION);
            if (commonRight)
            {
                person.ID = id;
                if (await person.Update())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // DELETE: api/Persons/5
        // удалить пользователя (администратор)
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider != null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск указанного пользователя
            Person person = await Person.GetInstanceAsync(id);
            if (person == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //првоерка наличия прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.PERSON_COMMON_PERMISSION);
            if (commonRight)
            {
                if (await person.Update())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }



        [HttpGet]
        [Route("api/People/UpdateToken")]
        public async Task<dynamic> UpdateToken([FromBody]AccountModels auth)
        {
            return null;
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
