using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Flow
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string AltName { get; set; }
        public string DepartmentId { get; set; }

        /// <summary>
        /// Возвращает поток по id
        /// </summary>
        /// <param name="id">id предмета</param>
        /// <returns></returns>
        public static async Task<Flow> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from Flows where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (result != null)
                    return new Flow
                    {
                        ID = result.ContainsKey("ID") ? result["ID"].ToString() : null,
                        Name = result.ContainsKey("name") ? result["name"].ToString() : null,
                        DepartmentId = result.ContainsKey("DepartmentID") ? result["DepartmentID"].ToString() : null,
                        AltName = result.ContainsKey("altName") ? result["altName"].ToString() : null
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
        /// Возвращает полный список потоков
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Flow>> GetCollectionAsync()
        {
            string sqlQuery = "select * from Flows";
            DB db = DB.GetInstance();
            var result = await db.ExecSelectQueryAsync(sqlQuery);
            var flows = ToFlows(result);
            return flows;
        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="flowList"></param>
        /// <returns></returns>
        public static List<Flow> ToFlows(List<Dictionary<string, dynamic>> flowList)
        {
            if (flowList.Count == 0)
                return null;
            else
            {
                var flows = new List<Flow>(flowList.Count);
                foreach (var obj in flowList)
                {
                    flows.Add(new Flow
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                        DepartmentId = obj.ContainsKey("DepartmentID") ? obj["DepartmentID"].ToString() : null,
                        AltName = obj.ContainsKey("altName") ? obj["altName"].ToString() : null
                    });
                }
                return flows;
            }
        }

        /// <summary>
        /// Сохраняет текущий объект Flow в БД
        /// </summary>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddFlow";
            var parameters = new Dictionary<string, string>
            {
                { "@name", Name },
                { "@altName", AltName},
                { "@departmentId", DepartmentId }
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
            string procName = "dbo.UpdateFlow";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@name", Name },
                { "@altName", AltName }
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
            string procName = "dbo.DeleteFlow";
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