using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElJournal.Models
{
    public class AlertModels
    {
        public string ID { get; set; }
        public string SubjectGroupSemesterID { get; set; }
        public string RoomID { get; set; }
        public string FacultyID { get; set; }
        public string DepartmentID { get; set; }
        public DateTime createDate { get; set; }
        public DateTime eventDate { get; set; }
        public string EventTypeID { get; set; }
        public string title { get; set; }
        public string info { get; set; }
        public string authorID { get; set; }
    }
}