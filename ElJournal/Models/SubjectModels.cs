using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Subject
    {
        public string ID { get; set; }
        public string DepartmentID { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }

        /// <summary>
        /// Возвращает предмет по id
        /// </summary>
        /// <param name="id">id предмета</param>
        /// <returns></returns>
        public static async Task<Subject> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from Subjects where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (result != null)
                    return new Subject
                    {
                        ID = result.ContainsKey("ID") ? result["ID"].ToString() : null,
                        Name = result.ContainsKey("name") ? result["name"].ToString() : null,
                        DepartmentID = result.ContainsKey("DepartmentID") ? result["DepartmentID"].ToString() : null,
                        Info = result.ContainsKey("info") ? result["info"].ToString() : null
                    };
                else
                    return null;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return null;
            }
        }

        /// <summary>
        /// Возвращает полный список групп
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Subject>> GetCollectionAsync()
        {
            string sqlQuery = "select * from Subjects";
            DB db = DB.GetInstance();
            var result = await db.ExecSelectQueryAsync(sqlQuery);
            var labWorks = new List<Subject>(result.Count);
            foreach (var obj in result)
            {
                labWorks.Add(new Subject
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                    Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                    DepartmentID = obj.ContainsKey("DepartmentID") ? obj["DepartmentID"] : null
                });
            }

            return labWorks;
        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="groupList"></param>
        /// <returns></returns>
        public static List<Subject> ToSubjects(List<Dictionary<string, dynamic>> groupList)
        {
            if (groupList.Count == 0)
                return null;
            else
            {
                var subjects = new List<Subject>(groupList.Count);
                foreach (var obj in groupList)
                {
                    subjects.Add(new Subject
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                        DepartmentID = obj.ContainsKey("DepartmentID") ? obj["DepartmentID"].ToString() : null,
                        Info = obj.ContainsKey("info") ? obj["info"].ToString() : null
                    });
                }
                return subjects;
            }
        }

        /// <summary>
        /// Сохраняет текущий объект Group в БД
        /// </summary>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddSubject ";
            var parameters = new Dictionary<string, string>
            {
                { "@name", Name },
                { "@departmentId", DepartmentID },
                { "@info", Info }
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
        /// Обновляет в БД выбранный объект (по ID)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Update()
        {
            string procName = "dbo.UpdateSubjects";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@name", Name },
                { "@info", Info },
                { "@DepartmentID", DepartmentID }
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
            string procName = "dbo.DeleteSubject";
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

    public class SubjectGroupSemester
    {
        public string ID
        {
            get { return _ID; }
            set
            {
                DB db = DB.GetInstance();
                string sqlQuery = "select dbo.GetGroupSemester(@semesterId,@groupId)";
                var parameters = new Dictionary<string, string>
                {
                    { "@groupId", GroupId },
                    { "@semesterId", SemesterId }
                };
                _groupSemesterId = db.ExecuteScalarQueryAsync(sqlQuery, parameters).Result;
                _ID = value;
            }
        }
        public string GroupId { get; set; }
        public string SemesterId { get; set; }
        public string SubjectId { get; set; }
        public string TeacherPersonId { get; set; }

        private string _ID;
        private string _groupSemesterId;


        /// <summary>
        /// Возвращает предмет-группа-семестр по id
        /// </summary>
        /// <param name="id">id предмета</param>
        /// <returns></returns>
        public static async Task<SubjectGroupSemester> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from SubjectsGroupsSemesters where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new SubjectGroupSemester
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        GroupId = obj.ContainsKey("GroupsID") ? obj["GroupsID"].ToString() : null,
                        SemesterId = obj.ContainsKey("SemesterID") ? obj["SemesterID"] : null,
                        SubjectId = obj.ContainsKey("SubjectID") ? obj["SubjectID"] : null,
                        TeacherPersonId = obj.ContainsKey("teacherPersonID") ? obj["teacherPersonID"] : null
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
        /// Возвращает полный список предмет-группа-семестр
        /// </summary>
        /// <returns></returns>
        public static async Task<List<SubjectGroupSemester>> GetCollectionAsync()
        {
            string sqlQuery = "select * from dbo.GetSubjectsGroupsSemesters";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var subjects = new List<SubjectGroupSemester>(result.Count);
                foreach (var obj in result)
                {
                    subjects.Add(new SubjectGroupSemester
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        GroupId = obj.ContainsKey("GroupsID") ? obj["GroupsID"].ToString() : null,
                        SemesterId = obj.ContainsKey("SemesterID") ? obj["SemesterID"] : null,
                        SubjectId = obj.ContainsKey("SubjectID") ? obj["SubjectID"] : null,
                        TeacherPersonId = obj.ContainsKey("teacherPersonID") ? obj["teacherPersonID"] : null
                    });
                }

                return subjects;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<SubjectGroupSemester>();
            }
        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="subjectList"></param>
        /// <returns></returns>
        public static List<SubjectGroupSemester> ToSubjects(List<Dictionary<string, dynamic>> subjectList)
        {
            if (subjectList.Count == 0)
                return null;
            else
            {
                var subjects = new List<SubjectGroupSemester>(subjectList.Count);
                foreach (var obj in subjectList)
                {
                    subjects.Add(new SubjectGroupSemester
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        GroupId = obj.ContainsKey("GroupsID") ? obj["GroupsID"].ToString() : null,
                        SemesterId = obj.ContainsKey("SemesterID") ? obj["SemesterID"] : null,
                        SubjectId = obj.ContainsKey("SubjectID") ? obj["SubjectID"] : null,
                        TeacherPersonId = obj.ContainsKey("teacherPersonID") ? obj["teacherPersonID"] : null
                    });
                }
                return subjects;
            }
        }

        /// <summary>
        /// Сохраняет текущий объект SubjectsGroupsSemesters в БД
        /// </summary>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddSubjectGroupSemester"; //процедура для добавления сущности в БД
            var parameters = new Dictionary<string, string>
            {
                {"@SubjectId", SubjectId },
                {"@TeacherId", TeacherPersonId },
                {"@GroupSemesterId", _groupSemesterId }
            };
            try
            {
                DB db = DB.GetInstance();
                if (!string.IsNullOrEmpty(_groupSemesterId))
                    return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
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

        /// <summary>
        /// Удаление текущего объекта из БД
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            string procName = "delete from SubjectsGroupsSemesters where GroupSemestrID=@groupSemesterId and SubjectID=@subjectId " +
                " and teacherPersonID=@teacherId";
            var parameters = new Dictionary<string, string>
            {
                { "@groupSemesterId", _groupSemesterId },
                {"@subjectId", SubjectId },
                {"@teacherId", TeacherPersonId }
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