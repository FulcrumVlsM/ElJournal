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
    public class FacultiesController : ApiController
    {
        // GET: api/Faculties
        // полный список всех факультетов
        public async Task<dynamic> Get()
        {
            //формат ответа
            Response response = new Response();

            try
            {
                DB db = DB.GetInstance();
                response.Succesful = true;
                response.Data = await db.ExecSelectQuery("select * from Faculties");
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }
            return response;
        }

        // GET: api/Faculties/guid
        // возвращает данные по конкретному факультету
        public async Task<dynamic> Get(string id)
        {
            Response response = new Response();

            try
            {
                DB db = DB.GetInstance();
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("@id", id);
                response.Succesful = true;
                response.Data = await db.ExecSelectQuery("select * from Faculties where ID=@id", parameters);
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }
        

        // POST: api/Faculties
        public async Task<dynamic> Post()
        {
            //формат ответа
            Response response = new Response();

            try
            {
                //получение параметров
                string authorId = default(string),
                    name = default(string),
                    dekanId = default(string),
                    description = default(string);
                string inDataString = await Request.Content.ReadAsStringAsync();
                dynamic inData;

                if (inDataString.CompareTo(String.Empty) == 0)
                {
                    response.Error = ErrorMessage.INCORRECT_REQUEST_DATA;
                }
                else
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
                        response.Error = ErrorMessage.INCORRECT_REQUEST_DATA;
                        return response;
                    }

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
                        {
                            response.Succesful = true;
                            response.message = "New faculty was added";
                        }
                        else
                        {
                            response.message = "Faculty not added";
                        }
                    }
                    else
                    {
                        response.Error = ErrorMessage.PERMISSION_ERROR;
                    }
                }
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;

        }

        // PUT: api/Faculties/5
        public async Task<dynamic> Put(string id)
        {
            //для результата запроса
            Response response = new Response();

            try
            {
                //получение параметров
                string authorId = default(string),
                    name = default(string),
                    dekanId = default(string),
                    description = default(string);
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
                        response.Error = ErrorMessage.INCORRECT_REQUEST_DATA;
                        return response;
                    }
                }
                else
                {
                    response.Succesful = false;
                    response.Error = ErrorMessage.INCORRECT_REQUEST_DATA;
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
                    response.Error = ErrorMessage.PERMISSION_ERROR;
                }
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }

        // DELETE: api/Faculties/5
        public async Task<dynamic> Delete(string id)
        {
            Response response = new Response();

            try
            {
                string authorId = default(string);
                string inDataString = await Request.Content.ReadAsStringAsync();
                dynamic inData;
                if (inDataString.CompareTo("") == 0)
                {
                    response.Succesful = false;
                    response.Error = ErrorMessage.INCORRECT_REQUEST_DATA;
                }
                else
                {
                    inData = JsonConvert.DeserializeObject(inDataString);
                    try
                    {
                        authorId = inData.authorId;
                    }
                    catch (NullReferenceException)
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
                        sqlQuery = "delete from Faculties where ID=@ID";
                        parameters.Clear();
                        parameters.Add("@ID", id);
                        int result = db.ExecInsOrDelQuery(sqlQuery, parameters);
                        if (result == 1)
                        {
                            response.Succesful = true;
                            response.message = String.Format("Faculty was deleted");
                        }
                        else
                        {
                            response.Succesful = false;
                            response.message = "Operation was failed";
                        }
                    }
                    else
                    {
                        response.Succesful = false;
                        response.Error = ErrorMessage.PERMISSION_ERROR;
                    }
                }
            }
            catch(Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }
    }
}
