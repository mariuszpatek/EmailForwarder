using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailForwarder_2
{
    public class Pop3Configuration
    {
        public string ServerName { get; set; }
        public int ServerPort { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

    }
}
