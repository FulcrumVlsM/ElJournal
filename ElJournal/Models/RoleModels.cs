using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Role
    {
        public string ID { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="rightList"></param>
        /// <returns></returns>
        private static List<Role> ToRoles(List<Dictionary<string, dynamic>> rightList)
        {
            if (rightList.Count == 0)
                return new List<Role>();
            else
            {
                var rights = new List<Role>(rightList.Count);
                foreach (var obj in rightList)
                {
                    rights.Add(new Role
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null
                    });
                }
                return rights;
            }
        }

        /// <summary>
        /// Возвращает полный список прав доступа
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Role>> GetCollectionAsync()
        {
            string sqlQuery = "select * from Roles";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                return ToRoles(result);
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