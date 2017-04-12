using SendGridMail;
using SendGridMail.Transport;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace OSBIDE.Web.Models
{
    public class Email
    {
        public static void Send(string subject, string message, ICollection<MailAddress> to)
        {
#if !DEBUG
            //ignore empty sends
            if (to.Count == 0)
            {
                return;
            }

            SmtpClient mailClient = new SmtpClient();

            MailAddress fromAddress = new MailAddress(ConfigurationManager.AppSettings["OsbideFromEmail"], "OSBIDE");
            var credentials = new NetworkCredential(ConfigurationManager.AppSettings["EmailUser"], ConfigurationManager.AppSettings["EmailPassword"]);
            //var transportSMTP = SMTP.GetInstance(credentials); 
            foreach (MailAddress recipient in to)
            {
                MailMessage mm = new MailMessage();

                mm.From = fromAddress;
                mm.To.Add(recipient);
                mm.Subject = subject;
                mm.Body = message;
                mm.IsBodyHtml = true;
                mailClient.Send(mm);
                /*
                SendGrid gridMessage = SendGrid.GetInstance(
                    fromAddress, 
                    new MailAddress[] { recipient }, 
                    new MailAddress[0], 
                    new MailAddress[0], 
                    subject, 
                    message, 
                    message
                    );
                

                //bomb's away!
                transportSMTP.Deliver(gridMessage);
                 * */
            }

            mailClient.Dispose();
#endif
        }
    }
}