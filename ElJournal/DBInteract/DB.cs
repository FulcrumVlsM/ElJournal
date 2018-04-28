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


        /// <summary>
        /// Выполняет асинхронный запрос на выборку к БД, выполняя указанный sql запрос
        /// </summary>
        /// <param name="sqlQuery">sql запрос</param>
        /// <returns>Коллекция из Dictionary, ключи соответствуют полям таблицы из БД</returns>
        public Task<List<Dictionary<string, dynamic>>> ExecSelectQueryAsync(string sqlQuery)
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


        /// <summary>
        /// Выполняет асинхронный запрос на выборку к БД, выполняя указанный sql запрос
        /// </summary>
        /// <param name="sqlQuery">sql запрос</param>
        /// <param name="parameters">дополнительные параметры, представленные в запросе как @value</param>
        /// <returns>Коллекция из Dictionary, ключи соответствуют полям таблицы из БД</returns>
        public Task<List<Dictionary<string, dynamic>>> ExecSelectQueryAsync(string sqlQuery, Dictionary<string, string> parameters)
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


        /// <summary>
        /// Выполняет асинхронный запрос к БД на выборку одной строки, выпролняя указанный sql запрос
        /// </summary>
        /// <param name="sqlQuery">sql запрос</param>
        /// <param name="parameters">дополнительные параметры, представленные в запросе как @value</param>
        /// <returns>Коллекция ключ-значение, содержащая данные первой строки sql запроса</returns>
        public async Task<Dictionary<string,dynamic>> ExecSelectQuerySingleAsync(string sqlQuery, Dictionary<string, string> parameters)
        {
            if (sqlQuery.Contains("insert") || sqlQuery.Contains("delete"))
                throw new FormatException("Incorrect sql query for this method");

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
                if (set.Count == 0)
                    return null;
                else
                    return set[0];
            }
            catch (SqlException e)
            {
                //TODO: add log
                conn.Close();
                throw e;
            }
        }


        /// <summary>
        /// Выполняет асинхронный запрос на получение скалярного значения из БД.
        /// </summary>
        /// <param name="sqlQuery">sql запрос</param>
        /// <returns>скалярное значение произвольного типа</returns>
        public Task<dynamic> ExecuteScalarQueryAsync(string sqlQuery)
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


        /// <summary>
        /// Выполняет асинхронный запрос на получение скалярного значения из БД.
        /// </summary>
        /// <param name="sqlQuery">sql запрос</param>
        /// <param name="parameters">дополнительные параметры, представленные в запросе как @value</param>
        /// <returns>скалярное значение произвольного типа</returns>
        public Task<dynamic> ExecuteScalarQueryAsync(string sqlQuery, Dictionary<string,string> parameters)
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
                    if (result is DBNull)
                        result = null;
                    conn.Close();//закрытие подклчения
                    return result;
                }
                catch(SqlException e)
                {
                    //TODO: add log
                    conn.Close();
                    throw e;
                }
            });
        }


        /// <summary>
        /// Выполняет запрос на выполнение хранимой процедуры
        /// </summary>
        /// <param name="procedureName">имя хранимой процедуры</param>
        /// <param name="parameters">параметры хранимой процедуры</param>
        /// <returns>Значение, возвращаемое хранимой процедурой</returns>
        public dynamic ExecStoredProcedure(string procedureName, Dictionary<string,string> parameters)
        {
            lock (locker)
            {
                conn.Open();
                dynamic answer = default(dynamic);

                try
                {
                    SqlCommand query = new SqlCommand(procedureName, conn);//формирование запроса
                    query.CommandType = System.Data.CommandType.StoredProcedure;

                    foreach (var obj in parameters)//добавление параметров к запросу
                        query.Parameters.Add(new SqlParameter(obj.Key, obj.Value));

                    var returnParam = query.Parameters.Add("@ReturnVal", SqlDbType.Int);//выходноу значение
                    returnParam.Direction = ParameterDirection.ReturnValue;

                    query.ExecuteNonQuery();//выполнение запроса
                    answer = returnParam.Value;//получение выходного значения
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


        /// <summary>
        /// Выполняет запрос на выполнение хранимой процедуры в асинхронном режиме.
        /// Не рекомендуется для хранимых процедур с наличием Delete запросов.
        /// </summary>
        /// <param name="procedureName">имя хранимых процедуры</param>
        /// <param name="parameters">параметры хранимой процедуры</param>
        /// <returns>Значение, возвращаемое хранимой процедурой</returns>
        public async Task<dynamic> ExecStoredProcedureAsync(string procedureName, Dictionary<string,string> parameters)
        {
            conn.Open();
            dynamic answer = default(dynamic);

            try
            {
                SqlCommand query = new SqlCommand(procedureName, conn);//формирование запроса
                query.CommandType = System.Data.CommandType.StoredProcedure;

                foreach (var obj in parameters)//добавление параметров к запросу
                    query.Parameters.Add(new SqlParameter(obj.Key, obj.Value));

                var returnParam = query.Parameters.Add("@ReturnVal", SqlDbType.Int);//выходноу значение
                returnParam.Direction = ParameterDirection.ReturnValue;

                await query.ExecuteNonQueryAsync();//выполнение запроса
                answer = returnParam.Value;//получение выходного значения
            }
            catch (SqlException e)
            {
                //TODO: add log
                conn.Close();
                throw e;
            }

            conn.Close();
            return answer;
        }


        /// <summary>
        /// Выполняет запросы Insert, Update, Delete к базе данных
        /// </summary>
        /// <param name="sqlQuery">sql запрос</param>
        /// <returns>результат, возвращаемый хранимой процедурой</returns>
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


        /// <summary>
        /// Выполняет запросы Insert, Update, Delete к базе данных
        /// </summary>
        /// <param name="sqlQuery">sql запрос</param>
        /// <param name="parameters">дополнительные параметры, представленные в запросе как @value</param>
        /// <returns>количество строк задействованных в выполнении команды</returns>
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


        /// <summary>
        /// Асинхронно выполняет запросы Insert, Update, Delete к базе данных.
        /// Не рекомендуется для Delete запросов.
        /// </summary>
        /// <param name="sqlQuery">sql запрос</param>
        /// <returns>количество строк задействованных в выполнении команды</returns>
        public async Task<int> ExecInsOrDelQueryAsync(string sqlQuery)
        {
            if (sqlQuery.Contains("select"))//запрет на выборку (select)
                throw new FormatException("Incorrect sql query for this method");

            conn.Open();
            int result = default(int);
            try
            {
                SqlCommand query = new SqlCommand(sqlQuery, conn);
                result = await query.ExecuteNonQueryAsync();
            }
            catch (SqlException e)
            {
                //TODO: add log
                conn.Close();
                throw e;
            }

            conn.Close();
            return result;
        }


        /// <summary>
        /// Асинхронно выполняет запросы Insert, Update, Delete к базе данных.
        /// Не рекомендуется для Delete запросов.
        /// </summary>
        /// <param name="sqlQuery">sql запрос</param>
        /// <param name="parameters">дополнительные параметры, представленные в запросе как @value</param>
        /// <returns></returns>
        public async Task<int> ExecInsOrDelQueryAsync(string sqlQuery, Dictionary<string, string> parameters)
        {
            if (sqlQuery.Contains("select"))//запрет на выборку (select)
                throw new FormatException("Incorrect sql query for this method");

            conn.Open();
            int result = default(int);
            try
            {
                SqlCommand query = new SqlCommand(sqlQuery, conn);//формирование запроса

                foreach (var obj in parameters)//добавление добавление параметров к запросу
                    query.Parameters.Add(new SqlParameter(obj.Key, obj.Value));

                result = await query.ExecuteNonQueryAsync();//выполнение запроса
            }
            catch (SqlException e)
            {
                //TODO: add log
                conn.Close();
                throw e;
            }

            conn.Close();
            return result;
        }



        /// <summary>
        /// Проводит проверку наличия указанных прав пользователя
        /// </summary>
        /// <param name="personId">ID пользователя</param>
        /// <param name="permission">Запрашиваемое разрешение</param>
        /// <returns></returns>
        public async Task<bool> CheckPermission(string personId,string permission)
        {
            string sqlQuery = "select dbo.CheckRight(@token, @permission)";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@token", personId);
            parameters.Add("@permission", permission);
            if (!string.IsNullOrWhiteSpace(personId))
                return await ExecuteScalarQueryAsync(sqlQuery, parameters);
            else return false;
        }



        /// <summary>
        /// Список кафедр университета
        /// </summary>
        public List<Dictionary<string,dynamic>> Departments
        {
            get
            {
                return ExecSelectQueryAsync("select * from Departments").Result;
            }
        }

        /// <summary>
        /// Список факультетов университета
        /// </summary>
        public List<Dictionary<string,dynamic>> Faculties
        {
            get
            {
                return ExecSelectQueryAsync("select * from Faculties").Result;
            }
        }

        /// <summary>
        /// Список групп
        /// </summary>
        public List<Dictionary<string,dynamic>> Groups
        {
            get
            {
                return ExecSelectQueryAsync("select * from Groups").Result;
            }
        }

        /// <summary>
        /// Список пользователей
        /// </summary>
        public List<Dictionary<string, dynamic>> People
        {
            get
            {
                return ExecSelectQueryAsync("select * from People").Result;
            }
        }
    }

    public static class Permission
    {
        public const string PERSON_COMMON_PERMISSION = "PERSON_COMMON_PERMISSION";

        public const string FACULTY_PERMISSION = "FACULTY_PERMISSION";
        public const string FACULTY_COMMON_PERMISSION = "FACULTY_COMMON_PERMISSION";

        public const string DEPARTMENT_COMMON_PERMISSION = "DEPARTMENT_COMMON_PERMISSION";
        public const string DEPARTMENT_PERMISSION = "DEPARTMENT_PERMISSION";

        public const string EVENT_PERMISSION = "EVENT_PERMISSION";
        public const string EVENT_COMMON_PERMISSION = "EVENT_COMMON_PERMISSION";

        public const string STUDENT_COMMON_PERMISSION = "STUDENT_COMMON_PERMISSION";
        public const string STUDENT_PERMISSION = "STUDENT_PERMISSION";

        public const string LBWRK_COMMON_PERMISSION = "LBWRK_COMMON_PERMISSION";
        public const string LBWRK_PERMISSION = "LBWRK_PERMISSION";
        public const string LBWRK_READ_PERMISSION = "LBWRK_READ_PERMISSION";

        public const string CRSWRK_PERMISSION = "CRSWRK_PERMISSION";
        public const string CRSWRK_COMMON_PERMISSION = "CRSWRK_COMMON_PERMISSION";

        public const string GROUP_PERMISSION = "GROUP_PERMISSION";
        public const string GROUP_COMMON_PERMISSION = "GROUP_COMMON_PERMISSION";

        public const string ACCOUNT_PERMISSION = "ACCOUNT_PERMISSION";
    }
}