using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Alert
    {
        public string ID { get; set; }
        public string FlowSubjectId { get; set; }
        public string FacultyId { get; set; }
        public string DepartmentId { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? EventDate { get; set; }
        public string EventTypeId { get; set; }
        public string Title { get; set; }
        public string Info { get; set; }
        public string AuthorID { get; set; }
        public bool? Opened { get; set; }


        /// <summary>
        /// Возвращает уведомление по id
        /// </summary>
        /// <param name="id">id уведомления</param>
        /// <returns></returns>
        public static async Task<Alert> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from Events where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new Alert
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        FlowSubjectId = obj.ContainsKey("FlowSubjectID") ? obj["FlowSubjectID"].ToString() : null,
                        FacultyId = obj.ContainsKey("FacultyID") ? obj["FacultyID"].ToString() : null,
                        DepartmentId = obj.ContainsKey("DepartmentID") ? obj["DepartmentID"].ToString() : null,
                        CreateDate = obj.ContainsKey("createDate") ?
                                                 (!(obj["createDate"] is DBNull) ? obj["createDate"] : null) : null,
                        EventDate = obj.ContainsKey("eventDate") ?
                                                 (!(obj["eventDate"] is DBNull) ? obj["eventDate"] : null) : null,
                        EventTypeId = obj.ContainsKey("EventTypeID") ? obj["EventTypeID"].ToString() : null,
                        Title = obj.ContainsKey("title") ? obj["title"].ToString() : null,
                        Info = obj.ContainsKey("info") ? obj["info"].ToString() : null,
                        AuthorID = obj.ContainsKey("authorPersonID") ? obj["authorPersonID"].ToString() : null,
                        Opened = obj.ContainsKey("public") ? Convert.ToBoolean(obj["public"].ToString()) : null
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
        /// <param name="alertList"></param>
        /// <returns></returns>
        public static List<Alert> ToAlerts(List<Dictionary<string, dynamic>> alertList)
        {
            if (alertList.Count == 0)
                return new List<Alert>(0);
            else
            {
                var faculties = new List<Alert>(alertList.Count);
                foreach (var obj in alertList)
                {
                    faculties.Add(new Alert
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        FlowSubjectId = obj.ContainsKey("FlowSubjectID") ? obj["FlowSubjectID"].ToString() : null,
                        FacultyId = obj.ContainsKey("FacultyID") ? obj["FacultyID"].ToString() : null,
                        DepartmentId = obj.ContainsKey("DepartmentID") ? obj["DepartmentID"].ToString() : null,
                        CreateDate = obj.ContainsKey("createDate") ?
                                                 (!(obj["createDate"] is DBNull) ? obj["createDate"] : null) : null,
                        EventDate = obj.ContainsKey("eventDate") ?
                                                 (!(obj["eventDate"] is DBNull) ? obj["eventDate"] : null) : null,
                        EventTypeId = obj.ContainsKey("EventTypeID") ? obj["EventTypeID"].ToString() : null,
                        Title = obj.ContainsKey("title") ? obj["title"].ToString() : null,
                        Info = obj.ContainsKey("info") ? obj["info"].ToString() : null,
                        AuthorID = obj.ContainsKey("authorPersonID") ? obj["authorPersonID"].ToString() : null,
                        Opened = obj.ContainsKey("public") ? Convert.ToBoolean(obj["public"].ToString()) : null
                    });
                }
                return faculties;
            }
        }

        /// <summary>
        /// Возвращает полный список уведомлений
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Alert>> GetCollectionAsync(DateTime start, DateTime end)
        {
            string sqlQuery = "select ID,EventTypeID,FacultyID," +
                "[DepartmentID],[FlowSubjectID],[authorPersonID],[createDate],[eventDate],[title],[public] from dbo.GetEvents(@start,@end)";
            var parameters = new Dictionary<string, string>
            {
                { "@start", start.ToShortDateString() },
                { "@end", end.ToShortDateString() }
            };
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                var alerts = ToAlerts(result);
                return alerts;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<Alert>(0);
            }
        }

        /// <summary>
        /// Сохраняет текущий объект Alert в БД
        /// </summary>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddEvent";
            var parameters = new Dictionary<string, string>
            {
                { "@eventTypeId", EventTypeId },
                { "@facultyId", FacultyId },
                { "@departmentId", DepartmentId },
                { "@flowSubjectId", FlowSubjectId },
                { "@authorId", AuthorID },
                { "@eventDate", EventDate.ToString() },
                { "@title", Title },
                { "@info", Info },
                { "@public", (Convert.ToInt32(Opened)).ToString() }
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
            string procName = "dbo.UpdateEvent";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@eventDate", EventDate.ToString() },
                { "@info", Info },
                { "@title", Title }
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
            string sqlQuery = "delete from Events where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", ID }
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
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return false;
            }
        }
    }

    public class AlertType
    {
        public string ID { get; set; }
        public string Name { get; set; }


        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="alertList"></param>
        /// <returns></returns>
        private static List<AlertType> ToTypes(List<Dictionary<string, dynamic>> alertList)
        {
            if (alertList.Count == 0)
                return new List<AlertType>(0);
            else
            {
                var types = new List<AlertType>(alertList.Count);
                foreach (var obj in alertList)
                {
                    types.Add(new AlertType
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null
                    });
                }
                return types;
            }
        }

        /// <summary>
        /// Возвращает полный список типов уведомлений
        /// </summary>
        /// <returns></returns>
        public static async Task<List<AlertType>> GetCollectionAsync()
        {
            string sqlQuery = "select * from EventsTypes";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var alerts = ToTypes(result);
                return alerts;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<AlertType>();
            }
        }
    }
}