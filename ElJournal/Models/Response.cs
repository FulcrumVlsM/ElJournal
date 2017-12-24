﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElJournal.Models
{
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

    public static class ErrorMessage
    {
        public const string PERMISSION_ERROR = "You don't have permission for this operation";
        public const string INCORRECT_REQUEST_DATA = "Incorrect request data";
        public const string WAIT_YOUR_TIME = "the next request will be available in 10 minutes";
    }
}