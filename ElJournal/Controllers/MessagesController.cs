using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Models;
using ElJournal.Providers;

namespace ElJournal.Controllers
{
    public class MessagesController : ApiController
    {
        // вернуть диалог с указанным пользователем (должен быть флаг для пометки архивных сообщений)
        // вернуть непрочитанные сообщения. При этом, пометить их как прочитанные
        // отправить сообщение указанному пользователю
        // изменить сообщение
        // удалить сообщение


        // GET: api/Messages/5
        // возвращает весь диалог с указанным пользователем (при флаге archive еще и архивные сообщения)
        public async Task<dynamic> Get(string id, [FromUri]bool archive=false)
        {
            return null;
        }

        // GET: api/Messages
        // возвращает все непрочитанные сообщения
        public async Task<dynamic> Get()
        {
            return null;
        }

        // POST: api/Messages
        // отправка сообщения указанному пользователю
        public async Task<dynamic> Post([FromBody]MessageModels message)
        {
            return null;
        }


        // PUT: api/Messages/5
        // изменение текста сообщения
        public async Task<dynamic> Put(string id, [FromBody]MessageModels message)
        {
            return null;
        }

        // DELETE: api/Messages/5
        // удаление сообщения
        public dynamic Delete(string id)
        {
            return null;
        }
    }
}
