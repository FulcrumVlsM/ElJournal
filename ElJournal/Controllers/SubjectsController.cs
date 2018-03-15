using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Providers;
using ElJournal.Models;

namespace ElJournal.Controllers
{
    public class SubjectsController : ApiController
    {
        //получить общий список предметов (и по имени) (все)
        //получить конкретный предмет по id (все)
        //получить список предметов группы (все)
        //получить список предметов, которые ведет преподаватель (все рег. пользователи)
        //получить игнор-список для данного предмета (администратор, администратор кафедры, преподаватель)
        //добавить новый предмет (администратор, администратор кафедры)
        //установить предмет для группы в указанном семестре (администратор, администратор кафедры)
        //установить игнорирование студента для данного предмета (администратор, администратор кафедры, преподаватель)
        //изменить информацию о предмете (администратор, администратор кафедры)
        //изменить преподавателя по предмету для группы в указанном семестре (администратор, администратор кафедры)
        //удалить предмет из плана для группы в указанном семестре (администратор, администратор кафедры)
        //удалить предмет (администратор)
        //удалить игнорирование студента для данного предмета (администратор, администратор кафедры, преподаватель)


        // GET: api/Subjects
        // возвращает общий список предметов
        public async Task<dynamic> Get([FromUri]string name = null)
        {
            return null;
        }

        // GET: api/Subjects/5
        // возвращает конкретный предмет
        [HttpGet]
        [Route("api/Subjects/{id}")]
        public async Task<dynamic> GetConcrete(string id)
        {
            return null;
        }

        // GET: api/Subjects/ByGroup/5
        // возвращает предметы для указанной группы (группа-семестр)
        [HttpGet]
        [Route("api/Subjects/ByGroup/{groupId}")]
        public async Task<dynamic> GetGroup(string groupId)
        {
            return null;
        }


        // GET: api/Subjects/ByTeacher/5
        // возвращает список предметов, которые ведет указанный преподаватель
        [HttpGet]
        [Route("api/Subjects/ByTeacher/{personId}")]
        public async Task<dynamic> GetTeacher(string personId)
        {
            return null;
        }


        // GET: api/Subjects/ignore/5
        // возвращает игнор-список студентов для данного предмета
        [HttpGet]
        [Route("api/Subjects/ignore/{subjectId}")]
        public async Task<dynamic> GetIgnore(string subjectId)
        {
            return null;
        }


        // POST: api/Subjects
        // добавление нового предмета
        public async Task<dynamic> Post([FromBody]SubjectModels subject)
        {
            return null;
        }


        // POST: api/Subjects
        // установка предмета в план для группы (группа-семестр)
        [HttpPost]
        [Route("api/Subjects/plan")]
        public async Task<dynamic> PostPlan([FromBody]SubjectGroupSemesterModels subjectGroupSemester)
        {
            return null;
        }


        // POST: api/Subjects/ignore
        // добавление игнорирования студента для данного предмета
        [HttpPost]
        [Route("api/Subjects/ignore/{subjectId}/{studentId}")]
        public async Task<dynamic> PostIgnore(string subjectId, string studentId)
        {
            return null;
        }


        // PUT: api/Subjects/5
        // изменить информацию о предмете (название, общая информация, кафедра)
        public async Task<dynamic> Put(string id, [FromBody]SubjectModels subject)
        {
            return null;
        }


        // PUT: api/Subjects/5
        // изменить предмет в плане
        [HttpPut]
        [Route("api/Subjects/plan/{id}")]
        public async Task<dynamic> Put(string id, [FromBody]SubjectGroupSemesterModels subjectGroup)
        {
            return null;
        }


        // DELETE: api/Subjects/plan/5
        // удалить предмет из плана
        [HttpDelete]
        [Route("api/Subjects/plan/{id}")]
        public dynamic DeletePlan(string id)
        {
            return null;
        }


        // DELETE: api/Subjects/5
        // удалить предмет
        public dynamic Delete(int id)
        {
            return null;
        }


        // DELETE: api/Subjects/Ignore/5/5
        [HttpDelete]
        [Route("api/Subjects/ignore/{subjectId}/{studentId}")]
        public dynamic DeleteIgnor(string subjectId, string studentId)
        {
            return null;
        }
    }
}
