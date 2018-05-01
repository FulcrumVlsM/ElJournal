using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Models;

namespace ElJournal.Controllers
{
    public class PracticeWorkController : ApiController
    {
        // получить весь список практических (все рег. пользователи)
        // получить список моих практических работ (все рег. пользователи)
        // получить план практических по определенному предмету (все)
        // получить список выполненных студентом практ работ по предмету (все рег. пользователи)
        // получить конкретную практическую по id (все)
        // получить файл, прикрепленный к практ работе (все рег. пользователи)
        // добавить практическую работу в план (автор, администратор)
        // установить практичскую работу из плана как выполненную студентом (преподаватель, администратор)
        // добавить практическую работу (преподаватель, администратор)
        // добавить файл к практической работе (автор, администратор)
        // изменить практическую работу (автор, администратор)
        // удалить практ работу из плана (преподаватель, администратор)
        // удалить факт выполнения практической работы (преподаватель, администратор)
        // удалить практическую работу (администратор)
        // удалить файл из практической работы (автор, администратор)

        // GET: api/PracticeWork
        // получить весь список практических (все рег. пользователи)
        public async Task<HttpResponseMessage> Get([FromUri]string name = null, [FromUri]int count = 50)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // получить список моих практических работ (все рег. пользователи)
        [HttpGet]
        [Route("api/PracticeWork/my")]
        public async Task<HttpResponseMessage> GetMy()
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // получить план практических по определенному предмету (все)
        [HttpGet]
        [Route("api/PracticeWork/plan/{flowSubjectId}")]
        public async Task<HttpResponseMessage> GetPlan(string flowSubjectId)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // получить список выполненных студентом практ работ по предмету (все рег. пользователи)
        [HttpGet]
        [Route("api/PracticeWork/exec/{studentFlowId}/{subjectFlowId}")]
        public async Task<HttpResponseMessage> GetExec(string studentFlowId, string subjectFlowId)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // GET: api/PracticeWork/5
        // получить конкретную практическую по id (все)
        public async Task<HttpResponseMessage> Get(string id)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // получить файл, прикрепленный к практ работе (все рег. пользователи)
        [HttpGet]
        [Route("api/PracticeWork/file/{id}")]
        public async Task<HttpResponseMessage> GetFile(string id)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // добавить практическую работу в план (автор, администратор)
        [HttpPost]
        [Route("api/PracticeWork/plan/{subjectFlowId}/{workId}")]
        public async Task<HttpResponseMessage> PostPlan(string subjectFlowId, string workId)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // установить практичскую работу из плана как выполненную студентом (преподаватель, администратор)
        [HttpPost]
        [Route("api/PracticeWork/exec")]
        public async Task<HttpResponseMessage> PostExec([FromBody]ExecutedPractWork executedPract)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // POST: api/PracticeWork
        // добавить практическую работу (преподаватель, администратор)
        [HttpPost]
        [Route("api/PracticeWork")]
        public async Task<HttpResponseMessage> Post([FromBody]PracticeWork work)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // добавить файл к практической работе (автор, администратор)
        [HttpPost]
        [Route("api/PracticeWork")]
        public async Task<HttpResponseMessage> Post(string id)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // PUT: api/PracticeWork/5
        // изменить практическую работу (автор, администратор)
        public async Task<HttpResponseMessage> Put(string id, [FromBody]PracticeWork work)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // удалить практ работу из плана (преподаватель, администратор)
        [HttpDelete]
        [Route("api/PracticeWork/plan/{id}")]
        public async Task<HttpResponseMessage> DeletePlan(string id)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // удалить факт выполнения практической работы (преподаватель, администратор)
        [HttpDelete]
        [Route("api/PracticeWork/exec/{id}")]
        public async Task<HttpResponseMessage> DeleteExec(string id)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // DELETE: api/PracticeWork/5
        [HttpDelete]
        [Route("api/PracticeWork/{id}")]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // удалить файл из практической работы (автор, администратор)
        [HttpDelete]
        [Route("api/PracticeWork/file")]
        public async Task<HttpResponseMessage> DeleteFile(string id)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}
