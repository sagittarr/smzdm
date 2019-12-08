using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmzdmBot
{
    public class Account
    {
        public static Dictionary<string, int> ColumnIndexMapper = new Dictionary<string, int>();

        public int RowIndex { get; set; }
        public int ColumnIndex(string columnName)
        {
            if (ColumnIndexMapper.ContainsKey(columnName)) return ColumnIndexMapper[columnName];
            return -1;
        }
        public string phone { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string login { get; set; }
        public string deal { get; set; }
        public string nickName { get; set; }
        public int Level { get; set; }

        public int BaoLiaoLeftCount { get; set; }

        public string GoldTransferTarget { get; set; }
        public string StatusFilePath { get; set; }
        public int GoldLeft { get; set; }
        //public int baoLiaoCount { get; set; }
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
        //public Account(string phone, string email, string pass)
        //{
        //    this.phone = phone;
        //    password = pass;
        //    this.email = email;
        //    waitTime = 5;
        //    order = 2;
        //    limit = 10;
        //    descriptionMode = 1;
        //    output = "";
        //}
        public Account()
        {
            waitTime = 5;
            order = 2;
            limit = 20;
            descriptionMode = 0;
            output = "";
        }

    }
}
