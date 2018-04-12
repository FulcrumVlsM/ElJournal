using ElJournal.DBInteract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Models
{
    public class Student
    {
        public string personId { get; set; }
        public string groupId { get; set; }
        public string semesterId { get; set; }
    }
}