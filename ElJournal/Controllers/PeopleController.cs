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
        public async Task<HttpResponseMessage> Get([FromUri]string name = null, [FromUri]string roleId = null, 
            [FromUri]int count = 30)
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
            people = (people.Take(count)).ToList(); //отбор указанного количества
            response.Data = people;
            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        // GET: api/Persons/5
        // получить пользователя по ID (все, админ видит конфиденциальную инфу)
        [HttpGet]
        [Route("api/People/{id}")]
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


        [HttpGet]
        [Route("api/People/me")]
        public async Task<HttpResponseMessage> GetMe()
        {
            Response response = new Response();

            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            Person me = await Person.GetPublicInstanceAsync(authProvider.PersonId);
            response.Data = me;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // POST: api/Persons
        // добавить пользователя (администратор)
        public async Task<HttpResponseMessage> Post([FromBody]Person person)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
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


        // добавить пользователя на указанный факультет (администратор)
        [HttpPost]
        [Route("api/People/{personId}/faculty/{facultyId}")]
        public async Task<HttpResponseMessage> PostFaculty(string personId, string facultyId)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            Person person = await Person.GetInstanceAsync(personId);
            Faculty faculty = await Faculty.GetInstanceAsync(facultyId);
            if(person == null || faculty == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.PERSON_COMMON_PERMISSION);
            if (commonRight)
            {
                if (await person.AddOnFaculty(faculty.ID))
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // добавить пользователя к указанной кафедре (администратор)
        [HttpPost]
        [Route("api/People/{personId}/department/{departmentId}")]
        public async Task<HttpResponseMessage> PostDepartment(string personId, string departmentId)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            Person person = await Person.GetInstanceAsync(personId);
            Department department = await Department.GetInstanceAsync(departmentId);
            if (person == null || department == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.PERSON_COMMON_PERMISSION);
            if (commonRight)
            {
                if (await person.AddOnDepartment(department.ID))
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // PUT: api/Persons/5
        // изменить пользователя (администратор)
        [HttpPut]
        [Route("api/People/{id}")]
        public async Task<HttpResponseMessage> Put(string id, [FromBody]Person person)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
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
        [HttpDelete]
        [Route("api/People/{id}")]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск указанного пользователя
            Person person = await Person.GetInstanceAsync(id);
            if (person == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //првоерка наличия прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.PERSON_COMMON_PERMISSION);
            if (commonRight)
            {
                if (person.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // удалить пользователя из указанного факультета (администратор)
        [HttpDelete]
        [Route("api/People/{personId}/faculty/{facultyId}")]
        public async Task<HttpResponseMessage> DeleteFaculty(string personId, string facultyId)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            Person person = await Person.GetInstanceAsync(personId);
            Faculty faculty = await Faculty.GetInstanceAsync(facultyId);
            if (person == null || faculty == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.PERSON_COMMON_PERMISSION);
            if (commonRight)
            {
                if (await person.RemoveOnFaculty(facultyId))
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // удалить пользователя из указанной кафедры (администратор)
        [HttpDelete]
        [Route("api/People/{personId}/department/{departmentId}")]
        public async Task<HttpResponseMessage> DeleteDepartment(string personId, string departmentId)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            Person person = await Person.GetInstanceAsync(personId);
            Department department = await Department.GetInstanceAsync(departmentId);
            if (person == null || department == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав на операцию
            bool commonRight = authProvider.CheckPermission(Permission.PERSON_COMMON_PERMISSION);
            if (commonRight)
            {
                if (await person.RemoveOnDepartment(departmentId))
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        [HttpPost]
        [Route("api/People/UpdateToken")]
        public async Task<dynamic> UpdateToken()
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            Person person = await Person.GetInstanceAsync(authProvider.PersonId);
            if (await person.UpdateToken())
                return Request.CreateResponse(HttpStatusCode.OK);
            else
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
