using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmzdmBot
{
    public class Account
    {
        public string phone { get; }
        public string password { get; }
        public string email { get; }
        public string login { get; set; }
        public Account(string p, string e, string pass)
        {
            phone = p;
            password = pass;
            email = e;
        }

    }
}
