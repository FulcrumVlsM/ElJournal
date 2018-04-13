using ElJournal.DBInteract;
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
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SessionStart { get; set; }
        public DateTime SessionEnd { get; set; }

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
            var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);

            if (result != null)
                return new Semester
                {
                    ID = result.ContainsKey("ID") ? result["ID"].ToString() : null,
                    Name = result.ContainsKey("name") ? result["name"].ToString() : null,
                    StartDate = result.ContainsKey("StartDate") ? result["StartDate"] : default(DateTime),
                    EndDate = result.ContainsKey("EndDate") ? result["EndDate"] : default(DateTime),
                    SessionStart = result.ContainsKey("SessionStartDate") ? result["SessionStartDate"] : default(DateTime),
                    SessionEnd = result.ContainsKey("SessionEndDate") ? result["SessionEndDate"] : default(DateTime)
                };
            else
                return null;
        }
    }
}