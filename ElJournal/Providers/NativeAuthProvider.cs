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
        public static NativeAuthProvider GetInstance(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;
            else
            {
                DB db = DB.GetInstance();
                string sqlQuery = "dbo.CheckToken(@token)";
                if ((bool)db.ExecuteScalarQuery(sqlQuery).Result)
                    return new NativeAuthProvider(token);
                else
                    return null;
            }
        }

        public List<string> Permissions
        {
            get
            {
                string sqlQuery = "select codeName from dbo.GetRights(@personId)";
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                DB db = DB.GetInstance();
                parameters.Add("@personId", PersonId);
                var rights = db.ExecSelectQuery(sqlQuery, parameters).Result;
                var permissions = new List<string>();
                foreach(var right in rights)
                    permissions.Add(right["codeName"]);
                return permissions;
            }
        }
        public List<string> Subjects
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select ID from dbo.GetTeacherSubjects(@personId)";
                var parameters = new Dictionary<string, string>();
                parameters.Add("@personId", PersonId);
                var list = db.ExecSelectQuery(sqlQuery, parameters).Result;
                var subjects = new List<string>();
                foreach (var item in list)
                    subjects.Add(item["ID"]);
                return subjects;
            }
        }
        public List<string> Faculties
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select ID from dbo.PersonFaculty(@personId)";
                var parameters = new Dictionary<string, string>();
                parameters.Add("@personId", PersonId);
                var list = db.ExecSelectQuery(sqlQuery, parameters).Result;
                var faculties = new List<string>();
                foreach (var item in list)
                    faculties.Add(item["ID"]);
                return faculties;
            }
        }
        public List<string> Departments
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select ID from dbo.PersonDepartment(@personId)";
                var parameters = new Dictionary<string, string>();
                parameters.Add("@personId", PersonId);
                var list = db.ExecSelectQuery(sqlQuery, parameters).Result;
                var departments = new List<string>();
                foreach (var item in list)
                    departments.Add(item["ID"]);
                return departments;
            }
        }
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
        public string PersonId
        {
            get
            {
                DB db = DB.GetInstance();
                var people = (List <Dictionary<string,dynamic>>)db.People.Where(x => x["token"] = _token);
                return (people[0])["ID"];
            }
        }

        public bool CheckPermission(string permission)
        {
            return Permissions.Contains(permission);
        }
    }
}