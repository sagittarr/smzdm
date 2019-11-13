using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmzdmBot
{
    public class CsvHelper
    {
        public static List<List<string>> GetRowsByNames(List<List<string>> table, List<string> names)
        {
            var res = new List<List<string>>();
            List<int> index = new List<int>();
            names.ForEach(n => index.Add(table[0].IndexOf(n)));
            index.RemoveAll(p => p==-1);
            for (int i=1; i<table.Count; i++)
            {
                var row = new List<string>();
                foreach(var idx in index)
                {
                    row.Add(table[i][idx]);
                }
                res.Add(row);
            }
            return res;
        }
    }
}
