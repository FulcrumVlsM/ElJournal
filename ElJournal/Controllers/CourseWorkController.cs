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


        // GET: api/CourseWork/State/5
        // получить статус выполнения курсовой работы студентом
        [HttpGet]
        [Route("api/CourseWork/State/{studentId}")]
        public async Task<dynamic> GetState(string studentId)
        {
            return null;
        }


        // POST: api/CourseWork
        // добавить курсовую работу
        public async Task<dynamic> Post([FromBody]CourseWorkModels courseWork)
        {
            return null;
        }


        // POST: api/CourseWork/Plan/5/5
        // добавить курсовую работу в план по предмету
        [HttpPost]
        [Route("api/CourseWork/Plan/{subjectId}/{workId}")]
        public async Task<dynamic> PostPlan(string subjectId, string workId)
        {
            return null;
        }


        // POST: api/CourseWork/Stage/5
        // добавление этапа процентовки в план по курсовой
        [HttpPost]
        [Route("api/CourseWork/Stage/{subjectId}")]
        public async Task<dynamic> PostStage(string subjectId, [FromBody]CourseWorkStageModels stage)
        {
            return null;
        }


        // POST: api/CourseWork/Stage/5/5
        // поставить отметку о выполнении процентовки
        [HttpPost]
        [Route("api/CourseWork/Stage/{id}/{studentId}")]
        public async Task<dynamic> PostStage(string id, string studentId, [FromUri]bool state=true)
        {
            return null;
        }


        // POST: api/CourseWork/State/5
        // поставить отметки о выполнении курсовой работы
        [HttpPost]
        [Route("api/CourseWork/State/{studentId}")]
        public async Task<dynamic> PostState(string studentId, [FromBody]CourseWorkExecutionModels advanced,
            [FromUri]bool state=true)
        {
            return null;
        }


        // PUT: api/CourseWork/5
        // изменить данные о курсовой работе
        public async Task<dynamic> Put(string id, [FromBody]CourseWorkModels courseWork)
        {
            return null;
        }


        // PUT: api/CourseWork/Stage/5
        // изменить данные процентовки курсовой работы
        [HttpPut]
        [Route("api/CourseWork/Stage/{id}")]
        public async Task<dynamic> PutStage(string id, [FromBody]CourseWorkStageModels stage)
        {
            return null;
        }


        // DELETE: api/CourseWork/5
        // удаление лабораторной работы
        public dynamic Delete(string id)
        {
            return null;
        }


        // удаление курсовой работы из плана
        [HttpDelete]
        [Route("api/CourseWork/Plan/{id}")]
        public dynamic DeletePlan(string id)
        {
            return null;
        }
    }
}
