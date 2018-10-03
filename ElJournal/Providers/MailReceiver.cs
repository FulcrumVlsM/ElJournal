using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace ElJournal.Providers
{
    public class MailReceiver
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string SmtpAdress { get; set; }
        public int Port { get; set; }
        public string EmailPassword { get; set; }
        public bool EnableSsl { get; set; }

        public async Task<bool> Receive()
        {
            try
            {
                MailAddress from = new MailAddress(From);
                MailAddress to = new MailAddress(To);
                MailMessage m = new MailMessage(from, to)
                {
                    Subject = Title,
                    Body = Body
                };
                SmtpClient smtp = new SmtpClient(SmtpAdress, Port)
                {
                    Credentials = new NetworkCredential(From, EmailPassword),
                    EnableSsl = EnableSsl
                };
                await smtp.SendMailAsync(m);
                return true;
            }
            catch(Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e.ToString());
                return false;
            }
        }
    }
}