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
        // получить общий список предметов (все)
        // получить конкретный предмет по id (все)
        // добавить новый предмет (администратор, администратор кафедры)
        // изменить информацию о предмете (администратор, администратор кафедры)
        // удалить предмет (администратор, администратор кафедры)

        // получить общий список поток-предмет-семестр (все)
        // добавить поток-предмет-семестр (администратор, администратор кафедры)
        // удалить поток-предмет-семестр (администратор, администратор кафедры)


        // GET: api/Subjects
        //получить общий список предметов (все)
        public async Task<HttpResponseMessage> Get([FromUri]string name = null, [FromUri]string department = null)
        {
            Response response = new Response();
            var subjects = await Subject.GetCollectionAsync();
            if (!string.IsNullOrEmpty(department))
                subjects = subjects.FindAll(x => x.DepartmentID == department);
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


        // получить общий список поток-предмет-семестр (все)
        [HttpGet]
        [Route("api/Subjects/flow")]
        public async Task<HttpResponseMessage> GetSubjectFlow([FromUri]string flow = null, [FromUri]string teacher = null,
            [FromUri]string subject = null, [FromUri]string semester = null)
        {
            Response response = new Response();
            var flowsSubjects = await FlowSubject.GetCollectionAsync();

            if (!string.IsNullOrEmpty(flow)) //отбор по потоку
                flowsSubjects = flowsSubjects.FindAll(x => x.FlowId == flow);

            if (!string.IsNullOrEmpty(teacher)) //отбор по преподавателю
                flowsSubjects = flowsSubjects.FindAll(x => x.TeacherId == teacher);

            if (!string.IsNullOrEmpty(subject)) //отбор по предмету
                flowsSubjects = flowsSubjects.FindAll(x => x.SubjectId == subject);

            if (!string.IsNullOrEmpty(semester)) //отбор по семестру
                flowsSubjects = flowsSubjects.FindAll(x => x.SemesterId == semester);

            response.Data = flowsSubjects;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        
        //получить предмет-поток
        [HttpGet]
        [Route("api/Subjects/flow/{flowSubjectId}")]
        public async Task<HttpResponseMessage> GetSubjectFlow(string flowSubjectId)
        {
            Response response = new Response();

            FlowSubject fSubject = await FlowSubject.GetInstanceAsync(flowSubjectId);
            if (fSubject == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            response.Data = fSubject;
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
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.DEPARTMENT_COMMON_PERMISSION),
                    () => departRight = authProvider.CheckPermission(Permission.DEPARTMENT_PERMISSION) ?
                                         authProvider.Departments.Contains(subject.DepartmentID) : false);

                if(commonRight || departRight)
                {
                    if (await subject.Push())
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


        // добавить поток-предмет-семестр (администратор, администратор кафедры)
        [HttpPost]
        [Route("api/Subjects/flow")]
        public async Task<HttpResponseMessage> PostFlowSubject([FromBody]FlowSubject flowSubject)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if(authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск предмета
            Subject subject = await Subject.GetInstanceAsync(flowSubject.SubjectId);
            if(subject == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав доступа
            bool commonRight = default(bool),
                departRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.DEPARTMENT_COMMON_PERMISSION),
                () => departRight = authProvider.CheckPermission(Permission.DEPARTMENT_PERMISSION) ?
                                     authProvider.Departments.Contains(subject.DepartmentID) : false);

            if (commonRight || departRight)
            {
                if (await flowSubject.Push())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
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
                Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.DEPARTMENT_COMMON_PERMISSION),
                    () => departRight = authProvider.CheckPermission(Permission.DEPARTMENT_PERMISSION) ?
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
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.DEPARTMENT_COMMON_PERMISSION),
                () => departRight = authProvider.CheckPermission(Permission.DEPARTMENT_PERMISSION) ?
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


        // удалить поток-предмет-семестр (администратор, администратор кафедры)
        [HttpDelete]
        [Route("api/Subjects/flow/{id}")]
        public async Task<HttpResponseMessage> DeleteFlowSubject(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск поток-предмета
            FlowSubject flowSubject = await FlowSubject.GetInstanceAsync(id);
            if(flowSubject == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //поиск предмета
            Subject subject = await Subject.GetInstanceAsync(flowSubject.SubjectId);
            if (subject == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав доступа
            bool commonRight = default(bool),
                departRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.DEPARTMENT_COMMON_PERMISSION),
                () => departRight = authProvider.CheckPermission(Permission.DEPARTMENT_PERMISSION) ?
                                     authProvider.Departments.Contains(subject.DepartmentID) : false);

            if (commonRight || departRight)
            {
                if (flowSubject.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}
