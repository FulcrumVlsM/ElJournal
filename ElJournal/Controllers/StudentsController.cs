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
        //TODO: студент имеет следующие данные:
        /*
         * группа, в которой он учится (по таблице groups)
         * семестры, в течение которых он учится в той группе (по таблице GroupsSemesters)
         * ID (по таблице StudentsGroupSemesters)
         * номер зачетной книжки (по таблице Persons)
         */

        private const string STUDENT_ALL_PERMISSION = "STUDENT_ALL_PERMISSION";

        // GET: api/Students
        public async Task<dynamic> Get()
        {
            return null;
        }

        // GET: api/Students/5
        public async Task<dynamic> Get(string id)
        {
            Response response = new Response();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string sqlQuery = "select * from GetStudentInfo(@PersonID)";

            try
            {
                DB db = DB.GetInstance();
                parameters.Add("@PersonID", id);
                response.Data = await db.ExecSelectQuery(sqlQuery, parameters);
                response.Succesful = true;
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }

        // POST: api/Students
        public async Task<dynamic> Post([FromBody]Student student)
        {
            Response response = new Response();//формат ответа
            string sqlQuery = "insert into StudentsGroupsSemesters(PersonID,GroupSemesterID)" +
                " values(@PersonID,@GroupSemesterID)";
            string sqlQuery2 = "dbo.GetGroupSemester(@SemesterID, @GroupID)";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string authorId = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(authorId, STUDENT_ALL_PERMISSION);
                if (right)
                {
                    parameters.Add("@SemesterID", student.semesterId);
                    parameters.Add("@GroupID", student.groupId);
                    string groupsSemester = await db.ExecuteScalarQuery(sqlQuery2, parameters);

                    parameters.Clear();
                    parameters.Add("@PersonID", student.personId ?? String.Empty);
                    parameters.Add("@GroupSemesterID", groupsSemester ?? String.Empty);
                    int res = db.ExecInsOrDelQuery(sqlQuery, parameters);
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

        // PUT: api/Students/5
        public async Task<dynamic> Put(string id, [FromBody]Student student)
        {
            Response response = new Response();//формат ответа
            string sqlQuery = "dbo.UpdateStudentGroupSemester";
            string sqlQuery2 = "dbo.GetGroupSemester(@SemesterID, @GroupID)";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string authorId = Request?.Headers?.Authorization?.Scheme; //id пользователя из заголовка http

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(authorId, STUDENT_ALL_PERMISSION);
                if (right)
                {
                    parameters.Add("@SemesterID", student.semesterId);
                    parameters.Add("@GroupID", student.groupId);
                    string groupsSemester = await db.ExecuteScalarQuery(sqlQuery2, parameters);

                    parameters.Clear();
                    parameters.Add("@ID", id);
                    parameters.Add("@GroupSemesterID", groupsSemester);
                    parameters.Add("@PersonID", student.personId);
                    int res = db.ExecStoredProcedure(sqlQuery, parameters);
                    if (res == 0)
                    {
                        response.Succesful = true;
                        response.message = "Student was changed";
                    }
                    else
                        response.message = "Student wasn't changed";
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
        //TODO: метод еще не готов
        public async Task<dynamic> Delete(int id)
        {
            return null;
        }
    }
}
