using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailForwarder_2
{
    public class EmailMessageRepositrory
    {
        public string[] GetEmailsMessageIds()
        {
            using (var db = new EmailForwarderDbContext())
            {
                return db.MailMessages
                    .AsNoTracking()
                    .Select(m => m.MailId)
                    .ToArray();
            }
        }
    }
}
