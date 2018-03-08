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
        public async Task<dynamic> PostExec(string studentId, string subjWorkId, 
            [FromUri]bool state=true)
        {
            Response response = new Response();
            string token = Request?.Headers?.Authorization?.Scheme;
            var parameters = new Dictionary<string, string>();
            string sqlQuery1 = "select * from dbo.CheckStudentLabWorkPlan(@studentId,@planId)";
            string sqlQuery2 = "select SubjectGroupSemesterID from LabWorksPlan where ID=@planId";
            string sqlQuery3 = "select * from dbo.CheckTeach(@token,@subjectId)";
            string sqlQuery4 = "insert into LabWorksExecution(LabWorkPlanID,StudentGroupSemesterID)" +
                " values(@planId,@studentId)";
            string sqlQuery5 = "delete from LabWorksExecution where LabWorkPlanID=@planId " +
                "and StudentGroupSemesterID=@studentId";

            parameters.Add("@studentId", studentId);
            parameters.Add("@planId", subjWorkId);
            parameters.Add("@token", token);

            try
            {
                DB db = DB.GetInstance();
                bool commonRight=default(bool), teacherRight=default(bool), trueData=default(bool);

                Task[] tasks = new Task[3]
                {
                    new Task(async () => commonRight=await db.CheckPermission(token,Permission.LBWRK_COMMON_PERMISSION)),
                    new Task(async () => trueData = await db.ExecuteScalarQuery(sqlQuery1,parameters)),
                    new Task(async () =>
                    {
                        parameters.Add("@subjectId",await db.ExecuteScalarQuery(sqlQuery2,parameters));
                        teacherRight = await db.CheckPermission(token,Permission.LBWRK_PERMISSION) ?
                             await db.ExecuteScalarQuery(sqlQuery3,parameters) : false;
                    })
                };
                foreach (var task in tasks)
                    task.Start();
                Task.WaitAll(tasks);

                if(trueData && (commonRight || teacherRight))
                {
                    if (state)
                    {
                        int result = db.ExecInsOrDelQuery(sqlQuery4, parameters);
                        if (result == 1)
                            response.Succesful = true;
                        else
                        {
                            //TODO: add log
                            response.message = "Unknown error. Data could not be saved. Save the original query data to " +
                                "the log and send it to the developers";
                        }
                    }
                    else
                    {
                        int result = db.ExecInsOrDelQuery(sqlQuery5, parameters);
                        if (result == 1)
                            response.Succesful = true;
                        else
                        {
                            //TODO: add log
                            response.message = "Unknown error. Data could be lost. Save the original query data to " +
                                "the log and send it to the developers";
                        }
                    }
                }
                else
                {
                    if (!trueData)
                        response.Error = ErrorMessage.INCORRECT_REQUEST_DATA;
                    else
                        response.Error = ErrorMessage.PERMISSION_ERROR;
                }
            }
            catch(Exception e)
            {
                //TODO: add log
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }


        // POST: api/LabWork
        // добавление лабораторной работы
        // TODO: еще нет работы с файлами
        public async Task<dynamic> Post([FromBody]LabWork lab)
        {
            Response response = new Response();
            string token = Request?.Headers?.Authorization?.Scheme;
            var parameters = new Dictionary<string, string>();
            string sqlQuery = "dbo.AddLabWork";

            try
            {
                DB db = DB.GetInstance();

                bool commonRight = await db.CheckPermission(token, Permission.LBWRK_COMMON_PERMISSION);
                bool teacherRight = await db.CheckPermission(token, Permission.LBWRK_PERMISSION);
                if (commonRight || teacherRight)
                {
                    parameters.Add("@name", lab.Name);
                    parameters.Add("@advanced", lab.Advanced);

                    string result = db.ExecStoredProcedure(sqlQuery, parameters);
                    if (!string.IsNullOrEmpty(result))
                    {
                        response.Data = new { ID = result };
                        response.Succesful = true;
                    }
                    else
                    {
                        response.Error = ErrorMessage.INCORRECT_REQUEST_DATA;
                        //TODO: add log
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


        // PUT: api/LabWork/5
        // изменение лабораторной работы
        public async Task<dynamic> Put(string id, [FromBody]LabWork lab)
        {
            Response response = new Response();
            string token = Request?.Headers?.Authorization?.Scheme;
            var parameters = new Dictionary<string, string>();
            string sqlQuery = "dbo.UpdateLabWork";

            try
            {
                DB db = DB.GetInstance();

                parameters.Add("@ID", id);
                parameters.Add("@name", lab.Name);
                parameters.Add("@advanced", lab.Advanced);

                bool right = await db.CheckPermission(token, Permission.LBWRK_COMMON_PERMISSION) ||
                    await db.CheckPermission(token, Permission.LBWRK_PERMISSION);
                if (right)
                {
                    int result = db.ExecStoredProcedure(sqlQuery, parameters);
                    if (result == 0)
                        response.Succesful = true;
                    else
                    {
                        //TODO: add log
                        response.message = "Unknown error. Send logs to developers.";
                        response.Error = ErrorMessage.UNKNOWN_ERROR;
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


        // DELETE: api/LabWork/plan/5
        // удаление лабораторной работы из плана
        [HttpDelete]
        [Route("api/LabWork/plan/{id}")]
        public async Task<dynamic> DeletePlan(string id)
        {
            Response response = new Response();
            string token = Request?.Headers?.Authorization?.Scheme;
            var parameters = new Dictionary<string, string>();
            string sqlQuery1 = "delete from LabWorksPlan where ID=@ID";
            string sqlQuery2 = "select SubjectGroupSemesterID from LabWorksPlan where ID=@ID";
            string sqlQuery3 = "select * from dbo.CheckTeach(@token,@subjectId)";

            parameters.Add("@ID", id);
            parameters.Add("@token", token);

            try
            {
                DB db = DB.GetInstance();

                bool commonRight = default(bool), teacherRight = default(bool);
                Task[] tasks = new Task[2]
                {
                    new Task(async () => commonRight = await db.CheckPermission(token, Permission.LBWRK_COMMON_PERMISSION)),
                    new Task(async () =>
                    {
                        parameters.Add("@subjectId", await db.ExecuteScalarQuery(sqlQuery2, parameters));
                        teacherRight = await db.CheckPermission(token, Permission.LBWRK_PERMISSION) ?
                             await db.ExecuteScalarQuery(sqlQuery3, parameters) : false;
                    })
                };
                foreach (var task in tasks)
                    task.Start();

                Task.WaitAll(tasks);
                if (commonRight || teacherRight)
                {
                    int result = db.ExecInsOrDelQuery(sqlQuery1, parameters);
                    if (result == 1)
                        response.Succesful = true;
                    else
                    {
                        //TODO: add log
                        response.message = "Unknown error. Send logs to developers.";
                        response.Error = ErrorMessage.UNKNOWN_ERROR;
                    }
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch (Exception e)
            {
                //TODO: add log
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }


        // DELETE: api/LabWork/5
        // удаление лабораторной работы
        public async Task<dynamic> Delete(string id)
        {
            Response response = new Response();
            string token = Request?.Headers?.Authorization?.Scheme;
            var parameters = new Dictionary<string, string>();
            string sqlQuery = "delete from LabWorks where ID=@ID";

            parameters.Add("@ID", id);

            try
            {
                DB db = DB.GetInstance();

                bool right = await db.CheckPermission(token, Permission.LBWRK_COMMON_PERMISSION);
                
                if (right)
                {
                    int result = db.ExecInsOrDelQuery(sqlQuery, parameters);
                    if (result == 1)
                        response.Succesful = true;
                    else
                    {
                        //TODO: add log
                        response.message = "Unknown error. Send logs to developers.";
                        response.Error = ErrorMessage.UNKNOWN_ERROR;
                    }
                }
                else
                    response.Error = ErrorMessage.PERMISSION_ERROR;
            }
            catch (Exception e)
            {
                //TODO: add log
                response.Error = e.ToString();
                response.message = e.Message;
            }

            return response;
        }
    }
}
