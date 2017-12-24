using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace ElJournal.DBInteract
{
    public sealed class DB : IDisposable
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["dbMainConnection"].ConnectionString;
        private static DB db;
        private static object locker = new object();

        private SqlConnection conn;

        private DB()
        {
            conn = new SqlConnection(connectionString);
            //conn.Open();
        }

        public static DB GetInstance()
        {
            if (db == null)
                db = new DB();
            return db;
        }

        public void Dispose()
        {
            conn.Dispose();
        }


        public Task<List<Dictionary<string, dynamic>>> ExecSelectQuery(string sqlQuery)
        {
            if (sqlQuery.Contains("insert") || sqlQuery.Contains("delete"))
                throw new FormatException("Incorrect sql query for this method");

            return Task.Run(async () =>
            {
                await conn.OpenAsync();
                SqlCommand query = new SqlCommand(sqlQuery, conn);
                SqlDataReader result = await query.ExecuteReaderAsync();

                List<Dictionary<string, dynamic>> set = new List<Dictionary<string, dynamic>>();
                while (result.Read())
                {
                    Dictionary<string, dynamic> row = new Dictionary<string, dynamic>(result.FieldCount);
                    for (int i = 0; i < result.FieldCount; i++)
                    {
                        row.Add(result.GetName(i), result.GetFieldValue<dynamic>(i));
                    }
                    set.Add(row);
                }

                conn.Close();
                return set;
            });
        }
        public Task<List<Dictionary<string, dynamic>>> ExecSelectQuery(string sqlQuery, Dictionary<string, string> parameters)
        {
            if (sqlQuery.Contains("insert") || sqlQuery.Contains("delete"))
                throw new FormatException("Incorrect sql query for this method");

            return Task.Run(async () =>
            {
                await conn.OpenAsync();
                SqlCommand query = new SqlCommand(sqlQuery, conn);
                foreach (var obj in parameters)
                    query.Parameters.Add(new SqlParameter(obj.Key, obj.Value));
                SqlDataReader result = await query.ExecuteReaderAsync();

                List<Dictionary<string, dynamic>> set = new List<Dictionary<string, dynamic>>();
                while (result.Read())
                {
                    Dictionary<string, dynamic> row = new Dictionary<string, dynamic>(result.FieldCount);
                    for (int i = 0; i < result.FieldCount; i++)
                    {
                        row.Add(result.GetName(i), result.GetFieldValue<dynamic>(i));
                    }
                    set.Add(row);
                }

                conn.Close();
                return set;
            });
        }
        public Task<dynamic> ExecuteScalarQuery(string sqlQuery)
        {
            if (sqlQuery.Contains("insert") || sqlQuery.Contains("delete"))
                throw new FormatException("Incorrect sql query for this method");

            return Task.Run(async () =>
            {
                await conn.OpenAsync();
                SqlCommand query = new SqlCommand(sqlQuery, conn);
                dynamic result = await query.ExecuteScalarAsync();

                conn.Close();
                return result;
            });
        }
        public Task<dynamic> ExecuteScalarQuery(string sqlQuery, Dictionary<string,string> parameters)
        {
            if (sqlQuery.Contains("insert") || sqlQuery.Contains("delete"))
                throw new FormatException("Incorrect sql query for this method");

            return Task.Run(async () =>
            {
                await conn.OpenAsync();
                SqlCommand query = new SqlCommand(sqlQuery, conn);
                foreach (var obj in parameters)
                    query.Parameters.Add(new SqlParameter(obj.Key, obj.Value));
                dynamic result = default(dynamic);
                try
                {
                    result = await query.ExecuteScalarAsync();
                }
                catch(SqlException e)
                {
                    result = null;
                }

                conn.Close();
                return result;
            });
        }
        public int ExecStoredProcedure(string procedureName, Dictionary<string,string> parameters)
        {
            //TODO: после появления исключения не закрывается подключение к бд. Исправить.
            lock (locker)
            {
                conn.Open();
                SqlCommand query = new SqlCommand(procedureName, conn);
                query.CommandType = System.Data.CommandType.StoredProcedure;

                foreach (var obj in parameters)
                    query.Parameters.Add(new SqlParameter(obj.Key, obj.Value));
                var returnParam = query.Parameters.Add("@ReturnVal", SqlDbType.Int);
                returnParam.Direction = ParameterDirection.ReturnValue;

                query.ExecuteNonQuery();
                int answer = (int)returnParam.Value;
                conn.Close();
                return answer;
            }
        }
        public int ExecInsOrDelQuery(string sqlQuery)
        {
            if(sqlQuery.Contains("select"))
                throw new FormatException("Incorrect sql query for this method");

            lock (locker)
            {
                conn.Open();
                SqlCommand query = new SqlCommand(sqlQuery, conn);
                int result = query.ExecuteNonQuery();
                conn.Close();

                return result;
            }
        }
        public int ExecInsOrDelQuery(string sqlQuery, Dictionary<string,string> parameters)
        {
            if (sqlQuery.Contains("select"))
                throw new FormatException("Incorrect sql query for this method");

            lock (locker)
            {
                conn.Open();
                SqlCommand query = new SqlCommand(sqlQuery, conn);
                foreach (var obj in parameters)
                    query.Parameters.Add(new SqlParameter(obj.Key, obj.Value));
                int result = query.ExecuteNonQuery();
                conn.Close();

                return result;
            }
        }

        public async Task<bool> CheckPermission(string personId,string permission)
        {
            string sqlQuery = "select dbo.CheckRight(@personID, @permission)";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@personID", personId);
            parameters.Add("@permission", permission);
            if (!string.IsNullOrWhiteSpace(personId))
                return await ExecuteScalarQuery(sqlQuery, parameters);
            else return false;
        }
    }
}