using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ElJournal.DBInteract;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ElJournal.Models;

namespace ElJournal.Controllers
{
    public class FacultiesController : ApiController
    {
        // GET: api/Faculties
        // полный список всех факультетов
        public async Task<dynamic> Get()
        {
            DB db = DB.GetInstance();
            return await db.ExecSelectQuery("select * from Faculties");
        }

        // GET: api/Faculties/guid
        // возвращает данные по конкретному факультету
        public async Task<dynamic> Get(string id)
        {
            DB db = DB.GetInstance();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@id", id);
            return await db.ExecSelectQuery("select * from Faculties where ID=@id", parameters);
        }
        

        // POST: api/Faculties
        public async Task<dynamic> Post([FromBody]string authorId,
                                        [FromBody]string name,
                                        [FromBody]string dekanId,
                                        [FromBody]string description)
        {
            DB db = DB.GetInstance();
            string sqlQuery = "dbo.CheckRight(@personID,@permission)";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@personID", authorId);
            parameters.Add("@permission", "FACULTY_ALL_PERMISSION");
            bool right = await db.ExecuteScalarQuery(sqlQuery, parameters);
            if (right)
            {
                sqlQuery = "insert into Faculties(dekanPersonID,name,description) " +
                    "values(@dekanPersonID,@name,@description";
                parameters.Clear();
                parameters.Add("@dekanPersonID", dekanId);
                parameters.Add("@name", name);
                parameters.Add("@description", description);
                int res = db.ExecInsOrDelQuery(sqlQuery, parameters);
                if (res != 0)
                    return true;
                else return false;
            }
            else
                return false;

        }

        // PUT: api/Faculties/5
        public async Task<dynamic> Put(string id)
        {
            //для результата запроса
            Response response = new Response();

            //получение параметров
            string authorId=default(string),
                name=default(string),
                dekanId=default(string),
                description=default(string);
            string inDataString = await Request.Content.ReadAsStringAsync();
            dynamic inData;
            if (inDataString.CompareTo("") != 0)
            {
                inData = JsonConvert.DeserializeObject(inDataString);
                try
                {
                    if (inData.authorId.ToString().compareTo("") != 0) authorId = inData.authorId;
                    if (inData.name.ToString().compareTo("") != 0) name = inData.name;
                    if (inData.dekanId.ToString().compareTo("") != 0) dekanId = inData.dekanId;
                    if (inData.description.ToString().compareTo("") != 0) description = inData.description;
                }
                catch (NullReferenceException)
                {
                    response.Succesful = false;
                    response.Error = "Incorrect request data";
                    return response;
                }
            }
            else
            {
                response.Succesful = false;
                response.Error = "Incorrect request data";
                return response;
            }

            DB db = DB.GetInstance();

            //проверка прав на операцию
            string sqlQuery = "dbo.CheckRight(@personID,@permission)";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@personID", authorId);
            parameters.Add("@permission", "FACULTY_ALL_PERMISSION");
            bool right = await db.ExecuteScalarQuery(sqlQuery, parameters);

            if (right)
            {
                //выполнение операции
                sqlQuery = "dbo.UpdateFaculty";
                parameters.Clear();
                parameters.Add("@ID", id);
                parameters.Add("@dekanID", dekanId);
                parameters.Add("@name", name);
                parameters.Add("@description", description);
                int res = db.ExecStoredProcedure(sqlQuery, parameters);
                if (res == 0)
                    response.Succesful = true;
                else
                {
                    response.Succesful = false;
                    response.message = "Operation was false";
                }
            }
            else
            {
                response.Succesful = false;
                response.Error = "You don't have permission for this operation";
            }

            return response;
        }

        // DELETE: api/Faculties/5
        public async Task<dynamic> Delete(string id, [FromBody]string authorId)
        {
            return null;
        }
    }
}
