using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Person
    {
        public string ID { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string Name { get; set; }
        public string Student_id { get; set; }
        public string Passport_id { get; set; }
        public string Secret { get; set; }
        public string Info { get; set; }
        public string RoleId { get; set; }
        public List<Department> Departments { get; set; }
        public List<Faculty> Facuties { get; set; }

        private string _token;


        /// <summary>
        /// Возвращает пользователя по id
        /// </summary>
        /// <param name="id">id кафедры</param>
        /// <returns></returns>
        public static async Task<Person> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from People where ID=@id";
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
                    Person person = new Person
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Surname = obj.ContainsKey("surname") ? obj["surname"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                        Patronymic = obj.ContainsKey("patronymic") ? obj["patronymic"].ToString() : null,
                        Student_id = obj.ContainsKey("student_id") ? obj["student_id"].ToString() : null,
                        Passport_id = obj.ContainsKey("passport_id") ? obj["passport_id"].ToString() : null,
                        Secret = obj.ContainsKey("secret_word") ? obj["secret_word"].ToString() : null,
                        Info = obj.ContainsKey("info") ? obj["info"].ToString() : null,
                        RoleId = obj.ContainsKey("RolesID") ? obj["RolesID"].ToString() : null,
                        _token = obj.ContainsKey("token") ? obj["token"].ToString() : null

                    };
                    person.Facuties = await GetFacultiesOfPerson(person.ID);
                    person.Departments = await GetDepartmentsOfPerson(person.ID);
                    return person;
                }
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
        /// Возвращает пользователя по id (без конф. данных)
        /// </summary>
        /// <param name="id">id кафедры</param>
        /// <returns></returns>
        public static async Task<Person> GetPublicInstanceAsync(string id)
        {
            string sqlQuery = "select ID,name,surname,patronymic,RolesID,info from People where ID=@id";
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
                    Person person = new Person
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Surname = obj.ContainsKey("surname") ? obj["surname"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                        Patronymic = obj.ContainsKey("patronymic") ? obj["patronymic"].ToString() : null,
                        Info = obj.ContainsKey("info") ? obj["info"].ToString() : null,
                        RoleId = obj.ContainsKey("RolesID") ? obj["RolesID"].ToString() : null

                    };
                    person.Facuties = await GetFacultiesOfPerson(person.ID);
                    person.Departments = await GetDepartmentsOfPerson(person.ID);
                    return person;
                }
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

        private static async Task<List<Faculty>> GetFacultiesOfPerson(string personId)
        {
            string sqlQuery = "select * from dbo.PersonFaculty(@personId)";
            var parameters = new Dictionary<string, string>
            {
                { "@personId", personId }
            };
            try
            {
                DB db = DB.GetInstance();
                var list = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                return Faculty.ToFaculties(list);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString()); //запись лога с ошибкой
                return null;
            }
        }

        private static async Task<List<Department>> GetDepartmentsOfPerson(string personId)
        {
            string sqlQuery = "select * from dbo.PersonDepartment(@personId)";
            var parameters = new Dictionary<string, string>
            {
                { "@personId", personId }
            };
            try
            {
                DB db = DB.GetInstance();
                var list = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                return Department.ToDepartments(list);
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString()); //запись лога с ошибкой
                return null;
            }
        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="personList"></param>
        /// <returns></returns>
        public static List<Person> ToPeople(List<Dictionary<string, dynamic>> personList)
        {
            if (personList.Count == 0)
                return new List<Person>(0);
            else
            {
                var departments = new List<Person>(personList.Count);
                foreach (var obj in personList)
                {
                    departments.Add(new Person
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Surname = obj.ContainsKey("surname") ? obj["surname"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                        Patronymic = obj.ContainsKey("patronymic") ? obj["patronymic"].ToString() : null,
                        Student_id = obj.ContainsKey("student_id") ? obj["student_id"].ToString() : null,
                        Passport_id = obj.ContainsKey("passport_id") ? obj["passport_id"].ToString() : null,
                        Secret = obj.ContainsKey("secret_word") ? obj["secret_word"].ToString() : null,
                        Info = obj.ContainsKey("info") ? obj["info"].ToString() : null
                    });
                }
                return departments;
            }
        }

        /// <summary>
        /// Возвращает полный список пользователей без конфиденциальной информации
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Person>> GetCollectionAsync()
        {
            string sqlQuery = "select ID,surname,name,patronymic from People";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                return ToPeople(result);
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
            string procName = "dbo.AddPerson";
            var parameters = new Dictionary<string, string>
            {
                { "@surname", Surname },
                { "@name", Name },
                { "@patronymic", Patronymic },
                { "@info", Info},
                { "@RoleID", RoleId },
                { "@student_id", Student_id },
                { "@passport_id", Passport_id },
                { "@secret", Secret }
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
            string procName = "dbo.UpdatePerson";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@surname", Surname },
                { "@name", Name },
                { "@patronymic", Patronymic },
                { "@info", Info},
                { "@RoleID", RoleId },
                { "@student_id", Student_id },
                { "@passport_id", Passport_id },
                { "@secret", Secret }
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
            string procName = "dbo.DeletePerson";
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

        /// <summary>
        /// Добавление пользователя к указанному факультету
        /// </summary>
        /// <param name="facultyId"></param>
        /// <returns></returns>
        public async Task<bool> AddOnFaculty(string facultyId)
        {
            //поиск факультета
            Faculty faculty = await Faculty.GetInstanceAsync(facultyId);
            if (faculty == null)
                return false;

            string procName = "dbo.AddPersonFaculty";
            var parameters = new Dictionary<string, string>
            {
                { "@facultyId", facultyId },
                { "@personId", ID }
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
        /// Удаление пользователя из указанного факультета
        /// </summary>
        /// <param name="facultyId"></param>
        /// <returns></returns>
        public async Task<bool> RemoveOnFaculty(string facultyId)
        {
            //поиск факультета
            Faculty faculty = await Faculty.GetInstanceAsync(facultyId);
            if (faculty == null)
                return false;

            string sqlQuery = "delete from FacultiesPersons where FacultyID=@facultyId and PersonID=@personId";
            var parameters = new Dictionary<string, string>
            {
                { "@facultyId", facultyId },
                { "@personId", ID }
            };
            try
            {
                DB db = DB.GetInstance();
                return Convert.ToBoolean(db.ExecInsOrDelQuery(sqlQuery, parameters));
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return false;
            }
        }

        /// <summary>
        /// Добавление пользователя к указанной кафедре
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        public async Task<bool> AddOnDepartment(string departmentId)
        {
            //поиск факультета
            Department department = await Department.GetInstanceAsync(departmentId);
            if (department == null)
                return false;

            string procName = "dbo.AddPersonDepartment";
            var parameters = new Dictionary<string, string>
            {
                { "@departmentId", departmentId },
                { "@personId", ID }
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
        /// Удаление пользователя из указанной кафедры
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        public async Task<bool> RemoveOnDepartment(string departmentId)
        {
            //поиск факультета
            Department department = await Department.GetInstanceAsync(departmentId);
            if (department == null)
                return false;

            string sqlQuery = "delete from DepartmentsPerson where DepartmentID=@departmentId and PersonID=@personId";
            var parameters = new Dictionary<string, string>
            {
                { "@departmentId", departmentId },
                { "@personId", ID }
            };
            try
            {
                DB db = DB.GetInstance();
                return Convert.ToBoolean(db.ExecInsOrDelQuery(sqlQuery, parameters));
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return false;
            }
        }

        /// <summary>
        /// обновляет токен указанного пользователя
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpdateToken()
        {
            string procName = "dbo.TokenUpdate";
            var parameters = new Dictionary<string, string>
            {
                { "@personId", ID }
            };
            try
            {
                DB db = DB.GetInstance();
                await db.ExecStoredProcedureAsync(procName, parameters);
                return true;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return false;
            }
        }

        /// <summary>
        /// Возвращает токен пользователя
        /// </summary>
        /// <returns></returns>
        public string GetToken()
        {
            return _token;
        }
    }
}