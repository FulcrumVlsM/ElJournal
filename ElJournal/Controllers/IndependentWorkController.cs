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
    public class IndependentWorkController : ApiController
    {
        // получить весь список самостоятельных работ (администратор, администратор кафедры)
        // получить самостоятельную работу по id (все)
        // получить план самостоятельных работ по предмету (все)
        // получить состояние выполнения студентом (все рег. пользователи)
        // добавить самостоятельную работу (преподаватель, администратор)
        // добавить самостоятельную работу в план (преподаватель, администратор)
        // установить самостоятельную работу студенту (преподаватель, администратор)
        // установить выполнение самостоятельной работы студентом (преподаватель, администратор)
        // изменить самостоятельную работу (автор, администратор)
        // удалить самостоятельную работу из плана (преподаватель, администратор)
        // удалить самостоятельную работу (администратор)


        // GET: api/IndependentWork
        // получить весь список самостоятельных работ (администратор, администратор кафедры)
        public async Task<dynamic> Get([FromUri]string name=null)
        {
            return null;
        }

        // GET: api/IndependentWork/5
        // получить самостоятельную работу по id (все)
        [HttpGet]
        [Route("api/IndependentWork/{id}")]
        public async Task<dynamic> GetConcrete(string id)
        {
            return null;
        }


        // GET: api/IndependentWork/Plan/5
        // получить план самостоятельных работ по предмету (все)
        [HttpGet]
        [Route("api/IndependentWork/Plan/{subjectId}")]
        public async Task<dynamic> GetPlan(string subjectId)
        {
            return null;
        }


        // GET: api/IndependentWork/Exec/5
        // получить состояние выполнения студентом (все рег. пользователи)
        [HttpGet]
        [Route("api/IndependentWork/Exec/{studentId}/{subjectId}")]
        public async Task<dynamic> GetExec(string studentId, string subjectId)
        {
            return null;
        }


        // POST: api/IndependentWork
        // добавить самостоятельную работу (преподаватель, администратор)
        public async Task<dynamic> Post([FromBody]IndependentWork independentWork)
        {
            return null;
        }


        // POST: api/IndependentWork/Plan/5/5
        // добавить самостоятельную работу в план (преподаватель, администратор)
        [HttpPost]
        [Route("api/IndependentWork/Plan/{subjectId}/{workId}")]
        public async Task<dynamic> PostPlan(string subjectId, string workId)
        {
            return null;
        }


        // POST: api/IndependentWork/Plan/5/5
        // установить самостоятельную работу студенту (преподаватель, администратор)
        [HttpPost]
        [Route("api/IndependentWork/Exec/{studentId}/{workPlanId}")]
        public async Task<dynamic> PostExec(string studentId, string workPlanId)
        {
            return null;
        }


        // POST: api/IndependentWork/Exec/5
        // установить выполнение самостоятельной работы студентом (преподаватель, администратор)
        [HttpPost]
        [Route("api/IndependentWork/Exec/{studentId}")]
        public async Task<dynamic> PostExec(string studentId, [FromBody]string Info, [FromUri]bool state = true)
        {
            return null;
        }


        // PUT: api/IndependentWork/5
        // изменить самостоятельную работу (автор, администратор)
        public async Task<dynamic> Put(string id, [FromBody]IndependentWork independentWork)
        {
            return null;
        }


        // DELETE: api/IndependentWork/Plan/5
        // удалить самостоятельную работу из плана (преподаватель, администратор)
        [HttpDelete]
        [Route("api/IndependentWork/Plan/{id}")]
        public dynamic DeletePlan(string id)
        {
            return null;
        }


        // DELETE: api/IndependentWork/5
        // удалить самостоятельную работу (администратор)
        public dynamic Delete(string id)
        {
            return null;
        }
    }
}
