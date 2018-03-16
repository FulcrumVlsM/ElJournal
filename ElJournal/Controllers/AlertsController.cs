using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ElJournal.DBInteract;
using ElJournal.Models;
using ElJournal.Providers;
using System.Threading.Tasks;

namespace ElJournal.Controllers
{
    public class AlertsController : ApiController
    {
        // вернуть список всех уведомлений (новостей). Без авторизации доступны только открытые (новостные)
        // вернуть уведомление по id (все рег. пользователи)
        // вернуть типы уведомлений (все)
        // добавить уведомление (общее, для кафедры, для факультета) (администратор, администратор кафедры, факультета)
        // добавить уведомление к указанному предмету (преподаватель)
        // Добавить новостное уведомление (администратор)
        // изменить уведомление (создатель, администратор)
        // удалить уведомление (создатель, администратор)


        // GET: api/Alerts
        // возвращает список всех уведомлений.
        //TODO: надо, чтобы авторизированный пользователь получал все, доступные ему уведомления: 
        //студент по предметам, на которых учится, своей кафедры, факультета и т.д.
        public async Task<dynamic> Get([FromUri]DateTime startDate=default(DateTime),
            [FromUri]DateTime endDate=default(DateTime))
        {
            return null;

        }

        // GET: api/Alerts/5
        // возвращает уведомление по id
        public async Task<dynamic> Get(string id)
        {
            return "value";
        }


        // GET: api/Alerts/types
        // возвращает типы уведомлений
        [HttpGet]
        [Route("api/Alerts/types")]
        public async Task<dynamic> GetType()
        {
            return null;
        }


        // POST: api/Alerts
        // добавление уведомления
        public async Task<dynamic> Post([FromBody]AlertModels alert)
        {
            return null;
        }


        // POST: api/Alerts/ByTeacher
        // Добавление уведомления по предмету для преподавателя
        [HttpPost]
        [Route("api/Alerts/ByTeacher")]
        public async Task<dynamic> PostByTeacher([FromBody]AlertModels alert)
        {
            return null;
        }


        // POST: api/Alerts/news
        // добавление новостного уведомления
        [HttpPost]
        [Route("api/Alerts/news")]
        public async Task<dynamic> PostNews([FromBody]AlertModels alert)
        {
            return null;
        }


        // PUT: api/Alerts/5
        // изменение уведомления
        public async Task<dynamic> Put(string id, [FromBody]AlertModels alert)
        {
            return null;
        }


        // DELETE: api/Alerts/5
        // удаление уведомления
        public dynamic Delete(string id)
        {
            return null;
        }
    }
}
