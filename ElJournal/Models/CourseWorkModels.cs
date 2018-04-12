using ElJournal.DBInteract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class CourseWork
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Advanced { get; set; }
        public string FileURL { get; set; }
        public string FileName { get; set; }


        /// <summary>
        /// Возвращает курсовую работу по указанному ID
        /// </summary>
        /// <param name="id">id курсовой работы</param>
        /// <returns></returns>
        public static async Task<CourseWork> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from CourseWorks where ID=@id";
            var parameters = new Dictionary<string, string>();
            parameters.Add("@id", id);
            DB db = DB.GetInstance();
            var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
            if (result != null)
                return new CourseWork
                {
                    ID = result.ContainsKey("ID") ? result["ID"].ToString() : string.Empty,
                    Name = result.ContainsKey("name") ? result["name"].ToString() : string.Empty,
                    FileName = result.ContainsKey("fileName") ? result["fileName"].ToString() : string.Empty,
                    Advanced = result.ContainsKey("advanced") ? result["advanced"].ToString() : string.Empty,
                    Description = result.ContainsKey("description") ? result["description"].ToString() : string.Empty
                };
            else
                return null;
        }


        /// <summary>
        /// Возвращает полный список курсовых работ
        /// </summary>
        /// <returns></returns>
        public static async Task<List<CourseWork>> GetCollectionAsync()
        {
            string sqlQuery = "select * from CourseWorks";
            DB db = DB.GetInstance();
            var result = await db.ExecSelectQueryAsync(sqlQuery);
            var labWorks = new List<CourseWork>(result.Count);
            foreach (var obj in result)
            {
                labWorks.Add(new CourseWork
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : string.Empty,
                    Name = obj.ContainsKey("name") ? obj["name"].ToString() : string.Empty,
                    FileName = obj.ContainsKey("fileName") ? obj["fileName"].ToString() : string.Empty,
                    FileURL = obj.ContainsKey("fileURL") ? obj["fileURL"].ToString() : string.Empty,
                    Advanced = obj["advanced"].ToString()
                });
            }

            return labWorks;
        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="works"></param>
        /// <returns></returns>
        public static List<CourseWork> ToCourseWork(List<Dictionary<string, dynamic>> works)
        {
            var labWorks = new List<CourseWork>(works.Count);
            foreach (var obj in works)
            {
                labWorks.Add(new CourseWork
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : string.Empty,
                    Name = obj.ContainsKey("name") ? obj["name"].ToString() : string.Empty,
                    FileName = obj.ContainsKey("fileName") ? obj["fileName"].ToString() : string.Empty,
                    FileURL = obj.ContainsKey("fileURL") ? obj["fileURL"].ToString() : string.Empty,
                    Advanced = obj.ContainsKey("advanced") ? obj["advanced"].ToString() : string.Empty,
                    Description = obj.ContainsKey("description") ? obj["description"].ToString() : string.Empty
                });
            }

            return labWorks;
        }

        /// <summary>
        /// Возвращает курсовую работу по id плана
        /// </summary>
        /// <param name="planId">id плана</param>
        /// <returns></returns>
        public static async Task<CourseWork> GetCWorkFromPlan(string planId)
        {
            string sqlQuery = "select ID, name from dbo.GetCWorkIntoPlan(@planId)";
            var parameters = new Dictionary<string, string>
            {
                {"@planId",planId }
            };
            DB db = DB.GetInstance();
            var work = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
            if (work != null)
            {
                return new CourseWork
                {
                    ID = work.ContainsKey("ID") ? work["ID"].ToString() : string.Empty,
                    Name = work.ContainsKey("name") ? work["name"].ToString() : string.Empty
                };
            }
            else
                return new CourseWork();
        }

        /// <summary>
        /// Сохраняет текущий объект CourseWork в БД
        /// </summary>
        /// <param name="authorId">автор</param>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push(string authorId)
        {
            var parameters = new Dictionary<string, string>();
            string procName = "dbo.AddCourseWork";
            parameters.Add("@name", Name);
            parameters.Add("@advanced", Advanced);
            parameters.Add("@description", Description);
            parameters.Add("@authorId", authorId);
            DB db = DB.GetInstance();
            return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
        }

        /// <summary>
        /// Сохраняет указанное имя и URL файла как присоединенный файл
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AttachFile()
        {
            string procName = "dbo.UpdateCourseWork";
            var parameters = new Dictionary<string, string>();
            DB db = DB.GetInstance();
            parameters.Add("@ID", ID);
            parameters.Add("@fileName", FileName);
            parameters.Add("@fileURL", FileURL);
            return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
        }

        /// <summary>
        /// Обновляет в БД выбранный объект (по ID)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Update()
        {
            string procName = "dbo.UpdateCourseWork";
            var parameters = new Dictionary<string, string>();
            DB db = DB.GetInstance();
            parameters.Add("@ID", ID);
            parameters.Add("@name", Name);
            parameters.Add("@advanced", Advanced);
            return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
        }

        /// <summary>
        /// Удаление текущего объекта из БД
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            string sqlQuery = "delete from CourseWorks where ID=@ID";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID }
            };
            DB db = DB.GetInstance();
            int result = db.ExecInsOrDelQuery(sqlQuery, parameters);
            if (result == 1)
            {
                ID = null;
                return true;
            }
            else
                return false;
        }
    }

    public class CourseWorkStage
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public string SubjectGroupSemesterId { get; set; }

        /// <summary>
        /// Возвращает этап (процентовку) курсовой работу по указанному ID
        /// </summary>
        /// <param name="id">id этапа курсовой работы</param>
        /// <returns></returns>
        public static async Task<CourseWorkStage> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from CourseWorkStages where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            DB db = DB.GetInstance();
            var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);

            if (result.Count > 0)
                return new CourseWorkStage
                {
                    ID = result.ContainsKey("ID") ? result["ID"].ToString() : string.Empty,
                    Name = result.ContainsKey("name") ? result["name"].ToString() : string.Empty,
                    Info = result.ContainsKey("info") ? result["info"].ToString() : string.Empty,
                    SubjectGroupSemesterId = result.ContainsKey("SubjectGroupSemesterID") ? result["SubjectGroupSemesterID"] : string.Empty
                };
            else
                return null;
        }

        /// <summary>
        /// Возвращает полный список процентовок курсовых работ
        /// </summary>
        /// <returns></returns>
        public static async Task<List<CourseWorkStage>> GetCollectionAsync()
        {
            string sqlQuery = "select * from CourseWorkStages";
            DB db = DB.GetInstance();
            var result = await db.ExecSelectQueryAsync(sqlQuery);
            var labWorks = new List<CourseWorkStage>(result.Count);
            foreach (var obj in result)
            {
                labWorks.Add(new CourseWorkStage
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : string.Empty,
                    Name = obj.ContainsKey("name") ? obj["name"].ToString() : string.Empty,
                    Info = obj.ContainsKey("info") ? obj["info"].ToString() : string.Empty,
                    SubjectGroupSemesterId = obj.ContainsKey("SubjectGroupSemesterID") ? obj["SubjectGroupSemesterID"] : string.Empty
                });
            }

            return labWorks;
        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="stages"></param>
        /// <returns></returns>
        public static List<CourseWorkStage> ToCourseWorkStages(List<Dictionary<string, dynamic>> stages)
        {
            var labWorks = new List<CourseWorkStage>(stages.Count);
            foreach (var obj in stages)
            {
                labWorks.Add(new CourseWorkStage
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : string.Empty,
                    Name = obj.ContainsKey("name") ? obj["name"].ToString() : string.Empty,
                    Info = obj.ContainsKey("info") ? obj["info"].ToString() : string.Empty,
                    SubjectGroupSemesterId = obj.ContainsKey("SubjectGroupSemesterID") ? obj["SubjectGroupSemesterID"] : string.Empty
                });
            }

            return labWorks;
        }

        /// <summary>
        /// Возвращает процентовку курсовой работы по id предмета
        /// </summary>
        /// <param name="subjectId">id предмета (предмет-группа-семестр)</param>
        /// <returns></returns>
        public static async Task<List<CourseWorkStage>> GetCWorkStagesFromSubject(string subjectId)
        {
            var stages = await GetCollectionAsync();
            return stages.FindAll(x => x.SubjectGroupSemesterId.Equals(subjectId));
        }

        /// <summary>
        /// Сохраняет текущий объект CourseWorkStage в БД
        /// </summary>
        /// <param name="subjectId">автор</param>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            var parameters = new Dictionary<string, string>();
            string procName = "dbo.AddCourseWorkStage";
            parameters.Add("@name", Name);
            parameters.Add("@info", Info);
            parameters.Add("@subjectId", SubjectGroupSemesterId);
            DB db = DB.GetInstance();
            return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
        }

        /// <summary>
        /// Обновляет в БД выбранный объект (по ID)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Update()
        {
            string procName = "dbo.UpdateCourseWorkStage";
            var parameters = new Dictionary<string, string>();
            DB db = DB.GetInstance();
            parameters.Add("@ID", ID);
            parameters.Add("@name", Name);
            parameters.Add("@info", Info);
            return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
        }

        /// <summary>
        /// Удаление текущего объекта из БД
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            string sqlQuery = "delete from CourseWorkStages where ID=@ID";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID }
            };
            DB db = DB.GetInstance();
            int result = db.ExecInsOrDelQuery(sqlQuery, parameters);
            if (result == 1)
            {
                ID = null;
                return true;
            }
            else
                return false;
        }
    }

    public class CourseWorkExecution
    {
        public string ID { get; set; }
        public string Info { get; set; }
        public DateTime Date { get; set; }
        public bool State { get; set; }
        public CourseWork CWork { get; set; }


        /// <summary>
        /// Возвращает состояние выполнения студентом курсовой работы
        /// </summary>
        /// <param name="studentID">id этапа курсовой работы</param>
        /// <returns></returns>
        public static async Task<CourseWorkExecution> GetInstanceAsync(string studentID)
        {
            string sqlQuery = "select * from CourseWorkExecution where StudentGroupSemesterID=@studentId";
            var parameters = new Dictionary<string, string>
            {
                { "@studentId", studentID }
            };
            DB db = DB.GetInstance();
            var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
            if (result != null)
            {
                var work = new CourseWorkExecution
                {
                    ID = result.ContainsKey("ID") ? result["ID"].ToString() : string.Empty,
                    Info = result.ContainsKey("info") ? result["info"].ToString() : string.Empty,
                    Date = result.ContainsKey("date") ? result["date"] : string.Empty,
                    State = result.ContainsKey("state") ? result["state"].ToString() : string.Empty
                };
                work.CWork = result.ContainsKey("CourseWorkPlanID") ?
                    CourseWork.GetCWorkFromPlan(result["CourseWorkPlanID"].ToString()) : null;
                return work;
            }
            else
                return null;
        }

        /// <summary>
        /// Обновляет в БД выбранный объект (по ID)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Update()
        {
            string procName = "dbo.UpdateCourseWorkExecution";
            var parameters = new Dictionary<string, string>();
            DB db = DB.GetInstance();
            parameters.Add("@ID", ID);
            parameters.Add("@info", Info);
            parameters.Add("@state", Convert.ToInt16(State).ToString());
            return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
        }
    }


    public class CourseWorkStageExecution : CourseWorkStage
    {
        DateTime Date { get; set; }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="stages"></param>
        /// <returns></returns>
        public new static List<CourseWorkStageExecution> ToCourseWorkStages(List<Dictionary<string, dynamic>> stages)
        {
            var labWorks = new List<CourseWorkStageExecution>(stages.Count);
            foreach (var obj in stages)
            {
                labWorks.Add(new CourseWorkStageExecution
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : string.Empty,
                    Name = obj.ContainsKey("name") ? obj["name"].ToString() : string.Empty,
                    Info = obj.ContainsKey("info") ? obj["info"].ToString() : string.Empty,
                    SubjectGroupSemesterId = obj.ContainsKey("SubjectGroupSemesterID") ? obj["SubjectGroupSemesterID"] : string.Empty,
                    Date = obj.ContainsKey("date") ? obj["date"] : default(DateTime)
                });
            }

            return labWorks;
        }
    }
}