﻿using System;
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
    //develop: Elena
    public class AccountController : ApiController
    {
        //TODO: нужно сделать следующие методы:
        /*1. Get: логин и пароль получать через header
          2. Post: создание нового аккаунта
          3. Put: изменение пароля и логина*/
            //4. Добавить проверку пароля

        // GET: api/AccountW
        [HttpPost]
        [Route("api/Account/authorize")]
        public async Task<dynamic>Authorize([FromBody]AccountModels account)
        {
            
            return null;
        }
        public async Task<dynamic> Get()
        {
            //формат ответа
            Response response = new Response();
            string token = Request?.Headers?.Authorization?.Scheme; //id пользователя
            DB db = DB.GetInstance();
            AccountModels account = new AccountModels();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            try
            {
                bool right = await db.CheckPermission(account.authorId, "ACCOUNT_PERMISSION");
                if (right)
                {
                    response.Succesful = true;
                    response.Data = await db.ExecSelectQuery("SELECT Accounts.PersonID, People.name, Accounts.login, Accounts.password, Accounts.email FROM Accounts INNER JOIN People ON Accounts.PersonID = People.ID");
                }
                else
                    parameters.Add("@id", token);
                    response.Succesful = true;
                    response.Data = await db.ExecSelectQuery("select login, email from AccountPeople(@id)", parameters);             
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }
            return response;
        }

        // GET: api/AccountW/5
        public async Task<dynamic> Get(string id)
        {
            Response response = new Response();
            try
            {
                DB db = DB.GetInstance();
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("@id", id);
                response.Succesful = true;
                response.Data = (await db.ExecSelectQuery("select login, email from AccountPeople(@id)", parameters))[0];
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }
            return response;
        }


        // POST: api/AccountW
        public async Task<dynamic> Post([FromBody]AccountModels account)
        {
            Response response = new Response(); //формат ответа
            string sqlQuery = "insert into Accounts(PersonID,login,password,email) " +
                        "values(@PersonID,@login,@password,@email)";

            try
            {
                DB db = DB.GetInstance();
                
                    //необходимые параметры запроса
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@PersonID", account.PersonID);
                    parameters.Add("@login", account.login);
                    parameters.Add("@password", account.password);
                    parameters.Add("@email", account.email);

                    //выполнение запроса
                    int res = db.ExecInsOrDelQuery(sqlQuery, parameters);
                    if (res != 0)
                    {
                        response.Succesful = true;
                        response.message = "New department was added";
                    }
                    else
                        response.message = "Account not added";
                
                
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }

        // PUT: api/AccountW/5
        public async Task<dynamic> Put(string id, [FromBody]AccountModels account)
        {
            Response response = new Response(); //формат результата запроса
            string sqlQuery = "dbo.UpdateAccounts";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(account.authorId, "ACCOUNT_PERMISSION");
                if (right)
                {
                    //выполнение операции
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@ID", id);
                    parameters.Add("@login", account.login);
                    parameters.Add("@password", account.password);
                    parameters.Add("@email", account.email);
                    int res = db.ExecStoredProcedure(sqlQuery, parameters);
                    if (res == 0)
                        response.Succesful = true;
                    else
                        response.message = "Operation was false";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }

        // DELETE: api/AccountW/5
        public async Task<dynamic> Delete(string id, [FromBody]AccountModels account)
        {
            Response response = new Response(); //формат ответа
            string sqlQuery = "delete from Accounts where ID=@ID";

            try
            {
                DB db = DB.GetInstance();
                bool right = await db.CheckPermission(account.authorId, "ACCOUNT_PERMISSION");
                if (right)
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@ID", id);
                    int result = db.ExecInsOrDelQuery(sqlQuery, parameters);
                    if (result == 1)
                    {
                        response.Succesful = true;
                        response.message = String.Format("Department was deleted");
                    }
                    else
                        response.message = "Operation was failed";
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch (Exception e)
            {
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }        
    }
}
