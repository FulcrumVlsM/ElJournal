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
    public class IndependentWork
    {
        public string ID { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public string FileName { get; set; }
        [JsonIgnore]
        public string FileURL { get; set; }
        public string Description { get; set; }
        public string Advanced { get; set; }
        public string AuthorId { get; set; }


        /// <summary>
        /// Возвращает самостоятельную работу по указанному ID
        /// </summary>
        /// <param name="id">id самостоятельной работы</param>
        /// <returns></returns>
        public static async Task<IndependentWork> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from IndependentWorks where ID=@id";
            var parameters = new Dictionary<string, string>();
            parameters.Add("@id", id);
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (result != null)
                    return new IndependentWork
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
        /// Возвращает полный список самостоятельных работ
        /// </summary>
        /// <returns></returns>
        public static async Task<List<IndependentWork>> GetCollectionAsync()
        {
            try
            {
                string sqlQuery = "select * from IndependentWorks";
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                return ToIndependentWork(result);
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return new List<IndependentWork>();
            }
        }

        /// <summary>
        /// Возвращает полный список самостоятельных работ
        /// </summary>
        /// <param name="authorId">id автора курсовых работ</param>
        /// <returns></returns>
        public static async Task<List<IndependentWork>> GetCollectionAsync(string authorId)
        {
            string sqlQuery = "select * from IndependentWorks where authorID=@authorId";
            var parameters = new Dictionary<string, string>()
            {
                { "@authorID", authorId }
            };

            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                return ToIndependentWork(result);
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return new List<IndependentWork>();
            }
        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="works"></param>
        /// <returns></returns>
        public static List<IndependentWork> ToIndependentWork(List<Dictionary<string, dynamic>> works)
        {
            var indWorks = new List<IndependentWork>(works.Count);
            foreach (var obj in works)
            {
                indWorks.Add(new IndependentWork
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

            return indWorks;
        }

        /// <summary>
        /// Сохраняет текущий объект independentWork в БД
        /// </summary>
        /// <param name="authorId">автор</param>
        /// <returns>True, если объект был добавлен в БД</returns>
        public virtual async Task<bool> Push(string authorId)
        {
            string procName = "dbo.AddIndependentWork";
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
            catch (Exception e)
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

                string procName = "dbo.UpdateIndependentWork";
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
            string procName = "dbo.UpdateIndependentWork";
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
        public virtual bool Delete()
        {
            string sqlQuery = "delete from IndependentWorks where ID=@ID";
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


    public class IndependentWorkPlan : IndependentWork
    {
        public string IndependentWorkPlanId { get; set; }
        public string FlowSubjectId { get; set; }


        /// <summary>
        /// Возвращает самостоятельную работу из плана по указанному ID
        /// </summary>
        /// <param name="planId">id записи из списка плана</param>
        /// <returns></returns>
        public new static async Task<IndependentWorkPlan> GetInstanceAsync(string planId)
        {
            string sqlQuery = "select * from dbo.GetIndependentWorkWithPlan() where IndependentWorkPlanID=@planId";
            var parameters = new Dictionary<string, string>
            {
                { "@planId", planId }
            };

            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (result != null)
                    return new IndependentWorkPlan
                    {
                        ID = result.ContainsKey("ID") ? result["ID"].ToString() : string.Empty,
                        AuthorId = result.ContainsKey("authorID") ? result["authorID"].ToString() : string.Empty,
                        Name = result.ContainsKey("name") ? result["name"].ToString() : string.Empty,
                        FileName = result.ContainsKey("fileName") ? result["fileName"].ToString() : string.Empty,
                        FileURL = result.ContainsKey("fileURL") ? result["fileURL"].ToString() : string.Empty,
                        Advanced = result.ContainsKey("advanced") ? result["advanced"].ToString() : string.Empty,
                        Description = result.ContainsKey("description") ? result["description"].ToString() : string.Empty,
                        IndependentWorkPlanId = result.ContainsKey("IndependentWorkPlanID") 
                                                ? result["IndependentWorkPlanID"].ToString() : string.Empty
                    };
                else
                    return null;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// получает список самостоятельных из плана по указанному предмету
        /// </summary>
        /// <param name="flowSubjectId">id предмета (предмет-поток)</param>
        /// <returns></returns>
        public new static async Task<List<IndependentWorkPlan>> GetCollectionAsync(string flowSubjectId)
        {
            string sqlQuery = "select ID, name, authorID, IndendentWorkPlanID " +
                "from dbo.GetIndependentWorkPlan(@flowSubjectId)";
            var parameters = new Dictionary<string, string>
            {
                { "@flowSubjectId", flowSubjectId }
            };

            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                return ToIndependentWork(result);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<IndependentWorkPlan>();
            }
        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="works"></param>
        /// <returns></returns>
        public new static List<IndependentWorkPlan> ToIndependentWork(List<Dictionary<string, dynamic>> works)
        {
            var indWorks = new List<IndependentWorkPlan>(works.Count);
            foreach (var obj in works)
            {
                indWorks.Add(new IndependentWorkPlan
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : string.Empty,
                    AuthorId = obj.ContainsKey("authorID") ? obj["authorID"].ToString() : string.Empty,
                    Name = obj.ContainsKey("name") ? obj["name"].ToString() : string.Empty,
                    FileName = obj.ContainsKey("fileName") ? obj["fileName"].ToString() : string.Empty,
                    FileURL = obj.ContainsKey("fileURL") ? obj["fileURL"].ToString() : string.Empty,
                    Advanced = obj.ContainsKey("advanced") ? obj["advanced"].ToString() : string.Empty,
                    Description = obj.ContainsKey("description") ? obj["description"].ToString() : string.Empty,
                    IndependentWorkPlanId = obj.ContainsKey("IndendentWorkPlanID") ? obj["IndendentWorkPlanID"].ToString() : string.Empty
                });
            }

            return indWorks;
        }

        public async Task<bool> Push()
        {
            string sqlQuery = "insert into IndependentWorksPlan(IndependentWorkID,FlowSubjectID) values " +
                "(@workId, @flowSubjectId)";
            var parameters = new Dictionary<string, string>
            {
                { "@workId", ID },
                { "@flowSubjectId",  FlowSubjectId }
            };

            try
            {
                DB db = DB.GetInstance();
                int result = await db.ExecInsOrDelQueryAsync(sqlQuery, parameters);
                if (result == 1)
                    return true;
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

        public override bool Delete()
        {
            string sqlQuery = "delete from IndependentWorksPlan where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", IndependentWorkPlanId }
            };

            try
            {
                DB db = DB.GetInstance();
                int result = db.ExecInsOrDelQuery(sqlQuery, parameters);
                if (result == 1)
                    return true;
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

    public class IndependentWorkExecution
    {
        public string ID { get; set; }
        public string Info { get; set; }
        public string Date { get; set; }
        public bool State { get; set; }
        public string IndependentWorkPlanId { get; set; }
        public string StudentFlowSubjectId { get; set; }

        public static async Task<IndependentWorkExecution> GetInstanceAsync(string studentFlowSubjectId)
        {
            string sqlQuery = "select * from IndependentWorkExecution where StudentFlowSubjectID=@studentFlowSubjectId";
            var parameters = new Dictionary<string, string>
            {
                { "@studentFlowSubjectId", studentFlowSubjectId }
            };

            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (result != null)
                    return new IndependentWorkExecution
                    {
                        ID = result.ContainsKey("ID") ? result["ID"].ToString() : string.Empty,
                        Info = result.ContainsKey("info") ? result["info"].ToString() : string.Empty,
                        Date = result.ContainsKey("date") ? result["date"].ToString() : string.Empty,
                        State = result.ContainsKey("state") ? Convert.ToBoolean(result["state"].ToString()) : false,
                        IndependentWorkPlanId = result.ContainsKey("IndependentWorkPlanID") 
                                                           ? result["IndependentWorkPlanID"].ToString() : string.Empty,
                        StudentFlowSubjectId = result.ContainsKey("StudentFlowSubjectID")
                                                           ? result["StudentFlowSubjectID"].ToString() : string.Empty
                    };
                else
                    return null;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return null;
            }
        }

        public async Task<bool> Push()
        {
            string procName = "dbo.AddIndependentWorkExecution";
            var parameters = new Dictionary<string, string>
            {
                { "@studentFlowSubjectId", StudentFlowSubjectId },
                { "@planId", IndependentWorkPlanId },
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
                logger.Fatal(e.ToString());
                return false;
            }
        }

        public async Task<bool> Update()
        {
            string procName = "dbo.UpdateIndependentWorkExecution";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@info", Info },
                { "@state", State.ToString() }
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

        public bool Delete()
        {
            string sqlQuery = "delete from IndependentWorkExecution where ID=@ID";
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