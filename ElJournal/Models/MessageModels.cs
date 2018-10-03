using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElJournal.Models
{
    public class MessageModels
    {
        public string ID { get; set; }
        public string Receiver { get; set; }
        public string Sender { get; set; }
        public string Body { get; set; }
    }
}