using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EmailForwarder_2
{
    public class EmailMessage
    {
        public string MailId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string From { get; set; }
        public List<Attachment> Attachments { get; set; }
    }
}
