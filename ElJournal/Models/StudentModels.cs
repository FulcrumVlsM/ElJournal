using ElJournal.DBInteract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Student
    {
        public string ID { get; set; }
        public string personId { get; set; }
        public string groupId { get; set; }
        public string semesterId { get; set; }

        /// <summary>
        /// Возвращает данные по всем семестрам, в которых находится данный пользователь
        /// </summary>
        /// <param name="personId">id пользователя</param>
        /// <returns></returns>
        public static async Task<List<Student>> GetStudents(string personId)
        {
            string sqlQuery = "select * from dbo.GetAllStudentGroupsSemesters(@personId)";
            var parameters = new Dictionary<string, string>
            {
                {"@personId", personId }
            };
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                List<Student> students = Student.ToStudents(result);
                return students;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return new List<Student>(1);
            }

        }

        /// <summary>
        /// Возвращает список студентов группы в указанном семестре
        /// </summary>
        /// <param name="semesterId">id семестра</param>
        /// <param name="groupId">id группы</param>
        /// <returns></returns>
        public static async Task<List<Student>> GetGroupStudent(string semesterId, string groupId)
        {
            string sqlQuery = "select * from dbo.GetStudentsByGroup(@groupId,@semesterId)";
            var parameters = new Dictionary<string, string>
            {
                {"@groupId", groupId },
                {"@semesterId", semesterId }
            };
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                return ToStudents(result);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return new List<Student>(1);
            }

        }

        /// <summary>
        /// Возвращает список студентов, для указанного предмета, с вычетом студентов, которые данный предмет не изучают
        /// </summary>
        /// <param name="subjGrSemId">id предмета (предмет-группа-семестр)</param>
        /// <returns></returns>
        public static async Task<List<Student>> GetSubjectStudents(string subjGrSemId)
        {
            string sqlQuery = "select * from dbo.GetStudentsOfSubject(@subjectId)";
            var parameters = new Dictionary<string, string>
            {
                {"@subjectId", subjGrSemId }
            };
            try
            {
                DB db = DB.GetInstance();
                var result = await db.ExecSelectQueryAsync(sqlQuery, parameters);
                return ToStudents(result);
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return new List<Student>(1);
            }
        }

        /// <summary>
        /// Производит преобразование коллекции, полученной методами класса DB в коллекцию моделей
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static List<Student> ToStudents(List<Dictionary<string, dynamic>> objects)
        {
            if (objects.Count == 0)
                return null;
            else
            {
                var subjects = new List<Student>(objects.Count);
                foreach (var obj in objects)
                {
                    subjects.Add(new Student
                    {
                        ID = obj.ContainsKey("ID") ? obj["ID"].ToString() : null,
                        personId = obj.ContainsKey("PersonID") ? obj["PersonID"].ToString() : null,
                        groupId = obj.ContainsKey("GroupsID") ? obj["GroupsID"] : null,
                        semesterId = obj.ContainsKey("SemesterID") ? obj["SemesterID"] : null
                    });
                }
                return subjects;
            }
        }
    }
}