using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Account
    {
        public string ID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Alias { get; set; }
        public string PersonID { get; set; }
        public Person User { get; set; }

        /// <summary>
        /// Возвращает аккаунт по ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<Account> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from Accounts where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new Account
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        PersonID = obj.ContainsKey("PersonID") ? obj["PersonID"].ToString() : null,
                        User = obj.ContainsKey("PersonID") ? await Person.GetInstanceAsync(obj["PersonID"]) : null,
                        Login = obj.ContainsKey("login") ? obj["login"].ToString() : null,
                        Password = obj.ContainsKey("password") ? obj["password"].ToString() : null,
                        Email = obj.ContainsKey("email") ? obj["email"].ToString() : null,
                        Alias = obj.ContainsKey("alias") ? obj["alias"].ToString() : null
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
        /// Возвращает аккаунт по логину и паролю
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<Account> GetInstanceAsync(string login, string password)
        {
            string sqlQuery = "select * from Accounts where login=@login nd password=@password";
            var parameters = new Dictionary<string, string>
            {
                { "@login", login },
                { "@password", password }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new Account
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        PersonID = obj.ContainsKey("PersonID") ? obj["PersonID"].ToString() : null,
                        User = obj.ContainsKey("PersonID") ? await Person.GetInstanceAsync(obj["PersonID"]) : null,
                        Login = obj.ContainsKey("login") ? obj["login"].ToString() : null,
                        Password = obj.ContainsKey("password") ? obj["password"].ToString() : null,
                        Email = obj.ContainsKey("email") ? obj["email"].ToString() : null,
                        Alias = obj.ContainsKey("alias") ? obj["alias"].ToString() : null
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
        /// <param name="accountList"></param>
        /// <returns></returns>
        public static async Task<List<Account>> ToAccounts(List<Dictionary<string, dynamic>> accountList)
        {
            if (accountList.Count == 0)
                return new List<Account>(0);
            else
            {
                var accounts = new List<Account>(accountList.Count);
                foreach (var obj in accountList)
                {
                    accounts.Add(new Account
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        PersonID = obj.ContainsKey("PersonID") ? obj["PersonID"].ToString() : null,
                        User = obj.ContainsKey("PersonID") ? await Person.GetPublicInstanceAsync(obj["PersonID"]) : null,
                        Login = obj.ContainsKey("login") ? obj["login"].ToString() : null,
                        Password = obj.ContainsKey("password") ? obj["password"].ToString() : null,
                        Email = obj.ContainsKey("email") ? obj["email"].ToString() : null,
                        Alias = obj.ContainsKey("alias") ? obj["alias"].ToString() : null
                    });
                }
                return accounts;
            }
        }

        /// <summary>
        /// Возвращает полный список аккаунтов (только общая информация)
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Account>> GetCollectionAsync()
        {
            string sqlQuery = "select ID,PersonID,alias from Accounts";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                return await ToAccounts(result);
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString()); //запись лога с ошибкой
                return null;
            }
        }

        /// <summary>
        /// Сохраняет текущий объект Person в БД
        /// </summary>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddAccount";
            var parameters = new Dictionary<string, string>
            {
                { "@personId",  PersonID},
                { "@login", Login },
                { "@password", Password },
                { "@email", Email },
                { "@alias", Alias }
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
            string procName = "dbo.UpdateAccount";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@password", Password },
                { "@email", Email },
                { "@alias", Alias }
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
            string procName = "dbo.DeleteAccount";
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

    public class LogIn
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public async Task<Account> Authorize()
        {
            string sqlQuery = "select dbo.CheckAuth(@login,@password)";
            var parameters = new Dictionary<string, string>
            {
                { "@login", Login },
                { "@password", Password }
            };
            try
            {
                DB db = DB.GetInstance();
                string id = await db.ExecuteScalarQueryAsync(sqlQuery, parameters);
                return await Account.GetInstanceAsync(id);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return null;
            }
        }
    }

    public class NewAccount : Account
    {
        public string Secret { get; set; }
        public string NewPassword { get; set; }
    }
}