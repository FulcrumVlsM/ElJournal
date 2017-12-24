using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ElJournal.Controllers
{
    public class AccountController : ApiController
    {
        //TODO: нужно сделать следующие методы:
        /*1. Get: логин и пароль получать через header
          2. Post: создание нового аккаунта
          3. Put: изменение пароля и логина*/
        
        // GET: api/AccountW
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/AccountW/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/AccountW
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/AccountW/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/AccountW/5
        public void Delete(int id)
        {
        }
    }
}
