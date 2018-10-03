using ElJournal.DBInteract;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
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
        [JsonIgnore]
        public string FileURL { get; set; }
        [JsonIgnore]
        public string FileName { get; set; }
        public string AuthorId { get; set; }


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
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (result != null)
                    return new CourseWork
                    {
                        ID = result.ContainsKey("ID") ? result["ID"].ToString() : string.Empty,
                        AuthorId = result.ContainsKey("authorID") ? result["authorID"].ToString() : string.Empty,
                        Name = result.ContainsKey("name") ? result["name"].ToString() : string.Empty,
                        FileName = result.ContainsKey("fileName") ? result["fileName"].ToString() : string.Empty,
                        FileURL = result.ContainsKey("fileURL") ? result["fileURL"].ToString() : string.Empty,
                        Advanced = result.ContainsKey("advanced") ? result["advanced"].ToString() : string.Empty,
                        Description = result.ContainsKey("description") ? result["description"].ToString() : string.Empty
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
        /// Возвращает полный список курсовых работ
        /// </summary>
        /// <returns></returns>
        public static async Task<List<CourseWork>> GetCollectionAsync()
        {
            try
            {
                string sqlQuery = "select * from CourseWorks";
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                return ToCourseWork(result);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return new List<CourseWork>();
            }
        }

        /// <summary>
        /// Возвращает полный список курсовых работ
        /// </summary>
        /// <param name="authorId">id автора курсовых работ</param>
        /// <returns></returns>
        public static async Task<List<CourseWork>> GetCollectionAsync(string authorId)
        {
            string sqlQuery = "select * from CourseWorks where authorID=@authorId";
            var parameters = new Dictionary<string, string>()
            {
                { "@authorID", authorId }
            };

            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                return ToCourseWork(result);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return new List<CourseWork>();
            }
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
                    AuthorId = obj.ContainsKey("authorID") ? obj["authorID"].ToString() : string.Empty,
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
            string procName = "dbo.AddCourseWork";
            var parameters = new Dictionary<string, string>
            {
                { "@name", Name },
                { "@advanced", Advanced },
                { "@description", Description },
                { "@authorId", authorId }
            };
            try
            {
                DB db = DB.GetInstance();
                return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Сохраняет указанное имя и URL файла как присоединенный файл
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AttachFile(byte[] fileArray, string fileName)
        {
            try
            {
                FileURL = ConfigurationManager.AppSettings["FileStorage"];
                FileName = fileName;
                using (FileStream fs = new FileStream(FileURL + FileName, FileMode.CreateNew))
                {
                    await fs.WriteAsync(fileArray, 0, fileArray.Length);
                }

                string procName = "dbo.UpdateCourseWork";
                var parameters = new Dictionary<string, string>();
                DB db = DB.GetInstance();
                parameters.Add("@ID", ID);
                parameters.Add("@fileName", FileName);
                parameters.Add("@fileURL", FileURL);
                return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
            }
            catch (IOException)// если файл с таким именем есть на сервере, возникнет конфликт
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Warn(string.Format("Конфликт имени файла при добавлении на сервер. {0}", FileName)); //запись лога с ошибкой
                return false;
            }
            catch (SqlException e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// удаляет запись о присоединенном файле
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> DetachFile()
        {
            if (string.IsNullOrWhiteSpace(FileURL) || string.IsNullOrWhiteSpace(FileName))
                return false;

            string path = FileURL;
            string name = FileName;
            FileURL = string.Empty;
            FileName = string.Empty;

            try
            {
                File.Delete(path + name);
                if (await Update())
                    return true;
                else
                    throw new FileNotFoundException("Не удалось обновить в БД, что файл был удален", name);
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Warn(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Обновляет в БД выбранный объект (по ID)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Update()
        {
            string procName = "dbo.UpdateCourseWork";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@name", Name },
                { "@advanced", Advanced },
                { "@description", Description }
            };
            try
            {
                DB db = DB.GetInstance();
                return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return false;
            }
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
            try
            {
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
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return false;
            }
        }
    }

    public class CourseWorkStage
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public string FlowSubjectId { get; set; }

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
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);

                if (result.Count > 0)
                    return new CourseWorkStage
                    {
                        ID = result.ContainsKey("ID") ? result["ID"].ToString() : string.Empty,
                        Name = result.ContainsKey("name") ? result["name"].ToString() : string.Empty,
                        Info = result.ContainsKey("info") ? result["info"].ToString() : string.Empty,
                        FlowSubjectId = result.ContainsKey("FlowSubjectID") ? result["FlowSubjectID"] : string.Empty
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
        /// Возвращает полный список процентовок курсовых работ
        /// </summary>
        /// <returns></returns>
        public static async Task<List<CourseWorkStage>> GetCollectionAsync()
        {
            string sqlQuery = "select * from CourseWorkStages";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var labWorks = ToCourseWorkStages(result);
                return labWorks;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<CourseWorkStage>();
            }
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
                    FlowSubjectId = obj.ContainsKey("FlowSubjectID") ? obj["FlowSubjectID"] : string.Empty
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
            return stages.FindAll(x => x.FlowSubjectId == subjectId);
        }

        /// <summary>
        /// Получает список процентовок, выполненных студентом
        /// </summary>
        /// <param name="executionId">id записи о выполнении курсовой работы</param>
        /// <returns></returns>
        public static async Task<List<CourseWorkStage>> GetExecuted(string executionId)
        {
            string sqlQuery = "select ID,FlowSubjectID,name from ExecutedCourseWorkStages(@executionId)";
            var parameters = new Dictionary<string, string>
            {
                { "@executionid", executionId }
            };
            try
            {
                DB db = DB.GetInstance();
                var exec = await db.ExecSelectQueryAsync(sqlQuery, parameters);//получение данных из БД
                var result = ToCourseWorkStages(exec);
                return result;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<CourseWorkStage>();
            }
        }

        /// <summary>
        /// Сохраняет текущий объект CourseWorkStage в БД
        /// </summary>
        /// <param name="subjectId">автор</param>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddCourseWorkStage";
            var parameters = new Dictionary<string, string>
            {
                { "@name", Name },
                { "@info", Info },
                { "@subjectId", FlowSubjectId }
            };
            try
            {
                DB db = DB.GetInstance();
                return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
            }
            catch(Exception e)
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
            string procName = "dbo.UpdateCourseWorkStage";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@name", Name },
                { "@info", Info }
            };
            try
            {
                DB db = DB.GetInstance();
                return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
            }
            catch(Exception e)
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
            string sqlQuery = "delete from CourseWorkStages where ID=@ID";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID }
            };
            try
            {
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
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return false;
            }
        }
    }

    public class CourseWorkExecution
    {
        public string ID { get; set; }
        public string Info { get; set; }
        public string Date { get; set; }
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
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (result != null)
                {
                    var work = new CourseWorkExecution
                    {
                        ID = result.ContainsKey("ID") ? result["ID"].ToString() : string.Empty,
                        Info = result.ContainsKey("info") ? result["info"].ToString() : string.Empty,
                        Date = result.ContainsKey("date") ? result["date"] : string.Empty,
                        State = result.ContainsKey("state") ? Convert.ToBoolean(result["state"].ToString()) : false
                    };
                    work.CWork = result.ContainsKey("CourseWorkPlanID") ?
                        CourseWork.GetCWorkFromPlan(result["CourseWorkPlanID"].ToString()) : null;
                    return work;
                }
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
        /// Обновляет в БД выбранный объект (по ID)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Update()
        {
            string procName = "dbo.UpdateCourseWorkExecution";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@info", Info },
                { "@state", Convert.ToInt16(State).ToString() }
            };
            try
            {
                DB db = DB.GetInstance();
                return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return false;
            }
        }
    }


    public class CourseWorkStageExecution
    {
        public string ID { get; set; }
        public DateTime Date { get; set; }
        public string CourseWorkStageId { get; set; }
        public string CourseWorkExecutionId { get; set; }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="stages"></param>
        /// <returns></returns>
        public static List<CourseWorkStageExecution> ToCourseWorkStages(List<Dictionary<string, dynamic>> stages)
        {
            var labWorks = new List<CourseWorkStageExecution>(stages.Count);
            foreach (var obj in stages)
            {
                labWorks.Add(new CourseWorkStageExecution
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : string.Empty,
                    CourseWorkStageId = obj.ContainsKey("CWStageID") ? obj["CWStageID"].ToString() : string.Empty,
                    CourseWorkExecutionId = obj.ContainsKey("CWExecutionID") ? obj["CWExecutionID"] : string.Empty,
                    Date = obj.ContainsKey("date") ? obj["date"] : default(DateTime)
                });
            }

            return labWorks;
        }

        /// <summary>
        /// Производит добавление отметки о выполнени этапа процентовки в БД
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddCWorkStageExecution";
            var parameters = new Dictionary<string, string>
            {
                { "@CWExecutionId", CourseWorkExecutionId },
                { "@stageId", CourseWorkStageId }
            };

            try
            {
                DB db = DB.GetInstance();
                return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return false;
            }
        }

        /// <summary>
        /// Производит удаление отметки о выполнении этапа процентовки из БД
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            string sqlQuery = "delete from CourseWorkStagesExecution where CWStageID=@stageId and CWExecutionID=@executionId";
            var parameters = new Dictionary<string, string>
            {
                { "@stageId", CourseWorkStageId },
                { "@executionId", CourseWorkExecutionId }
            };

            try
            {
                DB db = DB.GetInstance();
                int count = db.ExecInsOrDelQuery(sqlQuery, parameters);
                if (count == 1)
                    return true;
                else
                    return false;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return false;
            }
        }
    }
}