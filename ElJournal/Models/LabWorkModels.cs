using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ElJournal.DBInteract;

namespace ElJournal.Models
{
    public class LabWork
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string FileURL { get; set; }
        public string Advanced { get; set; }


        /// <summary>
        /// Возвращает лабораторную работу по указанному ID
        /// </summary>
        /// <param name="id">id лабораторной работы</param>
        /// <returns></returns>
        public static async Task<LabWork> GetInstanceAsync(string id)
        {
            string sqlQuery = "select * from LabWorks where ID=@id";
            var parameters = new Dictionary<string, string>();
            parameters.Add("@id", id);
            DB db = DB.GetInstance();
            var result = await db.ExecSelectQuerySingleAsync(sqlQuery, parameters);
            return new LabWork
            {
                ID = result["ID"],
                Name = result["name"],
                FileName = result["fileName"],
                FileURL = result["fileURL"],
                Advanced = result["advanced"]
            };
        }

        /// <summary>
        /// Возвращает полный список лабораторных работ
        /// </summary>
        /// <returns></returns>
        public static async Task<List<LabWork>> GetCollectionAsync()
        {
            string sqlQuery = "select * from LabWorks";
            DB db = DB.GetInstance();
            var result = await db.ExecSelectQueryAsync(sqlQuery);
            var labWorks = new List<LabWork>(result.Count);
            foreach(var obj in result)
            {
                labWorks.Add(new LabWork
                {
                    ID = obj["ID"],
                    Name = obj["name"],
                    FileName = obj["fileName"],
                    FileURL = obj["fileURL"],
                    Advanced = obj["advanced"]
                });
            }

            return labWorks;
        }

        /// <summary>
        /// Сохраняет текущий объект LabWork в БД
        /// </summary>
        /// <param name="authorId">автор</param>
        /// <returns>True, если объект был добавлен в БД</returns>
        public async Task<bool> Push(string authorId)
        {
            var parameters = new Dictionary<string, string>();
            string procName = "dbo.AddLabWork";
            parameters.Add("@name", Name);
            parameters.Add("@advanced", Advanced);
            parameters.Add("@authorId", authorId);
            DB db = DB.GetInstance();
            return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
        }

        /// <summary>
        /// Сохраняет указанное имя и URL файла как присоединенный файл
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AttachFile()
        {
            string sqlQuery = "dbo.UpdateLabWork";
            var parameters = new Dictionary<string, string>();
            DB db = DB.GetInstance();
            parameters.Add("@ID", ID);
            parameters.Add("@fileName", FileName);
            parameters.Add("@fileURL", FileURL);
            return Convert.ToBoolean(await db.ExecStoredProcedureAsync(sqlQuery, parameters));
        }

        /// <summary>
        /// Обновляет в БД выбранный объект (по ID)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Update()
        {
            string procName = "dbo.UpdateLabWork";
            var parameters = new Dictionary<string, string>();
            DB db = DB.GetInstance();
            parameters.Add("@ID", ID);
            parameters.Add("@name", Name);
            parameters.Add("@advanced", Advanced);
            return Convert.ToBoolean(await db.ExecStoredProcedureAsync(procName, parameters));
        }

        /// <summary>
        /// Удаление текущего объекта из БД
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            string sqlQuery = "delete from LabWorks where ID=@ID";
            var parameters = new Dictionary<string, string>();
            parameters.Add("@ID", ID);
            DB db = DB.GetInstance();
            int result = db.ExecInsOrDelQuery(sqlQuery, parameters);
            if (result == 1)
            {
                ID = null;
                return true;
            }
            else
                return false;
        }

    }
}