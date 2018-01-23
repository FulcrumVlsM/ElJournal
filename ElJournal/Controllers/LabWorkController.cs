using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ElJournal.Models;
using ElJournal.DBInteract;

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
            Response response = new Response();
            string token = Request?.Headers?.Authorization?.Scheme; //token пользователя
            string regexPart = name != null ? string.Format(" where name like '%{0}%'", name) : string.Empty;
            string sqlQuery = string.Format("select ID,name,advanced from LabWorks{0}", regexPart);

            try
            {
                DB db = DB.GetInstance();

                bool right = await db.CheckPermission(token, Permission.LBWRK_COMMON_PERMISSION);
                if (right)
                {
                    response.Data = await db.ExecSelectQuery(sqlQuery);
                    response.Succesful = true;
                }
                else
                    response.message = ErrorMessage.PERMISSION_ERROR;
            }
            catch(Exception e)
            {
                response.message = e.Message;
                response.Error = e.ToString();
                //TODO: add log
            }

            return response;
        }


        //возвращает список лабораторных по указанному предмету
        [HttpGet]
        [Route("api/LabWork/plan/{subjectId}")]
        public async Task<dynamic> GetPlan(string subjectId)
        {
            Response response = new Response();
            var parameters = new Dictionary<string, string>();
            string sqlQuery = "select * from dbo.PlannedLabWorks(@subjectId)";

            try
            {
                DB db = DB.GetInstance();

                parameters.Add("@subjectId", subjectId);
                response.Data = await db.ExecSelectQuery(sqlQuery, parameters);
                response.Succesful = true;
            }
            catch(Exception e)
            {
                response.message = e.Message;
                response.Error = e.ToString();
                //TODO: add log
            }

            return response;
        }


        //возвращает список лабораторных, выполненных студентом по предмету
        [HttpGet]
        [Route("api/LabWork/exec/{studentId}/{subjectId}")]
        public async Task<dynamic> GetExec(string studentId, string subjectId)
        {
            Response response = new Response();
            var parameters = new Dictionary<string, string>();
            string sqlQuery = "select * from dbo.ExecutedLabWorks(@studentId,@subjectId)";

            try
            {
                DB db = DB.GetInstance();

                parameters.Add("@studentId", studentId);
                parameters.Add("@subjectId", subjectId);
                response.Data = await db.ExecSelectQuery(sqlQuery, parameters);
                response.Succesful = true;
            }
            catch (Exception e)
            {
                response.message = e.Message;
                response.Error = e.ToString();
                //TODO: add log
            }

            return response;
        }


        // GET: api/LabWork/5
        // возвращает конкретную лабораторную работу (параметр file указывает нужно ли содержимое файла)
        //TODO: еще не работает отправка файлов
        public async Task<dynamic> Get(string id, [FromUri]bool file=false)
        {
            Response response = new Response();
            string token = Request?.Headers?.Authorization?.Scheme;
            var parameters = new Dictionary<string, string>();
            string sqlQuery = "select name,advanced from LabWorks where ID=@id";

            try
            {
                DB db = DB.GetInstance();

                bool right = await db.CheckPermission(token, Permission.LBWRK_READ_PERMISSION);
                if (right)
                {
                    parameters.Add("@id", id);
                    response.Data = (await db.ExecSelectQuery(sqlQuery, parameters))[0];
                    response.Succesful = true;
                }
                else
                    response.message = ErrorMessage.PERMISSION_ERROR;
            }
            catch(Exception e)
            {
                response.message = e.Message;
                response.Error = e.ToString();
                //TODO: add log
            }

            return response;
        }


        // POST: api/LabWork/plan/5/5
        // добавление лабораторной работы в план по предмету
        [HttpPost]
        [Route("api/LabWork/plan/{subjectGroupId}/{workId}")]
        public async Task<dynamic> PostPlan(string subjectGroupId, string workId)
        {
            Response response = new Response();
            string token = Request?.Headers?.Authorization?.Scheme;
            var parameters = new Dictionary<string, string>();
            string sqlQuery1 = "insert into LabWorksPlan(SubjectGroupSemesterID,LabWorkID) " +
                "values (@subjectId,@workId)";
            string sqlQuery2 = "select * from dbo.CheckTeach(@userToken,@subjectId)";

            try
            {
                DB db = DB.GetInstance();

                parameters.Add("@userToken", token);
                parameters.Add("@subjectId", subjectGroupId);
                parameters.Add("@workId", workId);

                bool commonRight = await db.CheckPermission(token, Permission.LBWRK_COMMON_PERMISSION);
                bool subjectRight = await db.CheckPermission(token, Permission.LBWRK_PERMISSION) ?
                    await db.ExecuteScalarQuery(sqlQuery2, parameters) : false;

                if (commonRight || subjectRight)
                {
                    int result = db.ExecInsOrDelQuery(sqlQuery1, parameters);
                    if (result == 1)
                        response.Succesful = true;
                    else
                    {
                        //TODO: add log
                        response.message = "Wasn't saved.";
                    }
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch(Exception e)
            {
                //TODO: add log
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }


        // POST: api/LabWork/exec/5/5?state=true
        // отметка о выполнении лаб. работы (параметр state)
        [HttpPost]
        [Route("api/LabWork/exec/{studentId}/{subjWorkId}")]
        public async Task<dynamic> PostExec([FromBody]LabWork lab, string studentId, string subjWorkId, 
            [FromUri]bool state=true)
        {
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
