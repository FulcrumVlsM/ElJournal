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
    public class PracticeWork : LabWork
    {
        [JsonIgnore]
        public override string FileName { get; set; }
        [JsonIgnore]
        public override string FileURL { get; set; }


        /// <summary>
        /// Возвращает практическую работу по указанному ID
        /// </summary>
        /// <param name="id">id лабораторной работы</param>
        /// <returns></returns>
        public new static async Task<PracticeWork> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from PracticeWorks where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new PracticeWork
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                        FileName = obj.ContainsKey("fileName") ? obj["fileName"].ToString() : null,
                        FileURL = obj.ContainsKey("fileURL") ? obj["fileURL"].ToString() : null,
                        Advanced = obj.ContainsKey("advanced") ? obj["advanced"].ToString() : null,
                        AuthorId = obj.ContainsKey("authorID") ? obj["authorID"] : null
                    };
                else
                    return null;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Возвращает практическую работу по указанному ID (только основные данные)
        /// </summary>
        /// <param name="id">id практической работы</param>
        /// <returns></returns>
        public new static async Task<PracticeWork> GetLightInstanceAsync(string id)
        {
            string sqlQuery = "select ID,name from PracticeWorks where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new PracticeWork
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null
                    };
                else
                    return null;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Пребразует коллекцию словарей в коллекцию моделей
        /// </summary>
        /// <param name="works"></param>
        /// <returns></returns>
        public static List<PracticeWork> ToPractWork(List<Dictionary<string, dynamic>> works)
        {
            var practWorks = new List<PracticeWork>(works.Count);
            foreach (var obj in works)
            {
                practWorks.Add(new PracticeWork
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                    Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                    FileName = obj.ContainsKey("fileName") ? obj["fileName"].ToString() : null,
                    FileURL = obj.ContainsKey("fileURL") ? obj["fileURL"].ToString() : null,
                    Advanced = obj.ContainsKey("advanced") ? obj["advanced"].ToString() : null,
                    AuthorId = obj.ContainsKey("authorID") ? obj["authorID"].ToString() : null
                });
            }

            return practWorks;
        }

        /// <summary>
        /// Возвращает полный список практических работ
        /// </summary>
        /// <returns></returns>
        public new static async Task<List<PracticeWork>> GetCollectionAsync()
        {
            try
            {
                string sqlQuery = "select ID,name,authorID from PracticeWorks";
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var practWorks = ToPractWork(result);
                return practWorks;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Возвращает полный список практических работ
        /// </summary>
        /// <param name="authorId">id автора</param>
        /// <returns></returns>
        public new static async Task<List<LabWork>> GetCollectionAsync(string authorId)
        {
            try
            {
                string sqlQuery = "select ID,name,authorID from dbo.GetPractWorksOfTheAuthor(@authorId)";
                var parameters = new Dictionary<string, string>
                {
                    { "@authorId", authorId }
                };
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                var labWorks = ToLabWork(result);
                return labWorks;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Сохраняет текущий объект PracticeWork в БД
        /// </summary>
        /// <param name="authorId">автор</param>
        /// <returns>True, если объект был добавлен в БД</returns>
        public override async Task<bool> Push(string authorId)
        {
            string procName = "dbo.AddPracticeWork";
            var parameters = new Dictionary<string, string>
            {
                { "@name", Name },
                { "@advanced", Advanced },
                { "@authorId", authorId }
            };
            try
            {
                DB db = DB.GetInstance();
                return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Присоединяет файл к практической работе
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> AttachFile(byte[] fileArray)
        {
            if (string.IsNullOrWhiteSpace(FileURL) || string.IsNullOrWhiteSpace(FileName))
                return false;

            try
            {
                FileURL = ConfigurationManager.AppSettings["FileStorage"];
                using (FileStream fs = new FileStream(FileURL + FileName, FileMode.CreateNew))
                {
                    await fs.WriteAsync(fileArray, 0, fileArray.Length);
                }

                string sqlQuery = "dbo.UpdatePractWork";
                var parameters = new Dictionary<string, string>();
                DB db = DB.GetInstance();
                parameters.Add("@ID", ID);
                parameters.Add("@fileName", FileName);
                parameters.Add("@fileURL", FileURL);
                return Convert.ToBoolean(await db.ExecStoredProcedureAsync(sqlQuery, parameters));
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
        /// Удаляет присоединенный файл
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> DetachFile()
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
        public override async Task<bool> Update()
        {
            string procName = "dbo.UpdatePractWork";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@name", Name },
                { "@advanced", Advanced }
            };
            try
            {
                DB db = DB.GetInstance();
                return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
            }
            catch (Exception e)
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
        public override bool Delete()
        {
            string procName = "dbo.DeletePractWork";
            var parameters = new Dictionary<string, string>
            {
                { "@id", ID }
            };
            try
            {
                DB db = DB.GetInstance();
                int result = db.ExecInsOrDelQuery(procName, parameters);
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
                logger.Fatal(e.ToString());
                return false;
            }
        }
    }

    public class ExecutedPractWork : ExecutedLabWork
    {
        public new static List<ExecutedPractWork> ToLabWork(List<Dictionary<string, dynamic>> works)
        {
            var practWorks = new List<ExecutedPractWork>(works.Count);
            foreach (var obj in works)
            {
                practWorks.Add(new ExecutedPractWork
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                    PlanId = obj.ContainsKey("LabWorkPlanID") ? obj["LabWorkPlanID"].ToString() : null,
                    Info = obj.ContainsKey("info") ? obj["info"].ToString() : null,
                    StudentFlowSubjectId = obj.ContainsKey("StudentFlowSubjectID") ? obj["StudentFlowSubjectID"].ToString() : null,
                    Date = obj.ContainsKey("date") ? obj["date"] : default(DateTime)
                });
            }

            return practWorks;
        }

        /// <summary>
        /// Возвращает объект выполнения практической работы в плане по указанному ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public new static async Task<ExecutedPractWork> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from PracticeWorkExecution where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                {
                    return new ExecutedPractWork
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        PlanId = obj.ContainsKey("LabWorkPlanID") ? obj["LabWorkPlanID"].ToString() : null,
                        Info = obj.ContainsKey("info") ? obj["info"].ToString() : null,
                        StudentFlowSubjectId = obj.ContainsKey("StudentFlowSubjectID") ? obj["StudentFlowSubjectID"].ToString() : null,
                        Date = obj.ContainsKey("date") ? obj["date"] : default(DateTime)
                    };
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Возвращает полный список выполнений практических работ студентами
        /// </summary>
        /// <returns></returns>
        public new static async Task<List<ExecutedPractWork>> GetCollectionAsync()
        {
            try
            {
                string sqlQuery = "select * from PracticeWorkExecution";
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var labWorks = ToLabWork(result);
                return labWorks;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Возвращает полный список выполнений практических работ студентами
        /// </summary>
        /// <param name="studentFlowId">id студента в потоке</param>
        /// <returns></returns>
        public new static async Task<List<ExecutedPractWork>> GetCollectionAsync(string studentFlowId)
        {
            try
            {
                string sqlQuery = "select * from PracticeWorkExecution where StudentFlowSemesterID=@studentId";
                var parameters = new Dictionary<string, string>
                {
                    { "@studentId", studentFlowId }
                };
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var practWorks = ToLabWork(result);
                return practWorks;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Сохраняет текущий объект PracticeWorkExecution в БД
        /// </summary>
        /// <param name="authorId">автор</param>
        /// <returns>True, если объект был добавлен в БД</returns>
        public override async Task<bool> Push()
        {
            string procName = "dbo.AddPractWorkExecution";
            var parameters = new Dictionary<string, string>
            {
                { "@labWorkPlanId", PlanId },
                { "@info", Info },
                { "@studentId", StudentFlowSubjectId }
            };
            try
            {
                DB db = DB.GetInstance();
                return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
            }
            catch (Exception e)
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
        public override bool Delete()
        {
            string sqlQuery = "delete from PracticeWorkExecution where ID=@ID";
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
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return false;
            }
        }
    }

    public class PracticeWorkPlan
    {
        public string ID { get; set; }
        public virtual PracticeWork Work { get; set; }
        public string FlowSubjectId { get; set; }


        /// <summary>
        /// Возвращает лабораторную работу в плане по указанному ID
        /// </summary>
        /// <param name="id">id лабораторной работы</param>
        /// <returns></returns>
        public static async Task<PracticeWorkPlan> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from LabWorksPlan where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                {
                    return new PracticeWorkPlan
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Work = obj.ContainsKey("LabWorkID") ? await LabWork.GetInstanceAsync(obj["LabWorkID"]) : null,
                        FlowSubjectId = obj.ContainsKey("FlowSubjectID") ? obj["FlowSubjectID"].ToString() : null
                    };
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Пребразует коллекцию словарей в коллекцию моделей
        /// </summary>
        /// <param name="works"></param>
        /// <returns></returns>
        public static async Task<List<PracticeWorkPlan>> ToPracticeWork(List<Dictionary<string, dynamic>> works)
        {
            var practWorks = new List<PracticeWorkPlan>(works.Count);
            foreach (var obj in works)
            {
                practWorks.Add(new PracticeWorkPlan
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                    Work = obj.ContainsKey("LabWorkID") ? await LabWork.GetLightInstanceAsync(obj["LabWorkID"]) : null,
                    FlowSubjectId = obj.ContainsKey("FlowSubjectID") ? obj["FlowSubjectID"].ToString() : null
                });
            }

            return practWorks;
        }

        /// <summary>
        /// Возвращает полный список пунктов планов практических работ
        /// </summary>
        /// <returns></returns>
        public static async Task<List<PracticeWorkPlan>> GetCollectionAsync()
        {
            try
            {
                string sqlQuery = "select * from LabWorksPlan";
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var practWorks = await ToPracticeWork(result);
                return practWorks;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return new List<PracticeWorkPlan>();
            }
        }

        /// <summary>
        /// Сохраняет текущий объект PracticeWorkPlan в БД
        /// </summary>
        /// <param name="authorId">автор</param>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddPractWorkPlan";
            var parameters = new Dictionary<string, string>
            {
                { "@practWorkId", Work.ID },
                { "@flowSubjId", FlowSubjectId }
            };
            try
            {
                DB db = DB.GetInstance();
                return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
            }
            catch (Exception e)
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
            string sqlQuery = "delete from PracticeWorksPlan where ID=@ID";
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
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return false;
            }
        }
    }
}