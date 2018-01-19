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
            //TODO: add log
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
            //TODO: add log
        }


        public Task<List<Dictionary<string, dynamic>>> ExecSelectQuery(string sqlQuery)
        {
            if (sqlQuery.Contains("insert") || sqlQuery.Contains("delete"))
                throw new FormatException("Incorrect sql query for this method");

            return Task.Run(async () =>
            {
                var conn = new SqlConnection(connectionString);
                var set = new List<Dictionary<string, dynamic>>(); //результат
                await conn.OpenAsync();
                try
                {
                    SqlCommand query = new SqlCommand(sqlQuery, conn);
                    SqlDataReader result = await query.ExecuteReaderAsync();

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
                }
                catch(SqlException e)
                {
                    //TODO: add log
                    conn.Close();
                    throw e;
                }

                return set;
            });
        }
        public Task<List<Dictionary<string, dynamic>>> ExecSelectQuery(string sqlQuery, Dictionary<string, string> parameters)
        {
            if (sqlQuery.Contains("insert") || sqlQuery.Contains("delete"))
                throw new FormatException("Incorrect sql query for this method");

            return Task.Run(async () =>
            {
                var conn = new SqlConnection(connectionString);
                var set = new List<Dictionary<string, dynamic>>(); //результат
                await conn.OpenAsync();
                try
                {
                    SqlCommand query = new SqlCommand(sqlQuery, conn);
                    foreach (var obj in parameters)//добавление параметров в запрос
                        query.Parameters.Add(new SqlParameter(obj.Key, obj.Value));
                    SqlDataReader result = await query.ExecuteReaderAsync();

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
                }
                catch(SqlException e)
                {
                    //TODO: add log
                    conn.Close();
                    throw e;
                }

                return set;
            });
        }
        public Task<dynamic> ExecuteScalarQuery(string sqlQuery)
        {
            if (sqlQuery.Contains("insert") || sqlQuery.Contains("delete"))//запрет на инструкции добавления и удаления
                throw new FormatException("Incorrect sql query for this method");

            return Task.Run(async () =>
            {
                var conn = new SqlConnection(connectionString);//создание подключения
                await conn.OpenAsync();

                dynamic result = default(dynamic);
                try
                {
                    SqlCommand query = new SqlCommand(sqlQuery, conn);//формирование запроса
                    result = await query.ExecuteScalarAsync();//выполнение запроса
                    conn.Close();//закрытие подключения
                }
                catch(SqlException e)
                {
                    //TODO: add log;
                    conn.Close();
                    throw e;
                }

                return result;
            });
        }
        public Task<dynamic> ExecuteScalarQuery(string sqlQuery, Dictionary<string,string> parameters)
        {
            if (sqlQuery.Contains("insert") || sqlQuery.Contains("delete"))//запрет на операции добавления и удаления
                throw new FormatException("Incorrect sql query for this method");

            return Task.Run(async () =>
            {
                var conn = new SqlConnection(connectionString);//создание подключения
                await conn.OpenAsync();

                SqlCommand query = new SqlCommand(sqlQuery, conn); //формирование запроса

                foreach (var obj in parameters)//добавление параметров к запросу
                    query.Parameters.Add(new SqlParameter(obj.Key, obj.Value));

                dynamic result = default(dynamic);
                try
                {
                    result = await query.ExecuteScalarAsync();//выполнение запроса
                }
                catch(SqlException e)
                {
                    //TODO: add log
                    conn.Close();
                    throw e;
                }

                conn.Close();//закрытие подклчения
                return result;
            });
        }
        public int ExecStoredProcedure(string procedureName, Dictionary<string,string> parameters)
        {
            lock (locker)
            {
                conn.Open();
                int answer = default(int);

                try
                {
                    SqlCommand query = new SqlCommand(procedureName, conn);//формирование запроса
                    query.CommandType = System.Data.CommandType.StoredProcedure;

                    foreach (var obj in parameters)//добавление параметров к запросу
                        query.Parameters.Add(new SqlParameter(obj.Key, obj.Value));

                    var returnParam = query.Parameters.Add("@ReturnVal", SqlDbType.Int);//выходноу значение
                    returnParam.Direction = ParameterDirection.ReturnValue;

                    query.ExecuteNonQuery();//выполнение запроса
                    answer = (int)returnParam.Value;//получение выходного значения
                }
                catch(SqlException e)
                {
                    //TODO: add log
                    conn.Close();
                    throw e;
                }

                conn.Close();
                return answer;
            }
        }
        public int ExecInsOrDelQuery(string sqlQuery)
        {
            if(sqlQuery.Contains("select"))//запрет на выборку (select)
                throw new FormatException("Incorrect sql query for this method");

            lock (locker)
            {
                conn.Open();
                int result = default(int);
                try
                {
                    SqlCommand query = new SqlCommand(sqlQuery, conn);
                    result = query.ExecuteNonQuery();
                }
                catch(SqlException e)
                {
                    //TODO: add log
                    conn.Close();
                    throw e;
                }

                conn.Close();
                return result;
            }
        }
        public int ExecInsOrDelQuery(string sqlQuery, Dictionary<string,string> parameters)
        {
            if (sqlQuery.Contains("select"))//запрет на выборку (select)
                throw new FormatException("Incorrect sql query for this method");

            lock (locker)
            {
                conn.Open();
                int result = default(int);
                try
                {
                    SqlCommand query = new SqlCommand(sqlQuery, conn);//формирование запроса

                    foreach (var obj in parameters)//добавление добавление параметров к запросу
                        query.Parameters.Add(new SqlParameter(obj.Key, obj.Value));

                    result = query.ExecuteNonQuery();//выполнение запроса
                }
                catch(SqlException e)
                {
                    //TODO: add log
                    conn.Close();
                    throw e;
                }

                conn.Close();
                return result;
            }
        }

        public async Task<bool> CheckPermission(string personId,string permission)
        {
            string sqlQuery = "select dbo.CheckRight(@token, @permission)";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@token", personId);
            parameters.Add("@permission", permission);
            if (!string.IsNullOrWhiteSpace(personId))
                return await ExecuteScalarQuery(sqlQuery, parameters);
            else return false;
        }
    }

    public static class Permission
    {
        public const string PERSON_COMMON_PERMISSION = "PERSON_COMMON_PERMISSION";

        public const string FACULTY_PERMISSION = "FACULTY_PERMISSION";
        public const string FACULTY_COMMON_PERMISSION = "FACULTY_COMMON_PERMISSION";

        public const string STUDENT_COMMON_PERMISSION = "STUDENT_COMMON_PERMISSION";
        public const string STUDENT_PERMISSION = "STUDENT_PERMISSION";


    }
}