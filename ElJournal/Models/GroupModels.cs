using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Group
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CuratorId { get; set; }
        public string FacultyId { get; set; }

        /// <summary>
        /// Возвращает группу по id
        /// </summary>
        /// <param name="id">id группы</param>
        /// <returns></returns>
        public static async Task<Group> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from Groups where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            DB db = DB.GetInstance();
            var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);

            if (result != null)
                return new Group
                {
                    ID = result.ContainsKey("ID") ? result["ID"].ToString() : null,
                    Name = result.ContainsKey("name") ? result["name"].ToString() : null,
                    Description = result.ContainsKey("description") ? result["description"].ToString() : null,
                    CuratorId = result.ContainsKey("curatorPersonID") ? result["curatorPersonID"].ToString() : null,
                    FacultyId = result.ContainsKey("FacultyID") ? result["FacultyID"].ToString() : null
                };
            else
                return null;
        }

        /// <summary>
        /// Возвращает полный список групп
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Group>> GetCollectionAsync()
        {
            string sqlQuery = "select * from Groups";
            DB db = DB.GetInstance();
            var result = await db.ExecSelectQueryAsync(sqlQuery);
            var labWorks = new List<Group>(result.Count);
            foreach (var obj in result)
            {
                labWorks.Add(new Group
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                    Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                    FacultyId = obj.ContainsKey("FacultyID") ? obj["FacultyID"] : null
                });
            }

            return labWorks;
        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="groupList"></param>
        /// <returns></returns>
        public static List<Group> ToGroups(List<Dictionary<string, dynamic>> groupList)
        {
            if (groupList.Count == 0)
                return null;
            else
            {
                var groups = new List<Group>(groupList.Count);
                foreach (var obj in groupList)
                {
                    groups.Add(new Group
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                        Description = obj.ContainsKey("description") ? obj["description"].ToString() : null,
                        CuratorId = obj.ContainsKey("curatorPersonID") ? obj["curatorPersonID"].ToString() : null,
                        FacultyId = obj.ContainsKey("FacultyID") ? obj["FacultyID"].ToString() : null
                    });
                }
                return groups;
            }
        }

        /// <summary>
        /// Добавляет группу на указанный семестр
        /// </summary>
        /// <param name="semesterId">id семестра</param>
        /// <returns></returns>
        public async Task<bool> AddToSemester(string semesterId)
        {
            string procName = "dbo.AddGroupSemester";
            var parameters = new Dictionary<string, string>
            {
                {"@groupId", ID },
                {"@semesterId", semesterId }
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
        /// Удаляет группу из указанного семестра (при наличии студентов в группе удаление невозможно)
        /// </summary>
        /// <param name="semesterId">id семестра</param>
        /// <returns></returns>
        public bool DeleteToSemester(string semesterId)
        {
            string procName = "dbo.DeleteGroupSemester";
            var parameters = new Dictionary<string, string>
            {
                {"@groupId", ID },
                {"@semesterId", semesterId }
            };
            try
            {
                DB db = DB.GetInstance();
                int result = db.ExecStoredProcedure(procName, parameters);
                if (result == 1)
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

        /// <summary>
        /// Сохраняет текущий объект Group в БД
        /// </summary>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddGroup";
            var parameters = new Dictionary<string, string>
            {
                { "@name", Name },
                { "@description", Description },
                { "@FacultyID", FacultyId },
                { "@curatorPersonID", CuratorId }
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
            string procName = "dbo.UpdateGroup";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@name", Name },
                { "@description", Description },
                { "@FacultyID", FacultyId },
                { "@curatorPersonID", CuratorId }
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
            string procName = "dbo.DeleteGroup";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID }
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
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return false;
            }
        }
    }
}