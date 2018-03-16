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
        // проводит авторизацию, возвращает токен пользователя после ввода логина и пароля
        [HttpGet]
        [Route("api/Account/auth")]
        public async Task<dynamic> Auth([FromUri]string login=null, [FromUri]string password=null)
        {
            return null;
        }


        // GET: api/Account
        // возвращает данные определенного аккаунта
        public async Task<dynamic> Get(string id)
        {
            return null;
        }


        // POST: api/AccountW
        // Создание нового аккаунта
        public async Task<dynamic> Post([FromBody]AccountModels account)
        {
            return null;
        }


        // PUT: api/AccountW?login=5&password=5
        // изменение пароля для указанного аккаунта
        public async Task<dynamic> Put([FromBody]AccountModels account,
            [FromUri]string login=null, [FromUri]string password=null)
        {
            return null;
        }


        // PUT: api/AccountW/5
        // изменение данных аккаунта (для администратора)
        public async Task<dynamic> Put(string id, [FromBody]AccountModels account)
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
