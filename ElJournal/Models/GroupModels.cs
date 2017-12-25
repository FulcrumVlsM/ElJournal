using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElJournal.Models
{
    public class Group
    {
        public string ID { get; set; }
        public string name { get; set; }
        public string info { get; set; }
        public string curatorId { get; set; }
        public string facultyId { get; set; }
    }
}