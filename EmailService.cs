using Microsoft.Azure.WebJobs.Host;
using OpenPop.Mime;
using OpenPop.Pop3;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmailForwarder_2
{
    public class EmailService
    {
        private static readonly string _smtpName = ConfigurationManager.AppSettings["SmtpName"];
        private static readonly string _sourceEmailAddress = ConfigurationManager.AppSettings["SmtpEmailAddress"];
        private static readonly string _smtpUserName = ConfigurationManager.AppSettings["SmtpUserName"];
        private static readonly string _smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
        private static readonly int _smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);

        private readonly TraceWriter _log;
        public EmailService(TraceWriter log)
        {
            _log = log;
        }

        public async Task<bool> SendEmail(string toEmailAddress, string subject, string body, List<Attachment> attachments)
        {
            if (string.IsNullOrEmpty(toEmailAddress) || string.IsNullOrEmpty(subject))
                return false;
                
            var mail = CreateMailMessage(toEmailAddress, subject, body, attachments);

            using (var smtpClient = new SmtpClient(_smtpName, _smtpPort))
            {
                try
                {
                    smtpClient.Timeout = 5000;
                    smtpClient.Credentials = new NetworkCredential(_smtpUserName, _smtpPassword);
             //       smtpClient.EnableSsl = true;
                    await smtpClient.SendMailAsync(mail);
                }
                catch (Exception ex)
                {
                    _log.Error($"Something went wrong during sending email: {ex.Message}");
                    return false;
                }
            }

            _log.Info($"[{Thread.CurrentThread.ManagedThreadId}] Email was sent successfuly: {mail.Subject}");
            return true;
        }

        public IEnumerable<EmailMessage> GetEmails(Pop3Configuration configuration, Func<string, bool> isValid)
        {

            using (var pop3Client = new Pop3Client())
            {
                pop3Client.Connect(configuration.ServerName, configuration.ServerPort, true);
                pop3Client.Authenticate(configuration.Login, configuration.Password);

                int messageCount = pop3Client.GetMessageCount();
                var allMessages = new List<Message>(messageCount);

                var emailMessages = new List<EmailMessage>();

                for (int i = messageCount; i > 0; i--)
                {
                    var messageUid = pop3Client.GetMessageUid(i);
                    
                    if (isValid(messageUid))
                    {                        
                        var message = pop3Client.GetMessage(i);
                        List<Attachment> attachments = GetAttachments(message);
                        
                        emailMessages.Add(
                        new EmailMessage
                        {
                            MailId = messageUid,
                            Body = FormatMessageBody(message),
                            Subject = message.Headers.Subject,
                            From = message.Headers.From?.Address,
                            Attachments = attachments
                        });
                    }
                }

                return emailMessages;
            }
        }

        private List<Attachment> GetAttachments(Message message)
        {
            List<Attachment> attachments = new List<Attachment>();
            string directoryPath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;

            int numberOfAttachment = 0;
            foreach (var attachment in message.FindAllAttachments())
            {
                var fileInfo = new FileInfo(Path.Combine(directoryPath, attachment.FileName + "_" + numberOfAttachment.ToString()));
                numberOfAttachment++;
                try
                {
                    attachment.Save(fileInfo);
                    attachments.Add(new Attachment(fileInfo.FullName));
                }
                catch (Exception ex)
                {
                    _log.Error($"Can't saved the attachment: {attachment.FileName}. {ex}");
                }
                
            }

            return attachments;
        }

        private string FormatMessageBody(Message message)
        {
            StringBuilder builder = new StringBuilder();
            MessagePart html = message.FindFirstHtmlVersion();
            if (html != null)
            {
                builder.Append(html.GetBodyAsText());
            }
            else
            {
                MessagePart plainText = message.FindFirstPlainTextVersion();
                if (plainText != null)
                {
                    builder.Append(plainText.GetBodyAsText());
                }
            }

            return builder?.ToString();
        }

        private MailMessage CreateMailMessage(string toEmailAddress, string subject, string body, List<Attachment> attachments)
        {
            var mail = new MailMessage
            {
                IsBodyHtml = true,
                From = new MailAddress(_sourceEmailAddress)
            };
            mail.To.Add(toEmailAddress);
            mail.Subject = subject;           
            mail.Body = body ?? "";
            foreach (var attachment in attachments)
                mail.Attachments.Add(attachment);

            return mail;
        }
    }
}
