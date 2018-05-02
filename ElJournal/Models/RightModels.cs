using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace ElJournal.Models
{
    public class Right
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="rightList"></param>
        /// <returns></returns>
        private static List<Right> ToRights(List<Dictionary<string, dynamic>> rightList)
        {
            if (rightList.Count == 0)
                return new List<Right>();
            else
            {
                var rights = new List<Right>(rightList.Count);
                foreach (var obj in rightList)
                {
                    rights.Add(new Right
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Description = obj.ContainsKey("descriptionName") ? obj["descriptionName"].ToString() : null,
                        Code = obj.ContainsKey("codeName") ? obj["codeName"].ToString() : null
                    });
                }
                return rights;
            }
        }

        /// <summary>
        /// Возвращает полный список прав доступа
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Right>> GetCollectionAsync()
        {
            string sqlQuery = "select * from AccessRights";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                return ToRights(result);
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString()); //запись лога с ошибкой
                return null;
            }
        }

        /// <summary>
        /// Возвращает полный список прав доступа для определенной роли
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Right>> GetCollectionAsync(string roleId)
        {
            string sqlQuery = "select * from dbo.GetRightsRoles(@roleId)";
            var parameters = new Dictionary<string, string>
            {
                { "@roleId", roleId }
            };
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                return ToRights(result);
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString()); //запись лога с ошибкой
                return null;
            }
        }
    }
}