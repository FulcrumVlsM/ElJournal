using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace ElJournal.DBInteract
{
    public sealed class DB : IDisposable
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["dbMain"].ConnectionString;
        private static DB db;
        private static object locker;

        private SqlConnection conn;

        private DB()
        {
            conn = new SqlConnection(connectionString);
            conn.Open();
        }

        public static DB getInstance()
        {
            if (db == null)
                db = new DB();
            return db;
        }

        public void Dispose()
        {
            conn.Close();
            conn.Dispose();
        }


        public List<Dictionary<string,dynamic>> ExecSelectQuery(string sqlQuery)
        {
            if (sqlQuery.Contains("insert") || sqlQuery.Contains("delete"))
                throw new FormatException("Incorrect sql query for this method");

            SqlCommand query = new SqlCommand(sqlQuery, conn);
            SqlDataReader result = query.ExecuteReader();

            List<Dictionary<string, dynamic>> set = new List<Dictionary<string, dynamic>>();
            while (result.Read())
            {
                Dictionary<string, dynamic> row = new Dictionary<string, dynamic>(result.FieldCount);
                for(int i=0; i<result.FieldCount; i++)
                {
                    row.Add(result.GetName(i), result.GetFieldValue<dynamic>(i));
                }
                set.Add(row);
            }

            return set;
        }
        public dynamic ExecuteScalarQuery(string sqlQuery)
        {
            if (sqlQuery.Contains("insert") || sqlQuery.Contains("delete"))
                throw new FormatException("Incorrect sql query for this method");

            SqlCommand query = new SqlCommand(sqlQuery, conn);
            dynamic result = query.ExecuteScalar();

            return result;
        }
        public int ExecStoredProcedure(string procedureName, Dictionary<string,string> parameters)
        {
            SqlCommand query = new SqlCommand(procedureName, conn);
            query.CommandType = System.Data.CommandType.StoredProcedure;

            foreach(var obj in parameters)
                query.Parameters.Add(new SqlParameter(obj.Key, obj.Value));
            var returnParam = query.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;

            query.ExecuteNonQuery();
            int answer = (int)returnParam.Value;
            return answer;
        }
        public int ExecInsOrDelQuery(string sqlQuery)
        {
            if(sqlQuery.Contains("select"))
                throw new FormatException("Incorrect sql query for this method");

            lock (locker)
            {
                SqlCommand query = new SqlCommand(sqlQuery, conn);
                int result = query.ExecuteNonQuery();

                return result;
            }
        }
    }
}