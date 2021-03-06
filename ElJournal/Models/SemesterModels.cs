﻿using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Semester
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? SessionStart { get; set; }
        public DateTime? SessionEnd { get; set; }

        /// <summary>
        /// Возвращает семестр по id
        /// </summary>
        /// <param name="id">id семестра</param>
        /// <returns></returns>
        public static async Task<Semester> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from Semesters where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            DB db = DB.GetInstance();
            var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);

            if (obj != null)
            {
                return new Semester
                {
                    ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                    Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                    StartDate = obj.ContainsKey("StartDate") ? (!(obj["StartDate"] is DBNull) ? obj["StartDate"] : null) : null,
                    EndDate = obj.ContainsKey("EndDate") ? (!(obj["EndDate"] is DBNull) ? obj["EndDate"] : null) : null,
                    SessionStart = obj.ContainsKey("SessionStartDate") ? 
                                                 (!(obj["SessionStartDate"] is DBNull) ? obj["SessionStartDate"] : null) : null,
                    SessionEnd = obj.ContainsKey("SessionEndDate") ? 
                                                      (!(obj["SessionEndDate"] is DBNull) ? obj["SessionEndDate"] : null) : null
                };
            }
            else
                return null;
        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="semesterList"></param>
        /// <returns></returns>
        public static List<Semester> ToSemesters(List<Dictionary<string, dynamic>> semesterList)
        {
            if (semesterList.Count == 0)
                return null;
            else
            {
                var semesters = new List<Semester>(semesterList.Count);
                foreach (var obj in semesterList)
                {
                    semesters.Add(new Semester
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null,
                        StartDate = obj.ContainsKey("StartDate") ? (!(obj["StartDate"] is DBNull) ? obj["StartDate"] : null) : null,
                        EndDate = obj.ContainsKey("EndDate") ? (!(obj["EndDate"] is DBNull) ? obj["EndDate"] : null) : null,
                        SessionStart = obj.ContainsKey("SessionStartDate") ?
                                                 (!(obj["SessionStartDate"] is DBNull) ? obj["SessionStartDate"] : null) : null,
                        SessionEnd = obj.ContainsKey("SessionEndDate") ?
                                                      (!(obj["SessionEndDate"] is DBNull) ? obj["SessionEndDate"] : null) : null
                    });
                }
                return semesters;
            }
        }

        /// <summary>
        /// Возвращает полный список факультетов
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Semester>> GetCollectionAsync()
        {
            string sqlQuery = "select * from Semesters";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var semesters = ToSemesters(result);
                return semesters;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return new List<Semester>();
            }
        }

        public static async Task<List<Semester>> GetCollectionAsync(string groupId)
        {
            string sqlQuery = "select * from dbo.GetSemestersOfGroup(@groupid)";
            var parameters = new Dictionary<string, string>
            {
                { "@groupId", groupId }
            };
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                var semesters = ToSemesters(result);
                return semesters;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return new List<Semester>();
            }
        }

        /// <summary>
        /// Сохраняет текущий объект Semester в БД
        /// </summary>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.AddSemester";
            var parameters = new Dictionary<string, string>
            {
                { "@name", Name },
                { "@start", StartDate?.ToString()},
                { "@end", EndDate?.ToString() },
                { "@sessionStart", SessionStart?.ToString() },
                { "@sessionEnd", SessionEnd?.ToString() }
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
            string procName = "dbo.UpdateSemester";
            var parameters = new Dictionary<string, string>
            {
                { "@ID", ID },
                { "@name", Name },
                { "@start", StartDate?.ToString() },
                { "@end", EndDate?.ToString() },
                { "@sessionStart", SessionStart?.ToString() },
                { "@sessionEnd", SessionEnd?.ToString() }
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
            string procName = "dbo.DeleteSemester";
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
}