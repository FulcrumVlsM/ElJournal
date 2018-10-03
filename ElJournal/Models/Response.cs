using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElJournal.Models
{
    /// <summary>
    /// Класс представляющий шаблон для ответа клиенту
    /// </summary>
    public class Response
    {
        public bool Succesful { get; set; } = default(bool);
        public string Error { get; set; } = null;
        public string message { get; set; } = null;
        public dynamic Data { get; set; } = null;
    }

    public class RespPerson:Response
    {
        public DateTime NextRequestTo { get; set; } = default(DateTime);
    }


    /// <summary>
    /// класс, содержащий тексты различных ошибок для клиента
    /// </summary>
    public static class ErrorMessage
    {
        public const string PERMISSION_ERROR = "You don't have permission for this operation";
        public const string INCORRECT_REQUEST_DATA = "Incorrect request data";
        public const string WAIT_YOUR_TIME = "the next request will be available in {0} minutes";
        public const string UNKNOWN_ERROR = "Unknown error";
    }
}