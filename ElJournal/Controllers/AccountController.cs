using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ElJournal.Models;
using ElJournal.Providers;
using System.Threading.Tasks;

namespace ElJournal.Controllers
{
    public class AccountController : ApiController
    {
        // вернуть токен по логину и паролю (все)
        // вернуть данные аккаунта по ID (администратор)
        // Добавить новый аккаунт (регистрация пользователя) (все)
        // Изменить логин и пароль (все рег. пользователи)
        // Изменить данные аккаунта по id (администратор)
        // Удалить аккаунт (администратор)



        // GET: api/Account/auth/5/5
        // вернуть токен по логину и паролю (все)
        [HttpGet]
        [Route("api/Account/auth")]
        public async Task<dynamic> Auth([FromUri]string login=null, [FromUri]string password=null)
        {
            return null;
        }


        // GET: api/Account
        // вернуть данные аккаунта по ID (администратор)
        public async Task<dynamic> Get(string id)
        {
            return null;
        }


        // POST: api/AccountW
        // Добавить новый аккаунт (регистрация пользователя) (все)
        public async Task<dynamic> Post([FromBody]Account account)
        {
            return null;
        }


        // PUT: api/AccountW?login=5&password=5
        // изменение пароля для указанного аккаунта
        // Изменить логин и пароль (все рег. пользователи)
        public async Task<dynamic> Put([FromBody]Account account,
            [FromUri]string login=null, [FromUri]string password=null)
        {
            return null;
        }


        // PUT: api/AccountW/5
        // Изменить данные аккаунта по id (администратор)
        public async Task<dynamic> Put(string id, [FromBody]Account account)
        {
            return null;
        }


        // DELETE: api/AccountW/5
        // удалить аккаунт (администратор)
        public dynamic Delete(string id)
        {
            return null;
        }
    }
}
