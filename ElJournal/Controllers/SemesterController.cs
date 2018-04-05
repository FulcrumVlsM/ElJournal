using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ElJournal.Models;
using System.Threading.Tasks;
using ElJournal.Providers;

namespace ElJournal.Controllers
{
    public class SemesterController : ApiController
    {
        // вернуть список всех семестров (все)
        // вернуть семестр по id (все)
        // добавить новый семестр (администратор)
        // изменить данные для указанного семестра (администратор)
        // удалить семестр (администратор)


        // GET: api/Semester
        //возвращает список всех семестров
        public async Task<dynamic> Get()
        {
            return null;
        }

        // GET: api/Semester/5
        // возвращает полную информацию об указанном семестре
        public async Task<dynamic> Get(string id)
        {
            return null;
        }

        // POST: api/Semester
        // добавление семестра
        public dynamic Post([FromBody]Semester semester)
        {
            return null;
        }

        // PUT: api/Semester/5
        // изменение данных о семестре
        public dynamic Put(string id, [FromBody]Semester semester)
        {
            return null;
        }

        // DELETE: api/Semester/5
        // удаление семестра
        public dynamic Delete(string id)
        {
            return null;
        }
    }
}
