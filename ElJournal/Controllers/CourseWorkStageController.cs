using ElJournal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ElJournal.Controllers
{
    public partial class CourseWorkController
    {
        // получить список процентовок курсовых работ (все рег. пользователи)
        // получить выполненные процентовки курсовых работ студентом (все рег. пользователи)
        // добавить этап процентовки в план по курсовой работе (администратор, преподаватель)
        // поставить отметку о выполнении процентовки (администратор, преподаватель)
        // изменить данные процентовки курсовой работы (администратор, преподаватель)
        // удалить этап процентовки (преподаватель, администратор)


        // получить список процентовок курсовых работ (все рег. пользователи)
        [HttpGet]
        [Route("api/CourseWork/stage/{subjectId}")]
        public async Task<HttpResponseMessage> GetStage(string subjectId)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // GET: api/CourseWork/Stage/5/5
        // получить выполненные процентовки курсовых работ студентом (все рег. пользователи)
        //TODO: метод еще пустой
        [HttpGet]
        [Route("api/CourseWork/stage/{studentId}/{subjectId}")]
        public async Task<HttpResponseMessage> GetStageExecution(string studentId, string subjectId)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // POST: api/CourseWork/Stage/5
        // добавить этап процентовки в план по курсовой работе (администратор, преподаватель)
        [HttpPost]
        [Route("api/CourseWork/stage/{subjectId}")]
        public async Task<HttpResponseMessage> PostStage(string subjectId, [FromBody]CourseWorkStage stage)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // POST: api/CourseWork/Stage/5/5
        // поставить отметку о выполнении процентовки (администратор, преподаватель)
        [HttpPost]
        [Route("api/CourseWork/stage/{id}/{studentId}")]
        public async Task<HttpResponseMessage> PostStage(string id, string studentId, [FromUri]bool state = true)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // PUT: api/CourseWork/Stage/5
        // изменить данные процентовки курсовой работы (администратор, преподаватель)
        [HttpPut]
        [Route("api/CourseWork/stage/{id}")]
        public async Task<HttpResponseMessage> PutStage(string id, [FromBody]CourseWorkStage stage)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // удалить этап процентовки (преподаватель, администратор)
        [HttpDelete]
        [Route("api/CourseWork/stage/{id}")]
        public HttpResponseMessage DeleteStage(string id)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}