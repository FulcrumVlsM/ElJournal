using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ElJournal.Models;
using ElJournal.Providers;
using System.Threading.Tasks;
using ElJournal.DBInteract;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Web;
using System.Configuration;

namespace ElJournal.Controllers
{
    public class AccountController : ApiController
    {
        // вернуть токен по логину и паролю (все)
        // вернуть данные аккаунта по ID (администратор)
        // вернуть все аккаунту (только публичные данные)
        // Добавить новый аккаунт (регистрация пользователя) (все)
        // Изменить данные своего аккаунта (все рег. пользователи)
        // Удалить аккаунт (администратор)



        // GET: api/Account/auth/5/5
        // вернуть токен по логину и паролю (все)
        [HttpPost]
        [Route("api/Account/auth")]
        public async Task<HttpResponseMessage> Auth([FromBody]LogIn data)
        {
            Response response = new Response();
            Account account = await data.Authorize();
            if(account != null)
            {
                Person person = await Person.GetInstanceAsync(account.PersonID);
                response.Data = person.GetToken();
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
                return Request.CreateResponse(HttpStatusCode.BadRequest);
        }


        // вернуть все аккаунту (только публичные данные)
        public async Task<HttpResponseMessage> Get([FromUri]string person = null, [FromUri]string name = null)
        {
            Response response = new Response();
            var accounts = await Account.GetCollectionAsync();

            if (person != null) //отбор по пользователю
                accounts = accounts.FindAll(x => x.PersonID == person);

            if (name != null) //поиск по имени
            {
                Regex regex = new Regex(name);
                accounts = accounts.FindAll(x => regex.IsMatch(x.Alias));
            }

            response.Data = accounts;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // GET: api/Account
        // вернуть данные аккаунта по ID (администратор)
        public async Task<HttpResponseMessage> Get(string id)
        {
            Response response = new Response();
            
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            bool commonRight = authProvider.CheckPermission(Permission.ACCOUNT_PERMISSION);
            if (commonRight)
            {
                Account account = await Account.GetInstanceAsync(id);
                response.Data = account;
                if (account != null)
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // POST: api/AccountW
        // Добавить новый аккаунт (регистрация пользователя) (все)
        [HttpPost]
        [Route("api/Account")]
        public async Task<HttpResponseMessage> Post([FromBody]NewAccount account)
        {
            // поиск указанного пользователя
            Person person = await Person.GetInstanceAsync(account?.PersonID);
            if(person == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка корректности введенного секретного слова пользователя
            if (account.Secret == person.Secret || 
                account.Secret == person.Student_id || 
                account.Secret == person.Passport_id)
            {
                if (await account.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.BadRequest);
        }


        // PUT: api/AccountW?login=5&password=5
        // Изменить данные своего аккаунта (все рег. пользователи)
        [HttpPut]
        [Route("api/Account")]
        public async Task<HttpResponseMessage> Put([FromBody]NewAccount account)
        {
            Account realAccount = await Account.GetInstanceAsync(account?.Email, account?.Password);
            if (realAccount == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            realAccount.Password = account.NewPassword;
            realAccount.Alias = account?.Alias;
            realAccount.Email = account?.Email;

            if (await realAccount.Update())
                return Request.CreateResponse(HttpStatusCode.OK);
            else
                return Request.CreateResponse(HttpStatusCode.BadRequest);
        }


        // DELETE: api/AccountW/5
        // удалить аккаунт (администратор)
        [HttpDelete]
        [Route("api/Account/{id}")]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            Account account = await Account.GetInstanceAsync(id);
            if (account == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            bool commonRight = authProvider.CheckPermission(Permission.ACCOUNT_PERMISSION);
            if (commonRight)
            {
                if(account.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // восстановление пароля (через почту)
        //TODO: метод еще не готов
        [HttpPost]
        [Route("api/Account/restore")]
        public async Task<HttpResponseMessage> Restore(Account account)
        {
            string email = ConfigurationManager.AppSettings["email"];
            string emailPassword = ConfigurationManager.AppSettings["emailPassword"];

            //TODO: тело должно быть в виде html, отдельным файлом. 

            MailReceiver receiver = new MailReceiver
            {
                From = email,
                To = account?.Email,
                Title = "Изменение пароля",
                Body = "Вы забыли свой пароль. " +
                       "Ваш новый пароль: {0}.",
                SmtpAdress = "smtp.gmail.com",
                Port = 587,
                EmailPassword = emailPassword,
                EnableSsl = true
            };
            if (await receiver.Receive())
                return Request.CreateResponse(HttpStatusCode.OK);
            else
                return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
