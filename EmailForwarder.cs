using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EmailForwarder_2
{
    public static class EmailForwarder
    {
        [FunctionName("EmailForwarder")]
        public static void Run([TimerTrigger("0 0 7 * * *", RunOnStartup = true) ]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger -> EmailForwarder function executed at: {DateTime.UtcNow}");

            var validator = new Validator();
            var service = new EmailService(log);
            var pop3Configuration = new Pop3Configuration
            {
                ServerName = ConfigurationManager.AppSettings["EmailServerName"],
                ServerPort = int.Parse(ConfigurationManager.AppSettings["EmailServerPort"]),
                Login = ConfigurationManager.AppSettings["EmailUserName"],
                Password = ConfigurationManager.AppSettings["EmailPassword"]
            };

            var emails = service.GetEmails(pop3Configuration, validator.IsEmailNotForwarded);

            var emailsToSave = new List<EmailMessageTable>();
            List<Task> sendingEmails = new List<Task>();

            foreach (var email in emails)
            {
                var task = Task.Run(async () =>
                {
                    if (await service.SendEmail(ConfigurationManager.AppSettings["EmailDeliveryAddress"], email.Subject, email.Body, email.Attachments))
                    {
                        emailsToSave.Add(new EmailMessageTable { MailId = email.MailId, DateStamp = DateTime.Now });
                    }
                    else
                    {
                        log.Error($"Unable to Send Email: {email.Subject}");
                    }
                });

                sendingEmails.Add(task);        
            }

            Task.WaitAll(sendingEmails.ToArray());

            if (emailsToSave.Count > 0)
            {
                using (var db = new EmailForwarderDbContext())
                {
                    db.MailMessages.AddRange(emailsToSave);
                    db.SaveChanges();
                }
            }
        }
    }
}
