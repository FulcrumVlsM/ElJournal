using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElJournal.Models
{
    public class Department
    {
        public string authorId { get; set; }
        public string name { get; set; }
        public string managerPersonID { get; set; }
        public string description { get; set; }
    }
}