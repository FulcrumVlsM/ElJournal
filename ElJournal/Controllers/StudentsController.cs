﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Models;
using ElJournal.DBInteract;
using ElJournal.Providers;

namespace ElJournal.Controllers
{
    //develop: Mikhail
    public class StudentsController : ApiController
    {
        // получить id студента для всех семестров по указанному пользователю (все)
        // получить студентов указанной группы в указанном семестре (все)
        // добавить пользователя в группу в указанный семестр (администратор факультета, администратор)
        // удалить пользователя из группы в указанном семестре (администратор факультета, администратор)

        // получить записи на потоки по предметам по указанной группе и семестру (все) (с фильтрами)
        // добавить студента на поток по предмету (администратор кафедры, администратор)
        // удалить студента из потока по предмету (администратор, администратор кафедры)

        
        // GET: api/Students/user/5
        // получить id студента для всех семестров по указанному пользователю (все)
        [HttpGet]
        [Route("api/Students/user/{personId}")]
        public async Task<HttpResponseMessage> GetByUser(string personId)
        {
            Response response = new Response();
            var students = await Student.GetStudents(personId);
            response.Data = students;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        // GET: api/Students/group/5/5
        // получить студентов указанной группы в указанном семестре (все)
        [HttpGet]
        [Route("api/Students/group/{semesterId}/{groupId}")]
        public async Task<HttpResponseMessage> GetByGroup(string semesterId, string groupId)
        {
            Response response = new Response();
            response.Data = await Student.GetGroupStudent(semesterId, groupId);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // GET: api/Students/subject/5
        // получить записи на потоки по предметам по указанной группе и семестру (все)
        [HttpGet]
        [Route("api/Students/flow/{semesterId}/{groupId}")]
        public async Task<HttpResponseMessage> GetStudentsFlow(string semesterId, string groupId, [FromUri]string student = null,
            string flowSubject = null)
        {
            Response response = new Response();
            string sqlQuery = "select * from dbo.GetStudentsFlowSubjects(@semesterId, @groupId)";
            var parameters = new Dictionary<string, string>
            {
                {"@semesterId", semesterId },
                { "@groupId", groupId }
            };
            DB db = DB.GetInstance();
            var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
            List<StudentFlowSubject> list = StudentFlowSubject.ToStudentFlowSubject(result);

            if (!string.IsNullOrEmpty(student))//фильтр по студенту
                list = list.FindAll(x => x.StudentId == student);
            if (!string.IsNullOrEmpty(flowSubject))//фильтр по поток-предмету
                list = list.FindAll(x => x.FlowSubjectId == flowSubject);

            response.Data = list;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        //получить записи о регистрации на предмет по указанному предмету и/или студенту
        [HttpGet]
        [Route("api/Students/flow")]
        public async Task<HttpResponseMessage> GetStudentsFlow([FromUri]string flowSubjectId = null, 
            [FromUri]string studentId = null)
        {
            if (string.IsNullOrEmpty(flowSubjectId) && string.IsNullOrEmpty(studentId))
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            Response response = new Response();
            var studentsFlow = await StudentFlowSubject.GetCollectionAsync();
            if (!string.IsNullOrEmpty(flowSubjectId))
                studentsFlow = studentsFlow.FindAll(x => x.FlowSubjectId == flowSubjectId);
            if (!string.IsNullOrEmpty(studentId))
                studentsFlow = studentsFlow.FindAll(x => x.StudentId == studentId);
            response.Data = studentsFlow;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // получить зарегестрированных студентов на указанный предмет
        [HttpGet]
        [Route("api/Students/flow/{flowSubjectId}")]
        public async Task<HttpResponseMessage> GetStudentsFlow(string flowSubjectId)
        {
            Response response = new Response();
            var studentsFlow = (await StudentFlowSubject.GetCollectionAsync()).FindAll(x => x.FlowSubjectId == flowSubjectId);
            response.Data = studentsFlow;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // получить студента
        [HttpGet]
        [Route("api/Students/{id}")]
        public async Task<HttpResponseMessage> GetById(string id)
        {
            Response response = new Response();

            Student student = await Student.GetInstanceAsync(id);
            if (student == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            response.Data = student;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // POST: api/Students
        public async Task<HttpResponseMessage> Post([FromBody]Student student)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            Group group = await Group.GetInstanceAsync(student.GroupId);
            if(group == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав доступа
            bool commonRight = default(bool),
                facultyRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.STUDENT_COMMON_PERMISSION),
                () => facultyRight = authProvider.CheckPermission(Permission.STUDENT_PERMISSION) ?
                                     authProvider.Faculties.Contains(group?.FacultyId) : false);

            if(commonRight || facultyRight)
            {
                if(await student.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // добавить студента на поток по предмету (администратор факультета, администратор)
        [HttpPost]
        [Route("api/Students/flow")]
        public async Task<HttpResponseMessage> PostStudentFlow([FromBody]StudentFlowSubject studentFlowSubject)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            FlowSubject flow = await FlowSubject.GetInstanceAsync(studentFlowSubject.FlowSubjectId);
            Student student = await Student.GetInstanceAsync(studentFlowSubject.StudentId);
            if(flow == null || student == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            Group group = await Group.GetInstanceAsync(student.GroupId);

            //проверка наличия прав доступа
            bool commonRight = default(bool),
                facultyRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.GROUP_COMMON_PERMISSION),
                () => facultyRight = authProvider.CheckPermission(Permission.GROUP_PERMISSION) ?
                                     authProvider.Faculties.Contains(group?.FacultyId) : false);

            if(commonRight || facultyRight)
            {
                if(await studentFlowSubject.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // DELETE: api/Students/5
        [HttpDelete]
        [Route("api/Students/{id}")]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            Student student = await Student.GetInstanceAsync(id);
            if(student == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            Group group = await Group.GetInstanceAsync(student.GroupId);

            //проверка наличия прав доступа
            bool commonRight = default(bool),
                facultyRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.STUDENT_COMMON_PERMISSION),
                () => facultyRight = authProvider.CheckPermission(Permission.STUDENT_PERMISSION) ?
                                     authProvider.Faculties.Contains(group?.FacultyId) : false);

            if (commonRight || facultyRight)
            {
                if (student.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // удалить студента из потока по предмету (администратор, администратор кафедры)
        [HttpDelete]
        [Route("api/Students/flow/{id}")]
        public async Task<HttpResponseMessage> DeleteStudentFlow(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            StudentFlowSubject studentFlowSubject = await StudentFlowSubject.GetInstanceAsync(id);
            if(studentFlowSubject == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            Student student = await Student.GetInstanceAsync(studentFlowSubject.StudentId);
            if (student == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            Group group = await Group.GetInstanceAsync(student.GroupId);

            //проверка наличия прав доступа
            bool commonRight = default(bool),
                facultyRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.GROUP_COMMON_PERMISSION),
                () => facultyRight = authProvider.CheckPermission(Permission.GROUP_PERMISSION) ?
                                     authProvider.Faculties.Contains(group?.FacultyId) : false);

            if (commonRight || facultyRight)
            {
                if (studentFlowSubject.Delete())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}
