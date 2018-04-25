using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailForwarder_2
{
    public class EmailMessageTable
    {
        public int EmailMessageTableId { get; set; }
        public string MailId { get; set; }
        public DateTime DateStamp { get; set; }
    }
}
