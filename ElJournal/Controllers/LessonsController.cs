using ElJournal.DBInteract;
using ElJournal.Models;
using ElJournal.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ElJournal.Controllers
{
    public class LessonsController : ApiController
    {
        // получить все проведенные занятия по предмету (все)
        // получить план занятий по предмету (количество требуемых занятий) (все)
        // получить типы предметов (все)
        // получить время занятий (все)
        // получить посещенные занятия студентом по предмету (все рег. пользователи)
        // установить план по предмету (администратор кафедры, администратор)
        // добавить проведеннное занятие (преподаватель, администратор)
        // отметить студента на занятии (преподаватель, администратор)
        // удалить проведенное занятия (преподаватель, администратор)
        // удалить студента с занятия (преподаватель, администратор)


        // GET: api/Lessons
        // получить все проведенные занятия по предмету (все)
        [HttpGet]
        [Route("api/Lesson/subject/{flowSubjectId}")]
        public async Task<HttpResponseMessage> GetToSubj(string flowSubjectId)
        {
            Response response = new Response();

            //поиск предмета
            FlowSubject subject = await FlowSubject.GetInstanceAsync(flowSubjectId);
            if (subject == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var lessons = await Lesson.GetCollectionAsync(subject.ID);
            response.Data = lessons;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // получить план занятий по предмету (количество требуемых занятий) (все)
        [HttpGet]
        [Route("api/Lesson/plan/{flowSubjectId}")]
        public async Task<HttpResponseMessage> GetPlan(string flowSubjectId)
        {
            Response response = new Response();

            //поиск предмета
            FlowSubject subject = await FlowSubject.GetInstanceAsync(flowSubjectId);
            if (subject == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var plan = await LessonPlan.GetCollectionAsync(subject.ID);
            response.Data = plan;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // получить типы предметов (все)
        [HttpGet]
        [Route("api/Lesson/types")]
        public async Task<HttpResponseMessage> GetTypes()
        {
            Response response = new Response();
            var types = await LessonType.GetCollectionAsync();
            response.Data = types;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // получить время занятий (все)
        [HttpGet]
        [Route("api/Lesson/times")]
        public async Task<HttpResponseMessage> GetTimes()
        {
            Response response = new Response();
            var times = await LessonTime.GetCollectionAsync();
            response.Data = times;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // получить посещенные занятия студентом по предмету (все рег. пользователи)
        [HttpGet]
        [Route("api/Lesson/attend/{flowSubjectId}/{studentId}")]
        public async Task<HttpResponseMessage> GetPresence(string flowSubjectId, string studentId)
        {
            Response response = new Response();
            var lessons = await Lesson.GetCollectionAsync(flowSubjectId, studentId);
            response.Data = lessons;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        // POST: api/Lessons
        // добавить проведеннное занятие (преподаватель, администратор)
        [HttpPost]
        [Route("api/Lesson")]
        public async Task<HttpResponseMessage> Post([FromBody]Lesson lesson)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            bool commonRight = default(bool),
                    teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.TEACHER_COMMON_PERMISSION),
                () => teacherRight = authProvider.CheckPermission(Permission.TEACHER_PERMISSION) ?
                                     authProvider.FlowsSubjects.Contains(lesson?.FlowSubjectId) : false);

            if(commonRight || teacherRight)
            {
                if (await lesson.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // отметить студента на занятии (преподаватель, администратор)
        [HttpPost]
        [Route("api/Lesson/attend/{lessonId}/{studentId}")]
        public async Task<HttpResponseMessage> PostAttend(string lessonId, string studentId)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск занятия и студента
            LessonAttend lesson = (LessonAttend)(await Lesson.GetInstanceAsync(lessonId));
            StudentFlowSubject student = await StudentFlowSubject.GetInstanceAsync(studentId);
            if (lesson == null || student == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав у пользователя
            bool commonRight = default(bool),
                    teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.TEACHER_COMMON_PERMISSION),
                () => teacherRight = authProvider.CheckPermission(Permission.TEACHER_PERMISSION) ?
                                     authProvider.FlowsSubjects.Contains(lesson?.FlowSubjectId) : false);

            if (commonRight || teacherRight)
            {
                lesson.StudentId = student.ID;
                if (await lesson.Push())
                    return Request.CreateResponse(HttpStatusCode.Created);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // DELETE: api/Lessons/5
        // удалить проведенное занятие (преподаватель, администратор)
        [HttpPost]
        [Route("api/Lesson/{id}")]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск занятия
            Lesson lesson = await Lesson.GetInstanceAsync(id);
            if (lesson == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав у пользователя
            bool commonRight = default(bool),
                    teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.TEACHER_COMMON_PERMISSION),
                () => teacherRight = authProvider.CheckPermission(Permission.TEACHER_PERMISSION) ?
                                     authProvider.FlowsSubjects.Contains(lesson?.FlowSubjectId) : false);

            if (commonRight || teacherRight)
            {
                if (lesson.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }


        // удалить студента с занятия (преподаватель, администратор)
        [HttpDelete]
        [Route("api/Lesson/attend/{lessonId}/{studentId}")]
        public async Task<HttpResponseMessage> DeleteAttend(string lessonId, string studentId)
        {
            //идентификация пользователя
            string token = Request?.Headers?.Authorization?.Scheme;
            NativeAuthProvider authProvider = await NativeAuthProvider.GetInstance(token);
            if (authProvider == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            //поиск занятия и студента
            LessonAttend lesson = (LessonAttend)(await Lesson.GetInstanceAsync(lessonId));
            StudentFlowSubject student = await StudentFlowSubject.GetInstanceAsync(studentId);
            if (lesson == null || student == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            //проверка наличия прав у пользователя
            bool commonRight = default(bool),
                    teacherRight = default(bool);
            Parallel.Invoke(() => commonRight = authProvider.CheckPermission(Permission.TEACHER_COMMON_PERMISSION),
                () => teacherRight = authProvider.CheckPermission(Permission.TEACHER_PERMISSION) ?
                                     authProvider.FlowsSubjects.Contains(lesson?.FlowSubjectId) : false);

            if (commonRight || teacherRight)
            {
                lesson.StudentId = student.ID;
                if (lesson.Delete())
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}
