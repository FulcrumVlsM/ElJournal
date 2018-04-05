using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElJournal.Models
{
    public class CourseWorkModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Advanced { get; set; }
        public string FileURL { get; set; }
    }

    public class CourseWorkStageModels
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
    }

    public class CourseWorkExecutionModels
    {
        public string Info { get; set; }
        public DateTime Date { get; set; }
    }
}