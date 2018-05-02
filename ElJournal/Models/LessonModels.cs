using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Lesson
    {
        public string ID { get; set; }
        public string FlowSubjectId { get; set; }
        public string TimeId { get; set; }
        public string TypeId { get; set; }
        DateTime? Date { get; set; }


        /// <summary>
        /// Возвращает занятие по id
        /// </summary>
        /// <param name="id">id занятия</param>
        /// <returns></returns>
        public static async Task<Lesson> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from Lessons where ID=@id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id }
            };
            try
            {
                DB db = DB.GetInstance();
                var obj = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
                if (obj != null)
                    return new Lesson
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        FlowSubjectId = obj.ContainsKey("FlowSubjectID") ? obj["FlowSubjectID"].ToString() : null,
                        TimeId = obj.ContainsKey("LessonTime") ? obj["LessonTime"].ToString() : null,
                        TypeId = obj.ContainsKey("LessonTypeID") ? obj["LessonTypeID"].ToString() : null,
                        Date = obj.ContainsKey("Date") ?  DateTime.Parse(obj["Date"].ToString()) : null
                    };
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
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="lessonList"></param>
        /// <returns></returns>
        public static List<Lesson> ToLessons(List<Dictionary<string, dynamic>> lessonList)
        {
            if (lessonList.Count == 0)
                return new List<Lesson>();
            else
            {
                var lessons = new List<Lesson>(lessonList.Count);
                foreach (var obj in lessonList)
                {
                    lessons.Add(new Lesson
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        FlowSubjectId = obj.ContainsKey("FlowSubjectID") ? obj["FlowSubjectID"].ToString() : null,
                        TimeId = obj.ContainsKey("LessonTime") ? obj["LessonTime"].ToString() : null,
                        TypeId = obj.ContainsKey("LessonTypeID") ? obj["LessonTypeID"].ToString() : null,
                        Date = obj.ContainsKey("Date") ? DateTime.Parse(obj["Date"].ToString()) : null
                    });
                }
                return lessons;
            }
        }

        /// <summary>
        /// Возвращает полный список занятий по предмету
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Lesson>> GetCollectionAsync(string flowSubjectId)
        {
            string sqlQuery = "select * from Lessons where FlowSubjectID=@subjId";
            var parameters = new Dictionary<string, string>
            {
                { "@subjId", flowSubjectId }
            };
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                var lessons = ToLessons(result);
                return lessons;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<Lesson>();
            }
        }

        /// <summary>
        /// Возвращает полный список занятий, на которых присутствовал студент
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Lesson>> GetCollectionAsync(string flowSubjectId, string studentId)
        {
            string sqlQuery = "select * from dbo.GetLessonsOfStudent(@studentId,@subjectId)";
            var parameters = new Dictionary<string, string>
            {
                { "@subjectId", flowSubjectId },
                { "@studentId", studentId }
            };
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                var lessons = ToLessons(result);
                return lessons;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<Lesson>();
            }
        }

        /// <summary>
        /// Сохраняет текущий объект Lesson в БД
        /// </summary>
        /// <returns>True, если объект был добавлен в БД</returns>
        public virtual async Task<bool> Push()
        {
            string procName = "dbo.AddLesson";
            var parameters = new Dictionary<string, string>
            {
                { "@subjectId", FlowSubjectId },
                { "@typeId", TypeId }
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
        public virtual bool Delete()
        {
            string procName = "dbo.DeleteLesson";
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

    public class LessonTime
    {
        public string ID { get; set; }
        public int WeekDay { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Name { get; set; }


        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="lessonList"></param>
        /// <returns></returns>
        public static List<LessonTime> ToLessons(List<Dictionary<string, dynamic>> lessonList)
        {
            if (lessonList.Count == 0)
                return new List<LessonTime>();
            else
            {
                var times = new List<LessonTime>(lessonList.Count);
                foreach (var obj in lessonList)
                {
                    times.Add(new LessonTime
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        WeekDay = obj.ContainsKey("DayOfWeek") ? Convert.ToInt32(obj["DayOfWeek"].ToString()) : null,
                        StartTime = obj.ContainsKey("StartTime") ? obj["StartTime"].ToString() : null,
                        EndTime = obj.ContainsKey("EndTime") ? obj["EndTime"].ToString() : null,
                        Name = obj.ContainsKey("name") ? DateTime.Parse(obj["name"]) : null
                    });
                }
                return times;
            }
        }

        /// <summary>
        /// Возвращает полный список времени занятий
        /// </summary>
        /// <returns></returns>
        public static async Task<List<LessonTime>> GetCollectionAsync()
        {
            string sqlQuery = "select * from LessonsTime";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var lessons = ToLessons(result);
                return lessons;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<LessonTime>();
            }
        }
    }

    public class LessonPlan
    {
        public string TypeId { get; set; }
        public string FlowSubjectId { get; set; }
        public int Number { get; set; }


        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="lessonList"></param>
        /// <returns></returns>
        private static List<LessonPlan> ToLessons(List<Dictionary<string, dynamic>> lessonList)
        {
            if (lessonList.Count == 0)
                return new List<LessonPlan>();
            else
            {
                var plan = new List<LessonPlan>(lessonList.Count);
                foreach (var obj in lessonList)
                {
                    plan.Add(new LessonPlan
                    {
                        FlowSubjectId = obj.ContainsKey("FlowSubjectID") ? obj["FlowSubjectID"].ToString() : null,
                        TypeId = obj.ContainsKey("LessonTypeID") ? obj["LessonTypeID"].ToString() : null,
                        Number = obj.ContainsKey("Number") ? obj["Number"].ToString() : null
                    });
                }
                return plan;
            }
        }

        /// <summary>
        /// Возвращает план занятий
        /// </summary>
        /// <returns></returns>
        public static async Task<List<LessonPlan>> GetCollectionAsync(string flowSubjectId)
        {
            string sqlQuery = "select * from LessonPlan where FlowSubjectID=@subjId";
            var parameters = new Dictionary<string, string>
            {
                { "@subjId", flowSubjectId }
            };
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                var lessons = ToLessons(result);
                return lessons;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<LessonPlan>();
            }
        }

        /// <summary>
        /// Сохраняет текущий объект LessonPlan в БД
        /// </summary>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push()
        {
            string procName = "dbo.SetLessonPlan";
            var parameters = new Dictionary<string, string>
            {
                { "@subjectId", FlowSubjectId },
                { "@typeId", TypeId },
                { "@number", Number.ToString() }
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
    }

    public class LessonType
    {
        public string ID { get; set; }
        public string Name { get; set; }


        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="typeList"></param>
        /// <returns></returns>
        private static List<LessonType> ToLessons(List<Dictionary<string, dynamic>> typeList)
        {
            if (typeList.Count == 0)
                return new List<LessonType>();
            else
            {
                var types = new List<LessonType>(typeList.Count);
                foreach (var obj in typeList)
                {
                    types.Add(new LessonType
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        Name = obj.ContainsKey("name") ? obj["name"].ToString() : null
                    });
                }
                return types;
            }
        }

        /// <summary>
        /// Возвращает типы занятий
        /// </summary>
        /// <returns></returns>
        public static async Task<List<LessonType>> GetCollectionAsync()
        {
            string sqlQuery = "select * from LessonType";
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery);
                var lessons = ToLessons(result);
                return lessons;
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());//запись лога с ошибкой
                return new List<LessonType>();
            }
        }
    }

    public class LessonAttend : Lesson
    {
        public string StudentId { get; set; }

        /// <summary>
        /// Записывает объект LessonAttend в БД
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> Push()
        {
            string procName = "dbo.AddLessonAttend";
            var parameters = new Dictionary<string, string>
            {
                { "@lessonId", FlowSubjectId },
                { "@studentId", StudentId }
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
        public override bool Delete()
        {
            string procName = "dbo.DeleteLessonAttend";
            var parameters = new Dictionary<string, string>
            {
                { "@lessonId", ID },
                { "@studentId", StudentId }
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