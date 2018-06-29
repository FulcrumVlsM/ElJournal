using System.Collections.Generic;
using System.Linq;
using ElJournal.DBInteract;
using System.Threading.Tasks;
using System;

namespace ElJournal.Providers
{

    /// <summary>
    /// Представляет интерфейс для получения данных о пользователе, необходимых системе
    /// </summary>
    public class NativeAuthProvider
    {
        private string _token;
        

        public NativeAuthProvider(string token)
        {
            Token = token;
        }


        /// <summary>
        /// Создает новый объект NativeAuthProvider
        /// </summary>
        /// <param name="token">токен пользователя</param>
        /// <returns>Новый объект NativeAuthProvider или Null при отсутствии пользователя с данным токеном</returns>
        public static async Task<NativeAuthProvider> GetInstance(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;
            else
            {
                try
                {
                    DB db = DB.GetInstance();
                    string sqlQuery = "select dbo.CheckToken(@token)";
                    var parameters = new Dictionary<string, string>();
                    parameters.Add("@token", token);
                    if ((bool)await db.ExecuteScalarQueryAsync(sqlQuery, parameters))
                        return new NativeAuthProvider(token);
                    else
                        return null;
                }
                catch(Exception e)
                {
                    //TODO: add log
                    return null;
                }
            }
        }


        /// <summary>
        /// Список разрешений, который имеется у пользователя
        /// </summary>
        public List<string> Permissions
        {
            get
            {
                string sqlQuery = "select codeName from dbo.GetRights(@personId)";
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                DB db = DB.GetInstance();
                parameters.Add("@personId", PersonId);
                var rights = db.ExecSelectQueryAsync(sqlQuery, parameters).Result;
                var permissions = new List<string>();
                foreach(var right in rights)
                    permissions.Add(right["codeName"]);
                return permissions;
            }
        }

        /// <summary>
        /// Список предметов в потоках, которые ведет преподаватель
        /// </summary>
        public List<string> FlowsSubjects
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select ID from dbo.GetSubjectsFlowOfTeacher(@personId)";
                var parameters = new Dictionary<string, string>();
                parameters.Add("@personId", PersonId);
                var list = db.ExecSelectQueryAsync(sqlQuery, parameters).Result;
                var subjects = new List<string>();
                foreach (var item in list)
                    subjects.Add(item["ID"]);
                return subjects;
            }
        }

        /// <summary>
        /// Список факультетов, к которым относится пользователь
        /// </summary>
        public List<string> Faculties
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select ID from dbo.PersonFaculty(@personId)";
                var parameters = new Dictionary<string, string>();
                parameters.Add("@personId", PersonId);
                var list = db.ExecSelectQueryAsync(sqlQuery, parameters).Result;
                var faculties = new List<string>();
                foreach (var item in list)
                    faculties.Add(item["ID"]);
                return faculties;
            }
        }

        /// <summary>
        /// Список кафедр, к которым относится пользователь
        /// </summary>
        public List<string> Departments
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select ID from dbo.PersonDepartment(@personId)";
                var parameters = new Dictionary<string, string>();
                parameters.Add("@personId", PersonId);
                var list = db.ExecSelectQueryAsync(sqlQuery, parameters).Result;
                var departments = new List<string>();
                foreach (var item in list)
                    departments.Add(item["ID"]);
                return departments;
            }
        }

        /// <summary>
        /// Список лабораторных работ, автором которых является пользователь
        /// </summary>
        public List<string> LabWorks
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select ID from dbo.GetLabWorksOfTheAuthor(@personId)";
                var parameters = new Dictionary<string, string>();
                parameters.Add("@personId", PersonId);
                var list = db.ExecSelectQueryAsync(sqlQuery, parameters).Result;
                var works = new List<string>();
                foreach (var item in list)
                    works.Add(item["ID"]);
                return works;
            }
        }

        /// <summary>
        /// Список практических работ, автором которых является пользователь
        /// </summary>
        public List<string> PractWorks
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select ID from dbo.GetPractWorksOfTheAuthor(@personId)";
                var parameters = new Dictionary<string, string>();
                parameters.Add("@personId", PersonId);
                var list = db.ExecSelectQueryAsync(sqlQuery, parameters).Result;
                var works = new List<string>();
                foreach (var item in list)
                    works.Add(item["ID"]);
                return works;
            }
        }

        /// <summary>
        /// Список курсовых работ, автором которых является пользователь
        /// </summary>
        public List<string> CourseWorks
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select ID from dbo.GetCourseWorksOfTheAuthor(@personId)";
                var parameters = new Dictionary<string, string>();
                parameters.Add("@personId", PersonId);
                var list = db.ExecSelectQueryAsync(sqlQuery, parameters).Result;
                var works = new List<string>();
                foreach (var item in list)
                    works.Add(item["ID"]);
                return works;
            }
        }

        /// <summary>
        /// Список самостоятельных работ, автором которых является пользователь
        /// </summary>
        public List<string> IndependentWorks
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select ID from dbo.GetIndependentWorksOfTheAuthor(@personId)";
                var parameters = new Dictionary<string, string>
                {
                    { "@personId", PersonId }
                };
                var list = db.ExecSelectQueryAsync(sqlQuery, parameters).Result;
                var works = new List<string>();
                foreach (var item in list)
                    works.Add(item["ID"]);
                return works;
            }
        }

        /// <summary>
        /// Токен пользователя
        /// </summary>
        public string Token
        {
            get
            {
                return _token;
            }
            set
            {
                _token = value;
            }
        }

        /// <summary>
        /// ID Пользователя
        /// </summary>
        public string PersonId
        {
            get
            {
                DB db = DB.GetInstance();
                var people = db.People.Find(x => x["token"] == _token);
                //var people = db.People.Where(x => x["token"] == _token);
                return people["ID"];
                //return (people[0])["ID"];
            }
        }

        /// <summary>
        /// Возвращает имеется ли указанное разрешение у пользователя
        /// </summary>
        /// <param name="permission">разрешение</param>
        /// <returns>Наличие разрешения</returns>
        public bool CheckPermission(string permission)
        {
            return Permissions.Contains(permission);
        }
    }
}