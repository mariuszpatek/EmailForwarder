using System.Configuration;
using System.Data.Entity;

namespace EmailForwarder_2
{
    public class EmailForwarderDbContext : DbContext
    {
        public EmailForwarderDbContext()
            : base(ConfigurationManager.ConnectionStrings["EmailForwarderConnectionString"].ConnectionString)
        {
        }
        public DbSet<EmailMessageTable> MailMessages { get; set; }

    }
}


