using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmzdmExcelAddin
{
    public static class DataTableExt
    {
        //public static void WriteToCsvFile(this System.Data.DataTable dataTable, string filePath)
        //{
        //    StringBuilder fileContent = new StringBuilder();

        //    foreach (var col in dataTable.Columns)
        //    {
        //        fileContent.Append(col.ToString() + ",");
        //    }

        //    fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);

        //    foreach (DataRow dr in dataTable.Rows)
        //    {
        //        foreach (var column in dr.ItemArray)
        //        {
        //            fileContent.Append("\"" + column.ToString() + "\",");
        //        }

        //        fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);
        //    }

        //    System.IO.File.WriteAllText(filePath, fileContent.ToString());
        //}
        public static List<List<string>> RangeToLists(Range range, int columns)
        {
            object[,] values = range.Value2 as object[,];

            //firstColumn.t
            List<List<string>> table = new List<List<string>>();
            List<string> list = new List<string>();
            foreach (object o in values)
            {
                if (list.Count == columns)
                {
                    table.Add(list);
                    list = new List<string>();
                }
                if (o == null)
                {
                    list.Add("");
                }
                else
                {
                    list.Add(o.ToString());
                }

            }
            if (list.Count != 0)
            {
                table.Add(list);
            }
            return table;
        }
    }
}
