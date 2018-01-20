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
    //develop: Mikhail
    public class LabWorkController : ApiController
    {
        /* Лабораторные работы связываются с предметом (SubjectGroupSemester).
         * Для связи лабораторной работы с преметом используются методы с роутингом /plan.
         * Студент связывается с лаб-предмет.
         * Для установки отметки сдал/не_сдал используются методы с роутингом /execute.
         */
        

        // GET: api/LabWork
        //TODO: возвращает полный список лабораторных работ (без самих файлов)
        public async Task<dynamic> Get([FromUri]string name)
        {
            return null;
        }


        //возаращает список лабораторных по указанному предмету
        [HttpGet]
        [Route("api/LabWork/plan/{subjectId}")]
        public async Task<dynamic> GetPlan(string subjectId)
        {
            return null;
        }


        //возвращает список лабораторных, выполненных студентом по предмету
        [HttpGet]
        [Route("api/LabWork/exec/{studentId}/{subjectId}")]
        public async Task<dynamic> GetExec(string studentId, string subjectId)
        {
            return null;
        }


        // GET: api/LabWork/5
        // возвращает конкретную лабораторную работу (параметр file указывает нужно ли содержимое файла)
        public async Task<dynamic> Get(string id, [FromUri]bool file=false)
        {
            return null;
        }


        // POST: api/LabWork/plan/5/5
        // добавление лабораторной работы в план по предмету
        [HttpPost]
        [Route("api/LabWork/plan/{subjectSemesterId}/{workId}")]
        public async Task<dynamic> PostPlan(string subjectSemesterId, string workId)
        {
            return null;
        }


        // POST: api/LabWork/exec/5/5?state=true
        // отметка о выполнении лаб. работы (параметр state)
        [HttpPost]
        [Route("api/LabWork/exec/{studentId}/{subjWorkId}")]
        public async Task<dynamic> PostExec([FromBody]string lab, string studentId, string subjWorkId, [FromUri]bool state=true)
        {
            //если state=false, запись удаляется из бд.
            return null;
        }


        // POST: api/LabWork
        // добавление лабораторной работы
        public async Task<dynamic> Post([FromBody]LabWork lab)
        {
            return null;
        }


        // PUT: api/LabWork/5
        // изменение лабораторной работы
        public async Task<dynamic> Put(string id, [FromBody]LabWork lab)
        {
            return null;
        }


        // DELETE: api/LabWork/plan/5
        // удаление лабораторной работы из плана
        [HttpDelete]
        [Route("api/LabWork/plan/{id}")]
        public async Task<dynamic> DeletePlan(string id)
        {
            return null;
        }


        // DELETE: api/LabWork/5
        // удаление лабораторной работы
        public async Task<dynamic> Delete(string id)
        {
            return null;
        }
    }
}
