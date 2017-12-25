using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElJournal.Models
{
    public class Faculty
    {
        public string authorId { get; set; }
        public string name { get; set; }
        public string dekanId { get; set; }
        public string description { get; set; }
    }

    public class Person
    {
        public string name { get; set; }
        public string student_id { get; set; } = default(string);
        public string passport_id { get; set; } = default(string);
        public string avn_login { get; set; } = default(string);
        public string info { get; set; } = default(string);
        public string RolesId { get; set; }
    }

    /*public class Group
    {
        public string ID { get; set; }
        public string FacultyId { get; set; }
        public string name { get; set; }
        public string info { get; set; }
        public string curatorPersonId { get; set; }
    }*/
}