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
        public List<Dictionary<string,dynamic>> Subjects
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select * from dbo.GetTeacherSubjects(@personId)";
                var parameters = new Dictionary<string, string>();
                parameters.Add("@personId", PersonId);
                return db.ExecSelectQuery(sqlQuery, parameters).Result;
            }
        }
        public List<Dictionary<string,dynamic>> Faculties
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select * from dbo.PersonFaculty(@personId)";
                var parameters = new Dictionary<string, string>();
                parameters.Add("@personId", PersonId);
                return db.ExecSelectQuery(sqlQuery, parameters).Result;
            }
        }
        public List<Dictionary<string, dynamic>> Departments
        {
            get
            {
                DB db = DB.GetInstance();
                string sqlQuery = " select * from dbo.PersonDepartment(@personId)";
                var parameters = new Dictionary<string, string>();
                parameters.Add("@personId", PersonId);
                return db.ExecSelectQuery(sqlQuery, parameters).Result;
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
    }
}