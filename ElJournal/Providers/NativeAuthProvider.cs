using System.Collections.Generic;
using System.Linq;
using ElJournal.DBInteract;

namespace ElJournal.Providers
{
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
        public static NativeAuthProvider GetInstance(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;
            else
            {
                DB db = DB.GetInstance();
                string sqlQuery = "dbo.CheckToken(@token)";
                if ((bool)db.ExecuteScalarQueryAsync(sqlQuery).Result)
                    return new NativeAuthProvider(token);
                else
                    return null;
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
        /// Список предметов, которые ведет преподаватель
        /// </summary>
        public List<string> Subjects
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select ID from dbo.GetTeacherSubjects(@personId)";
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
                var people = (List <Dictionary<string,dynamic>>)db.People.Where(x => x["token"] = _token);
                return (people[0])["ID"];
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