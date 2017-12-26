using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElJournal.Models
{
    public class Semester
    {
        public string ID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}