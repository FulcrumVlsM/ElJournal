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

    public class FlowSubject
    {
        public string ID { get; set; }
        public string FlowId { get; set; }
        public string TeacherId { get; set; }
        public string Teacher2Id { get; set; }
        public string Teacher3Id { get; set; }
        public string SubjectId { get; set; }
        public string SemesterId { get; set; }


        /// <summary>
        /// Возвращает предмет-группа-семестр по id
        /// </summary>
        /// <param name="id">id предмета</param>
        /// <returns></returns>
        public static async Task<FlowSubject> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from FlowsSubjects where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new FlowSubject
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        FlowId = obj.ContainsKey("FlowID") ? obj["FlowID"].ToString() : null,
                        TeacherId = obj.ContainsKey("TeacherID") ? obj["TeacherID"].ToString() : null,
                        SubjectId = obj.ContainsKey("SubjectID") ? obj["SubjectID"].ToString() : null,
                        SemesterId = obj.ContainsKey("SemesterID") ? obj["SemesterID"].ToString() : null,
                        Teacher2Id = obj.ContainsKey("Teacher2ID") ? obj["Teacher2ID"].ToString() : null,
                        Teacher3Id = obj.ContainsKey("Teacher3ID") ? obj["Teacher3ID"].ToString() : null
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
        public static async Task<List<FlowSubject>> GetCollectionAsync()
        {
            string sqlQuery = "select * from FlowsSubjects";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var subjects = ToSubjects(result);
                return subjects;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<FlowSubject>();
            }
        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="subjectList"></param>
        /// <returns></returns>
        public static List<FlowSubject> ToSubjects(List<Dictionary<string, dynamic>> subjectList)
        {
            if (subjectList.Count == 0)
                return null;
            else
            {
                var subjects = new List<FlowSubject>(subjectList.Count);
                foreach (var obj in subjectList)
                {
                    subjects.Add(new FlowSubject
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        FlowId = obj.ContainsKey("FlowID") ? obj["FlowID"].ToString() : null,
                        TeacherId = obj.ContainsKey("TeacherID") ? obj["TeacherID"].ToString() : null,
                        SubjectId = obj.ContainsKey("SubjectID") ? obj["SubjectID"].ToString() : null,
                        SemesterId = obj.ContainsKey("SemesterID") ? obj["SemesterID"].ToString() : null,
                        Teacher2Id = obj.ContainsKey("Teacher2ID") ? obj["Teacher2ID"].ToString() : null,
                        Teacher3Id = obj.ContainsKey("Teacher3ID") ? obj["Teacher3ID"].ToString() : null
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
            string procName = "dbo.AddFlowSubject"; //процедура для добавления сущности в БД
            var parameters = new Dictionary<string, string>
            {
                {"@flowId", FlowId },
                {"@teacherId", TeacherId },
                {"@subjectId", SubjectId },
                {"@semesterId", SemesterId },
                {"@teacher2Id", Teacher2Id },
                {"@teacher3Id", Teacher3Id }
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
            string procName = "dbo.DeleteFlowSubject";
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