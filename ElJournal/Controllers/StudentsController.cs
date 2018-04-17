using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Models;
using ElJournal.DBInteract;

namespace ElJournal.Controllers
{
    //develop: Mikhail
    public class StudentsController : ApiController
    {
        // получить id студента для всех семестров по указанному пользователю (все)
        // получить студентов указанной группы в указанном семестре (все)
        // получить студентов указанного предмета с учетом игнор-списка (все)
        // добавить пользователя в группу в указанный семестр (администратор факультета, администратор)
        // установить игнорирование студента для данного предмета (администратор, администратор кафедры, преподаватель)
        // удалить пользователя из группы в указанном семестре (администратор факультета, администратор)
        // удалить игнорирование студента для данного предмета (администратор, администратор кафедры, преподаватель)

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
        // получить студентов указанного предмета с учетом игнор-списка (все)
        [HttpGet]
        [Route("api/Students/subject/{subjectGroupSemesterId}")]
        public async Task<HttpResponseMessage> GetBySubject(string subjectGroupSemesterId)
        {
            Response response = new Response();
            response.Data = await Student.GetSubjectStudents(subjectGroupSemesterId);
            if (response.Data.Count > 0)
                return Request.CreateResponse(HttpStatusCode.OK, response);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);

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
                parameters.Add("@group", student.groupId);

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
                    parameters.Add("@SemesterID", student.semesterId);
                    parameters.Add("@GroupID", student.groupId);
                    string groupsSemester = await db.ExecuteScalarQueryAsync(sqlQuery2, parameters);

                    //добавление связи между человеком и группой
                    parameters.Clear();
                    parameters.Add("@PersonID", student.personId ?? String.Empty);
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
    }
}
