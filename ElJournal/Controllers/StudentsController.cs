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
        private static string table1 = "StudentsGroupsSemesters";
        private static string table2 = "People";
        private static string table3 = "Groups";

        // GET: api/Students
        public dynamic Get()
        {
            return null;
        }

        // GET: api/Students/5
        public async Task<dynamic> Get(string id)
        {
            //общие данные пользователя
            //группа
            Response response = new Response();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            //получение id пользователя
            string sqlQuery1 = string.Format("select PersonID from {0} where ID=@studentId",table1);
            //получение id ГруппыСеместр
            string sqlQuery2 = string.Format("select GroupSemesterID from {0} where ID=@studentId", table1);
            //получение данных пользователя
            string sqlQuery3 = string.Format("select ID,name from {0} where ID=@personId", table2);
            //получение данных группы
            string sqlQuery4 = string.Format("select ID,name,info from {0} where ID=dbo.GetGroup(@groupSemester)",
                table3);

            try
            {
                DB db = DB.GetInstance();
                parameters.Add("@studentId", id);
                parameters.Add("@personId", await db.ExecuteScalarQueryAsync(sqlQuery1, parameters));
                parameters.Add("@groupSemester", await db.ExecuteScalarQueryAsync(sqlQuery2, parameters));

                var person = (await db.ExecSelectQueryAsync(sqlQuery3, parameters))[0];
                var group = (await db.ExecSelectQueryAsync(sqlQuery4, parameters))[0];

                response.Data = new //формирование результата запроса
                {
                    person = new Person //информация о пользователе
                    {
                        ID = person["ID"],
                        name = person["name"]
                    },
                    group = new Group //информация о группе
                    {
                        ID = group["ID"],
                        name = group["name"]
                    }
                };
                response.Succesful = true;
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
                //TODO: add log
            }

            return response;
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
