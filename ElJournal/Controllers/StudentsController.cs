using System;
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

        private static string table1 = "StudentsGroupsSemesters";
        private static string table2 = "People";
        private static string table3 = "Groups";

        // GET: api/Students/user/5
        // получить id студента для всех семестров по указанному пользователю (все)
        [HttpGet]
        [Route("api/Students/user/{personId}")]
        public async Task<HttpResponseMessage> GetByUser(string personId)
        {
            Response response = new Response();
            response.Data = await Student.GetStudents(personId);
            if (response.Data.Count > 0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        // GET: api/Students/group/5/5
        // получить студентов указанной группы в указанном семестре (все)
        [HttpGet]
        [Route("api/Students/group/{semesterId}/{groupId}")]
        public async Task<HttpResponseMessage> GetByGroup(string semesterId, string groupId)
        {
            Response response = new Response();
            response.Data = await Student.GetGroupStudent(semesterId, groupId);
            if(response.Data.Count>0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // GET: api/Students/subject/5
        // получить записи на потоки по предметам по указанной группе и семестру (все)
        [HttpGet]
        [Route("api/Students/flow/{semesterId}")]
        public async Task<HttpResponseMessage> GetStudentsFlow(string semesterId, [FromUri]string student = null,
            string flowSubject = null)
        {
            Response response = new Response();
            string sqlQuery = "select * from dbo.GetStudentsFlowSubjects(@semesterId)";
            var parameters = new Dictionary<string, string>
            {
                {"@semesterId", semesterId }
            };
            DB db = DB.GetInstance();
            var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
            List<StudentFlowSubject> list = StudentFlowSubject.ToStudentFlowSubject(result);

            if (!string.IsNullOrEmpty(student))//фильтр по студенту
                list = list.FindAll(x => x.StudentId == student);
            if (!string.IsNullOrEmpty(flowSubject))//фильтр по поток-предмету
                list = list.FindAll(x => x.FlowSubjectId == flowSubject);

            response.Data = list;
            if (list.Count > 0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        // POST: api/Students
        public async Task<dynamic> Post([FromBody]Student student)
        {
            Response response = new Response();//формат ответа
            string sqlQuery = String.Format("insert into {0}(PersonID,GroupSemesterID)" +
                " values(@PersonID,@GroupSemesterID)", table1);
            string sqlQuery2 = "dbo.GetGroupSemester(@SemesterID, @GroupID)";
            string sqlQuery3 = "select dbo.CheckPersonGroupFaculty(@person,@group)";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string authorId = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http

            try
            {
                DB db = DB.GetInstance();
                parameters.Add("@person", authorId);
                parameters.Add("@group", student.GroupId);

                //проверка наличия необходимых разрешений
                bool facultyRight = default(bool), commonRight = default(bool);
                Parallel.Invoke(
                    async () => facultyRight = await db.CheckPermission(authorId, Permission.STUDENT_PERMISSION) ?
                    (bool)await db.ExecuteScalarQueryAsync(sqlQuery3, parameters) : false,
                    async () => commonRight = await db.CheckPermission(authorId, Permission.STUDENT_COMMON_PERMISSION));
                
                if (commonRight || facultyRight)
                {
                    //определение id записи ГруппаСеместр (GroupSemesters)
                    parameters.Clear();
                    parameters.Add("@SemesterID", student.SemesterId);
                    parameters.Add("@GroupID", student.GroupId);
                    string groupsSemester = await db.ExecuteScalarQueryAsync(sqlQuery2, parameters);

                    //добавление связи между человеком и группой
                    parameters.Clear();
                    parameters.Add("@PersonID", student.PersonId ?? String.Empty);
                    parameters.Add("@GroupSemesterID", groupsSemester ?? String.Empty);
                    int res = await db.ExecInsOrDelQueryAsync(sqlQuery, parameters);
                    if (res == 1)
                    {
                        response.Succesful = true;
                        response.message = "Student was added";
                    }
                    else
                        response.message = "Student wasn't added";
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
        public async Task<dynamic> Delete(string id)
        {
            Response response = new Response();//формат ответа
            string sqlQuery = String.Format("delete from {0} where ID=@id",table1);
            string sqlQuery2 = "select dbo.CheckPersonStudentFaculty(@person,@student)";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string authorId = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http

            try
            {
                DB db = DB.GetInstance();
                parameters.Add("@person", authorId);
                parameters.Add("@student", id);

                //проверка наличия необходимых разрешений
                bool facultyRight = await db.CheckPermission(authorId, Permission.STUDENT_PERMISSION) ?
                    (bool)await db.ExecuteScalarQueryAsync(sqlQuery2, parameters) : false;
                bool commonRight = await db.CheckPermission(authorId, Permission.STUDENT_COMMON_PERMISSION);

                if (commonRight || facultyRight)
                {
                    //удаление отношения
                    parameters.Clear();
                    parameters.Add("@id", id ?? String.Empty);
                    int res = db.ExecInsOrDelQuery(sqlQuery, parameters);
                    if (res == 1)
                    {
                        response.Succesful = true;
                        response.message = "Student was deleted";
                    }
                    else
                        response.message = "Student wasn't deleted";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
                //TODO: add log
            }

            return response;
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
