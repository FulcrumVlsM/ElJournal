using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Student
    {
        public string ID { get; set; }
        public string PersonId { get; set; }
        public string GroupId { get; set; }
        public string SemesterId { get; set; }


        /// <summary>
        /// Возвращает студент-группа-семестр по id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public static async Task<Student> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from StudentsGroupsSemesters where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new Student
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        PersonId = obj.ContainsKey("PersonID") ? obj["PersonID"].ToString() : null,
                        GroupId = obj.ContainsKey("GroupsID") ? obj["GroupsID"] : null,
                        SemesterId = obj.ContainsKey("SemesterID") ? obj["SemesterID"] : null
                    };
                else
                    return null;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return null;
            }
        }

        /// <summary>
        /// Возвращает данные по всем семестрам, в которых находится данный пользователь
        /// </summary>
        /// <param name="personId">id пользователя</param>
        /// <returns></returns>
        public static async Task<List<Student>> GetStudents(string personId)
        {
            string sqlQuery = "select * from dbo.GetAllStudentGroupsSemesters(@personId)";
            var parameters = new Dictionary<string, string>
            {
                {"@personId", personId }
            };
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                List<Student> students = Student.ToStudents(result);
                return students;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return new List<Student>(1);
            }

        }

        /// <summary>
        /// Возвращает список студентов группы в указанном семестре
        /// </summary>
        /// <param name="semesterId">id семестра</param>
        /// <param name="groupId">id группы</param>
        /// <returns></returns>
        public static async Task<List<Student>> GetGroupStudent(string semesterId, string groupId)
        {
            string sqlQuery = "select * from dbo.GetStudentsByGroup(@groupId,@semesterId)";
            var parameters = new Dictionary<string, string>
            {
                {"@groupId", groupId },
                {"@semesterId", semesterId }
            };
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                return ToStudents(result);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return new List<Student>(1);
            }

        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static List<Student> ToStudents(List<Dictionary<string, dynamic>> objects)
        {
            if (objects.Count == 0)
                return null;
            else
            {
                var subjects = new List<Student>(objects.Count);
                foreach (var obj in objects)
                {
                    subjects.Add(new Student
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        PersonId = obj.ContainsKey("PersonID") ? obj["PersonID"].ToString() : null,
                        GroupId = obj.ContainsKey("GroupsID") ? obj["GroupsID"] : null,
                        SemesterId = obj.ContainsKey("SemesterID") ? obj["SemesterID"] : null
                    });
                }
                return subjects;
            }
        }

        /// <summary>
        /// Сохраняет текущий объект StudentGroupSemester в БД
        /// </summary>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string sqlQuery1 = "select ID from GroupsSemesters where GroupsID=@groupId and SemesterID=@semesterId";
            string procName = "dbo.AddStudent"; //процедура для добавления сущности в БД
            var parameters = new Dictionary<string, string>
            {
                {"@personId", PersonId },
                {"@groupId", GroupId },
                {"@semesterId", SemesterId }
            };
            try
            {
                DB db = DB.GetInstance();
                string groupSemester = await db.ExecuteScalarQueryAsync(sqlQuery1, parameters);
                if (!string.IsNullOrEmpty(groupSemester))
                {
                    parameters.Add("@groupSemesterId", groupSemester);
                    return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
                }
                return false;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return false;
            }
        }

        /// <summary>
        /// Удаление текущего объекта из БД
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            string procName = "dbo.DeleteStudent";
            var parameters = new Dictionary<string, string>
            {
                { "@id", ID }
            };
            try
            {
                DB db = DB.GetInstance();
                int result = db.ExecStoredProcedure(procName, parameters);
                if (result == 1)
                {
                    ID = null;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return false;
            }
        }
    }


    public class StudentFlowSubject
    {
        public string ID { get; set; }
        public string StudentId { get; set; }
        public string FlowSubjectId { get; set; }


        /// <summary>
        /// Возвращает студент-поток-предмет по id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public static async Task<StudentFlowSubject> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from StudentsFlowsSubjects where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new StudentFlowSubject
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        StudentId = obj.ContainsKey("StudentGroupSemesterID") ? obj["StudentGroupSemesterID"].ToString() : null,
                        FlowSubjectId = obj.ContainsKey("FlowSubjectID") ? obj["FlowSubjectID"] : null
                    };
                else
                    return null;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return null;
            }
        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<StudentFlowSubject> ToStudentFlowSubject(List<Dictionary<string, dynamic>> list)
        {
            if (list.Count == 0)
                return null;
            else
            {
                var students = new List<StudentFlowSubject>(list.Count);
                foreach (var obj in list)
                {
                    students.Add(new StudentFlowSubject
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        StudentId = obj.ContainsKey("StudentGroupSemesterID") ? obj["StudentGroupSemesterID"].ToString() : null,
                        FlowSubjectId = obj.ContainsKey("FlowSubjectID") ? obj["FlowSubjectID"] : null
                    });
                }
                return students;
            }
        }

        /// <summary>
        /// Возвращает полный список студент-поток-предмет
        /// </summary>
        /// <returns></returns>
        public static async Task<List<StudentFlowSubject>> GetCollectionAsync()
        {
            string sqlQuery = "select * from StudentsFlowsSubjects";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var subjects = ToStudentFlowSubject(result);
                return subjects;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<StudentFlowSubject>();
            }
        }

        /// <summary>
        /// Сохраняет текущий объект StudentFlowSubject в БД
        /// </summary>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddStudentFlowSubject"; //процедура для добавления сущности в БД
            var parameters = new Dictionary<string, string>
            {
                {"@studentId", StudentId },
                {"@flowSubjectId", FlowSubjectId }
            };
            try
            {
                DB db = DB.GetInstance();
                return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return false;
            }
        }

        /// <summary>
        /// Удаление текущего объекта из БД
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            string procName = "dbo.DeleteStudentFlowSubject";
            var parameters = new Dictionary<string, string>
            {
                { "@id", ID }
            };
            try
            {
                DB db = DB.GetInstance();
                int result = db.ExecStoredProcedure(procName, parameters);
                if (result == 1)
                {
                    ID = null;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return false;
            }
        }
    }
}