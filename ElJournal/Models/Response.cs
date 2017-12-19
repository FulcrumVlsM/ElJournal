using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElJournal.Models
{
    public class Response
    {
        public bool Succesful { get; set; }
        public string Error { get; set; }
        public string message { get; set; }
        public dynamic Data { get; set; }
    }

    public static class ErrorMessage
    {
        public const string PERMISSION_ERROR = "You don't have permission for this operation";
        public const string INCORRECT_REQUEST_DATA = "Incorrect request data";
    }
}