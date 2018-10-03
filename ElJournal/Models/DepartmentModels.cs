using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Department
    {
        public string ID { get; set; }
        public string ManagerId { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }

        /// <summary>
        /// Возвращает кафедру по id
        /// </summary>
        /// <param name="id">id кафедры</param>
        /// <returns></returns>
        public static async Task<Department> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from Departments where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new Department
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                        ManagerId = obj.ContainsKey("managerPersonID") ? obj["managerPersonID"].ToString() : null,
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
        /// <param name="departmentList"></param>
        /// <returns></returns>
        public static List<Department> ToDepartments(List<Dictionary<string, dynamic>> departmentList)
        {
            if (departmentList.Count == 0)
                return new List<Department>();
            else
            {
                var departments = new List<Department>(departmentList.Count);
                foreach (var obj in departmentList)
                {
                    departments.Add(new Department
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                        ManagerId = obj.ContainsKey("managerPersonID") ? obj["managerPersonID"].ToString() : null,
                        Info = obj.ContainsKey("info") ? obj["info"].ToString() : null
                    });
                }
                return departments;
            }
        }

        /// <summary>
        /// Возвращает полный список кафедр
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Department>> GetCollectionAsync()
        {
            string sqlQuery = "select ID,name from Departments";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                return ToDepartments(result);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString()); //запись лога с ошибкой
                return new List<Department>();
            }
        }

        /// <summary>
        /// Сохраняет текущий объект Department в БД
        /// </summary>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddDepartment";
            var parameters = new Dictionary<string, string>
            {
                { "@name", Name },
                { "@info", Info},
                { "@managerId", ManagerId }
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
            string procName = "dbo.UpdateDepartment";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@name", Name },
                { "@info", Info },
                { "@managerPersonID", ManagerId }
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
            string procName = "dbo.DeleteDepartment";
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