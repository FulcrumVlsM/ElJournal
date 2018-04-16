using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Providers;
using ElJournal.Models;
using System.Text.RegularExpressions;
using ElJournal.DBInteract;

namespace ElJournal.Controllers
{
    public class SubjectsController : ApiController
    {
        //получить общий список предметов (все)
        //получить конкретный предмет по id (все)
        //получить список предметов с преподавателями в указанном семестре
                        //(предмет-группа-семестр) (все рег. пользователи)

        //добавить новый предмет (администратор, администратор кафедры)
        //установить предмет для группы в указанном семестре (администратор, администратор кафедры)

        //изменить информацию о предмете (администратор, администратор кафедры)
        //изменить преподавателя по предмету для группы в указанном семестре (администратор, администратор кафедры)

        //удалить предмет из плана для группы в указанном семестре (администратор, администратор кафедры)
        //удалить предмет (администратор, администратор кафедры)


        // GET: api/Subjects
        //получить общий список предметов (все)
        public async Task<HttpResponseMessage> Get([FromUri]string name = null, [FromUri]string department = null)
        {
            Response response = new Response();
            var subjects = await Subject.GetCollectionAsync();
            if (!string.IsNullOrEmpty(department))
                subjects = subjects.FindAll(x => string.Compare(x.Name, department) == 0);
            if(!string.IsNullOrEmpty(name))
            {
                Regex regex = new Regex(name);
                subjects = subjects.FindAll(x => regex.IsMatch(x.Name));
            }
            response.Data = subjects;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // GET: api/Subjects/5
        //получить конкретный предмет по id (все)
        [HttpGet]
        [Route("api/Subjects/{id}")]
        public async Task<HttpResponseMessage> GetConcrete(string id)
        {
            Response response = new Response();
            response.Data = await Subject.GetInstanceAsync(id);
            if(response.Data != null)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        // GET: api/Subjects/ByGroup/5
        //получить список предметов с преподавателями в указанном семестре (все)
        [HttpGet]
        [Route("api/Subjects/SubjectGroup/{semesterId}")]
        public async Task<HttpResponseMessage> GetSubjectGroup(string semesterId, [FromUri]string department = null,
            [FromUri]string groupId = null)
        {
            Response response = new Response();
            var subjects = await SubjectGroupSemester.GetCollectionAsync();
            subjects = subjects.FindAll(x => string.Compare(x.SemesterId, semesterId) == 0); //фильтр по семестру
            if (department != null) //фильтр по кафедре
                subjects = subjects.FindAll(x =>
                {
                    Subject subject = Subject.GetInstanceAsync(x.SubjectId).Result;
                    return string.Compare(subject.DepartmentID, department) == 0;
                });
            if (string.IsNullOrEmpty(groupId)) //фильтр по группе
                subjects = subjects.FindAll(x => string.Compare(x.GroupId, groupId) == 0);

            response.Data = subjects;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // POST: api/Subjects
        //добавить новый предмет (администратор, администратор кафедры)
        public async Task<HttpResponseMessage> Post([FromBody]Subject subject)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                //првоерка наличия прав доступа
                bool commonRight = default(bool),
                    departRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.GROUP_COMMON_PERMISSION),
                    () => departRight = authProvider.CheckPermission(Permission.GROUP_PERMISSION) ?
                                         authProvider.Departments.Contains(subject.DepartmentID) : false);

                if(commonRight || departRight)
                {
                    if (await subject.Push())
                        return Request.CreateResponse(HttpStatusCode.Created);
                    else
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }


        // POST: api/Subjects
        //установить предмет для группы в указанном семестре (администратор, администратор кафедры)
        [HttpPost]
        [Route("api/Subjects/SubjectGroup")]
        public async Task<dynamic> PostPlan([FromBody]SubjectGroupSemester subjectGroupSemester)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            //поиск предмета с указанным id
            Subject subject = await Subject.GetInstanceAsync(subjectGroupSemester.GroupId);
            if (subject == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            if (authProvider != null)
            {
                //проверка наличия прав доступа
                bool commonRight = default(bool),
                    departRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.GROUP_COMMON_PERMISSION),
                    () => departRight = authProvider.CheckPermission(Permission.GROUP_PERMISSION) ?
                                         authProvider.Departments.Contains(subject.DepartmentID) : false);

                if (commonRight || departRight)
                {
                    if(await subjectGroupSemester.Push())
                        return Request.CreateResponse(HttpStatusCode.Created);
                    else
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }


        // PUT: api/Subjects/5
        //изменить информацию о предмете (администратор, администратор кафедры)
        [HttpPut]
        [Route("api/Subjects/{id}")]
        public async Task<HttpResponseMessage> Put(string id, [FromBody]Subject subject)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);

            if (authProvider != null)
            {
                //првоерка наличия прав доступа
                bool commonRight = default(bool),
                    departRight = default(bool);
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.GROUP_COMMON_PERMISSION),
                    () => departRight = authProvider.CheckPermission(Permission.GROUP_PERMISSION) ?
                                         authProvider.Departments.Contains(subject.DepartmentID) : false);

                if (commonRight || departRight)
                {
                    subject.ID = id;
                    if (await subject.Update())
                        return Request.CreateResponse(HttpStatusCode.OK);
                    else
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                else
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }


        // DELETE: api/Subjects/plan/5
        //удалить предмет из плана для группы в указанном семестре (администратор, администратор кафедры)
        [HttpDelete]
        [Route("api/Subjects/SubjectGroup/{id}")]
        public async Task<HttpResponseMessage> DeletePlan(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if(authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск предмет-группа-семестр c указанным id
            SubjectGroupSemester subjectGroupSemester = await SubjectGroupSemester.GetInstanceAsync(id);
            if(subjectGroupSemester==null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //поиск предмета с указанным id
            Subject subject = await Subject.GetInstanceAsync(subjectGroupSemester.SubjectId);
            if (subject == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);


            //проверка наличия прав доступа
            bool commonRight = default(bool),
                departRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.GROUP_COMMON_PERMISSION),
                () => departRight = authProvider.CheckPermission(Permission.GROUP_PERMISSION) ?
                                     authProvider.Departments.Contains(subject.DepartmentID) : false);

            if (commonRight || departRight)
            {
                if (subjectGroupSemester.Delete())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // DELETE: api/Subjects/5
        // удалить предмет (администратор, администратор кафедры)
        [HttpDelete]
        [Route("api/Subjects/{id}")]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if(authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            Subject subject = await Subject.GetInstanceAsync(id);
            if(subject == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //првоерка наличия прав доступа
            bool commonRight = default(bool),
                departRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.GROUP_COMMON_PERMISSION),
                () => departRight = authProvider.CheckPermission(Permission.GROUP_PERMISSION) ?
                                     authProvider.Departments.Contains(subject.DepartmentID) : false);

            if (commonRight || departRight)
            {
                if (subject.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}
