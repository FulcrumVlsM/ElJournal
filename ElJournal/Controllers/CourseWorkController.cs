using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ElJournal.Controllers
{
    public class CourseWorkController : ApiController
    {
        // получить список всех курсовых работ (администратор)
        // получить курсовую работу по id (все рег. пользователи)
        // получить курсовые работы по предмету
        // получить выполненные процентовки курсовых работ студентом (все рег. пользователи)
        // получить статус выполнения курсовой работы студентом (да, нет)
        // добавить курсовую работу (администратор, преподаватель)
        // добавить курсовую работу в план по предмету (администратор, преподаватель)
        // добавить этап процентовки в план по курсовой работе (администратор, преподаватель)
        // поставить отметку о выполнении процентовки (администратор, преподаватель)
        // поставить отметки о выполнении курсовой работы (администратор, преподаватель)
        // изменить данные о курсовой работе (администратор, создатель)
        // изменить данные процентовки курсовой работы (администратор, преподаватель)
        // удалить курсовую работу (администратор, создатель)
        // удалить курсовую работу из плана (администратор, преподаватель)
        // удалить отметку о выполнении процентовки (администратор, преподаватель)
        // удалить отметку о выполнении курсовой работы (администратор, преподаватель)
        // удалить этап процентовки (преподаватель, администратор)


        // GET: api/CourseWork
        // получение списка всех курсовых работ
        public async Task<dynamic> Get([FromUri]string name=null)
        {
            return null;
        }


        // GET: api/CourseWork/5
        // получить курсовую работу по id
        [HttpGet]
        [Route("api/CourseWork/{id}")]
        public async Task<dynamic> GetById(string id)
        {
            return null;
        }


        // GET: api/CourseWork/BySubject/5
        // получить курсовые работы по предмету
        [HttpGet]
        [Route("api/CourseWork/BySubject/{id}")]
        public async Task<dynamic> GetBySubj(string id)
        {
            return null;
        }


        // GET: api/CourseWork/Stage/5/5
        // получить выполненные студентом процентовки для курсовых работ
        [HttpGet]
        [Route("api/CourseWork/Stage/{studentId}/{subjectId}")]
        public async Task<dynamic> GetStage(string studentId, string subjectId)
        {
            return null;
        }

        // POST: api/CourseWork
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/CourseWork/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/CourseWork/5
        public void Delete(int id)
        {
        }
    }
}
