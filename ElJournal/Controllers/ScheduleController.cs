using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ElJournal.Controllers
{
    public class ScheduleController : ApiController
    {
        // получить расписание для всех групп в указанный семестр
        // получить расписание для указанной группы в указанный семестр
        // получить расписание преподавателя в указанный семестр
        // установить предмет в расписание
        // удалить расписание группы на указанный семестр

        
        // GET: api/Schedule/5
        // получить расписание для всех групп в указанный семестр
        [HttpGet]
        [Route("api/Schedule/{semesterId}")]
        public async Task<dynamic> Get(string semesterId)
        {
            return null;
        }


        // GET: api/Schedule/5/5
        // получить расписание для указанной группы
        [HttpGet]
        [Route("api/Schedule/{semesterId}/{groupId}")]
        public async Task<dynamic> Get(int semesterId, string groupId)
        {
            return null;
        }


        // GET: api/Schedule/Teacher/5
        // получить расписание преподавателя в указанный семестр
        [HttpGet]
        [Route("api/Schedule/Teacher/{id}")]
        public async Task<dynamic> GetByTeacher(string id)
        {
            return null;
        }


        // POST: api/Schedule
        // установить предмет в расписание
        [Route("api/Schedule/{subjectId}/{timeId}")]
        public async Task<dynamic> Post(string subjectId, string timeId)
        {
            return null;
        }

        // DELETE: api/Schedule/5
        // удалить расписание группы на указанный семестр
        public async Task<dynamic> Delete(string id)
        {
            return null;
        }
    }
}
