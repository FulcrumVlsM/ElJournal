using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ElJournal.DBInteract;
using NLog;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;

namespace ElJournal.Models
{
    public class LabWork
    {
        public string ID { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public string FileName { get; set; }
        [JsonIgnore]
        public string FileURL { get; set; }
        public string Advanced { get; set; }
        public string AuthorId { get; set; }


        /// <summary>
        /// Возвращает лабораторную работу по указанному ID
        /// </summary>
        /// <param name="id">id лабораторной работы</param>
        /// <returns></returns>
        public static async Task<LabWork> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from LabWorks where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new LabWork
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
        /// Возвращает лабораторную работу по указанному ID (только основные данные)
        /// </summary>
        /// <param name="id">id лабораторной работы</param>
        /// <returns></returns>
        public static async Task<LabWork> GetLightInstanceAsync(string id)
        {
            string sqlQuery = "select ID,name from LabWorks where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new LabWork
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
        /// Возвращает полный список лабораторных работ
        /// </summary>
        /// <returns></returns>
        public static async Task<List<LabWork>> GetCollectionAsync()
        {
            try
            {
                string sqlQuery = "select ID,name,authorID from LabWorks";
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
        /// Пребразует коллекцию словарей в коллекцию моделей
        /// </summary>
        /// <param name="works"></param>
        /// <returns></returns>
        public static List<LabWork> ToLabWork(List<Dictionary<string, dynamic>> works)
        {
            var labWorks = new List<LabWork>(works.Count);
            foreach (var obj in works)
            {
                labWorks.Add(new LabWork
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                    Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                    FileName = obj.ContainsKey("fileName") ? obj["fileName"].ToString() : null,
                    FileURL = obj.ContainsKey("fileURL") ? obj["fileURL"].ToString() : null,
                    Advanced = obj.ContainsKey("advanced") ? obj["advanced"].ToString() : null,
                    AuthorId = obj.ContainsKey("authorID") ? obj["authorID"].ToString() : null
                });
            }

            return labWorks;
        }

        /// <summary>
        /// Сохраняет текущий объект LabWork в БД
        /// </summary>
        /// <param name="authorId">автор</param>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push(string authorId)
        {
            string procName = "dbo.AddLabWork";
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
        public async Task<bool> AttachFile(byte[] fileArray)
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

                string sqlQuery = "dbo.UpdateLabWork";
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
            catch(SqlException e)
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
        public async Task<bool> DetachFile()
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
            catch(Exception e)
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
            string procName = "dbo.UpdateLabWork";
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
            string sqlQuery = "delete from LabWorks where ID=@ID";
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


    public class LabWorkPlan
    {
        public string ID { get; set; }
        public LabWork labWork { get; set; }
        public string FlowSubjectId { get; set; }

        /// <summary>
        /// Возвращает лабораторную работу в плане по указанному ID
        /// </summary>
        /// <param name="id">id лабораторной работы</param>
        /// <returns></returns>
        public static async Task<LabWorkPlan> GetInstanceAsync(string id)
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
                    return new LabWorkPlan
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        labWork = obj.ContainsKey("LabWorkID") ? await LabWork.GetInstanceAsync(obj["LabWorkID"]) : null,
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
        public static async Task<List<LabWorkPlan>> ToLabWork(List<Dictionary<string, dynamic>> works)
        {
            var labWorks = new List<LabWorkPlan>(works.Count);
            foreach (var obj in works)
            {
                labWorks.Add(new LabWorkPlan
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                    labWork = obj.ContainsKey("LabWorkID") ? await LabWork.GetLightInstanceAsync(obj["LabWorkID"]) : null,
                    FlowSubjectId = obj.ContainsKey("FlowSubjectID") ? obj["FlowSubjectID"].ToString() : null
                });
            }

            return labWorks;
        }

        /// <summary>
        /// Возвращает полный список лабораторных работ в планах по всем предметам
        /// </summary>
        /// <returns></returns>
        public static async Task<List<LabWorkPlan>> GetCollectionAsync()
        {
            try
            {
                string sqlQuery = "select * from LabWorksPlan";
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var labWorks = await ToLabWork(result);
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
        /// Сохраняет текущий объект LabWorkPlan в БД
        /// </summary>
        /// <param name="authorId">автор</param>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddLabWorkPlan";
            var parameters = new Dictionary<string, string>
            {
                { "@labWorkId", labWork.ID },
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
            string sqlQuery = "delete from LabWorksPlan where ID=@ID";
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

    
    public class ExecutedLabWork
    {
        public string ID { get; set; }
        public string PlanId { get; set; }
        public string Info { get; set; }
        public DateTime Date { get; set; }
        public string StudentFlowSubjectId { get; set; }

        public static List<ExecutedLabWork> ToLabWork(List<Dictionary<string, dynamic>> works)
        {
            var labWorks = new List<ExecutedLabWork>(works.Count);
            foreach (var obj in works)
            {
                labWorks.Add(new ExecutedLabWork
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                    PlanId = obj.ContainsKey("LabWorkPlanID") ? obj["LabWorkPlanID"].ToString() : null,
                    Info = obj.ContainsKey("info") ? obj["info"].ToString() : null,
                    StudentFlowSubjectId = obj.ContainsKey("StudentFlowSubjectID") ? obj["StudentFlowSubjectID"].ToString() : null,
                    Date = obj.ContainsKey("date") ? obj["date"] : default(DateTime)
                });
            }

            return labWorks;
        }

        /// <summary>
        /// Возвращает объект выполнения лабораторной работы в плане по указанному ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<ExecutedLabWork> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from LabWorksExecution where ID=@id";
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
                    return new ExecutedLabWork
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
        /// Возвращает полный список лабораторных работ в планах по всем предметам
        /// </summary>
        /// <returns></returns>
        public static async Task<List<ExecutedLabWork>> GetCollectionAsync()
        {
            try
            {
                string sqlQuery = "select * from LabWorksExecution";
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
        /// Сохраняет текущий объект LabWorkExecution в БД
        /// </summary>
        /// <param name="authorId">автор</param>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddLabWorkExecution";
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
        public bool Delete()
        {
            string sqlQuery = "delete from LabWorksExecution where ID=@ID";
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