using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElJournal.Models
{
    public class SubjectModels
    {
        public string ID { get; set; }
        public string DepartmentID { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
    }

    public class SubjectGroupSemesterModels
    {
        public string ID { get; set; }
        public string GroupSemesterID { get; set; }
        public string SubjectID { get; set; }
        public string TeacherPersonID { get; set; }
    }
}