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
using System.Data.SqlClient;

namespace ElJournal.Controllers
{
    //develop: Mikhail
    public class FacultiesController : ApiController
    {
        //TODO: проверка прав пользователя учитывает в данный момент только разрешения общего типа

        private const string FACULTY_COMMON_PERMISSION = "FACULTY_COMMON_PERMISSION"; //общее разрешение
        private const string FACULTY_PERMISSION = "FACULTY_PERMISSION"; //разрешение на связанный

        // GET: api/Faculties
        public async Task<dynamic> Get()
        {
            Response response = new Response();//формат ответа
            await Task.Run(() =>
            {
                try
                {
                    DB db = DB.GetInstance();
                    response.Data = db.Faculties;//запроск БД
                    response.Succesful = true;
                }
                catch (Exception e)
                {
                    response.Error = e.ToString();
                    response.message = e.Message;
                    //TODO: add log
                }
            });
            return response;
        }

        // GET: api/Faculties/guid
        public async Task<dynamic> Get(string id)
        {
            Response response = new Response();//формат ответа
            await Task.Run(() =>
            {
                try
                {
                    DB db = DB.GetInstance();
                    response.Data = db.Faculties.Where(x => x["ID"].Equals(id));
                    response.Succesful = true;
                }
                catch (Exception e)
                {
                    response.Error = e.ToString();
                    response.message = e.Message;
                    //TODO: add log
                }
            });
            return response;
        }
        

        // POST: api/Faculties
        //TODO: запись в БД идет, но возвращается false
        public async Task<dynamic> Post([FromBody]Faculty faculty)
        {   
            Response response = new Response(); //формат ответа
            var parameters = new Dictionary<string, string>();
            string sqlAddQuery = "dbo.AddFaculty";
            string sqlSQuery = "select top 1 ID from Faculties where name=@name";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(faculty.authorId, FACULTY_COMMON_PERMISSION);
                
                if (right) //если у пользователя есть права на операцию
                {
                    parameters.Add("@dekanID", faculty.dekanId); //добавление параметров к запросу
                    parameters.Add("@name", faculty.name);
                    parameters.Add("@description", faculty.description);

                    int res = db.ExecStoredProcedure(sqlAddQuery, parameters); //выполнение запроса
                    if (res != 0)
                    {
                        response.Succesful = true;
                        response.message = "New faculty was added";
                        response.Data = new { ID = db.ExecuteScalarQuery(sqlSQuery,parameters) };
                    }
                    else
                        response.message = "Faculty not added";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
                //TODO: add log
            }

            return response;
        }

        // PUT: api/Faculties/guid
        public async Task<dynamic> Put(string id, [FromBody]Faculty faculty)
        {
            Response response = new Response(); //формат результата запроса
            var parameters = new Dictionary<string, string>();
            string sqlQuery = "dbo.UpdateFaculty";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(faculty.authorId, FACULTY_COMMON_PERMISSION);
                if (right)
                {
                    parameters.Add("@ID", id);              //добавление параметров к запросу
                    parameters.Add("@dekanID", faculty.dekanId);
                    parameters.Add("@name", faculty.name);
                    parameters.Add("@description", faculty.description);
                    int res = db.ExecStoredProcedure(sqlQuery, parameters);//выполнение запроса
                    if (res == 0)
                        response.Succesful = true;
                    else
                        response.message = "Operation was false";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
                //TODO: add log
            }

            return response;
        }

        // DELETE: api/Faculties/guid
        public async Task<dynamic> Delete(string id, [FromBody]Faculty faculty)
        {
            Response response = new Response(); //формат ответа
            var parameters = new Dictionary<string, string>();
            string sqlDelQuery = "delete from Faculties where ID=@ID";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(faculty.authorId, FACULTY_COMMON_PERMISSION);//проверка наличия прав
                if (right)
                {
                    parameters.Add("@ID", id);
                    int result = db.ExecInsOrDelQuery(sqlDelQuery, parameters);
                    if (result == 1)
                    {
                        response.Succesful = true;
                        response.message = String.Format("Faculty was deleted");
                    }
                    else
                        response.message = "Operation was failed";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
                //TODO: add log
            }

            return response;
        }
    }
}
