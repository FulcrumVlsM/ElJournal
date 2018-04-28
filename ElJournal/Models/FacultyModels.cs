using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Faculty
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string DekanId { get; set; }
        public string Info { get; set; }

        /// <summary>
        /// Возвращает факультет по id
        /// </summary>
        /// <param name="id">id факультета</param>
        /// <returns></returns>
        public static async Task<Faculty> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from Faculties where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new Faculty
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                        DekanId = obj.ContainsKey("dekanPersonID") ? obj["dekanPersonID"].ToString() : null,
                        Info = obj.ContainsKey("info") ? obj["info"].ToString() : null
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
        /// <param name="facultyList"></param>
        /// <returns></returns>
        public static List<Faculty> ToFaculties(List<Dictionary<string, dynamic>> facultyList)
        {
            if (facultyList.Count == 0)
                return null;
            else
            {
                var faculties = new List<Faculty>(facultyList.Count);
                foreach (var obj in facultyList)
                {
                    faculties.Add(new Faculty
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                        DekanId = obj.ContainsKey("dekanPersonID") ? obj["dekanPersonID"].ToString() : null,
                        Info = obj.ContainsKey("info") ? obj["info"].ToString() : null
                    });
                }
                return faculties;
            }
        }

        /// <summary>
        /// Возвращает полный список факультетов
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Faculty>> GetCollectionAsync()
        {
            string sqlQuery = "select ID,name from Faculties";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var faculties = ToFaculties(result);
                return faculties;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<Faculty>();
            }
        }

        /// <summary>
        /// Сохраняет текущий объект Faculty в БД
        /// </summary>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddFaculty";
            var parameters = new Dictionary<string, string>
            {
                { "@name", Name },
                { "@info", Info},
                { "@dekanID", DekanId }
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
            string procName = "dbo.UpdateFaculty";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@name", Name },
                { "@info", Info },
                { "@dekanId", DekanId }
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
            string procName = "dbo.DeleteFaculty";
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