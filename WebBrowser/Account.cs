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
        public string deal { get; set; }
        public string nickName { get; set; }
        public int Level { get; set; }
        public int baoLiaoCount { get; set; }

        public string arguments { get; set; }
        public string mode { get; set; }
        public int order { get; set; }
        public int waitTime { get; set; }
        public int descriptionMode { get; set; }
        public int limit { get; set; }
        public string customDespPrefix { get; set; }
        public string output { get; set; }
        public double discountRate { get; set; }
        public string category { get; set; }
        public string pages { get; set; }
        public Account(string phone, string email, string pass)
        {
            this.phone = phone;
            password = pass;
            this.email = email;
            waitTime = 5;
            order = 2;
            limit = 10;
            descriptionMode = 1;
            output = "";
        }

    }
}
