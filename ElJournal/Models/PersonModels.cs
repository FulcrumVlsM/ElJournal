using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElJournal.Models
{
    public class Person
    {
        public string ID { get; set; }
        public string name { get; set; }
        public string student_id { get; set; }
        public string passport_id { get; set; }
        public string avn_login { get; set; }
        public string info { get; set; }
        public string RolesId { get; set; }
        public string DepartmentId { get; set; }
        public string FacultyId { get; set; }
    }
}