using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
namespace SmzdmBot
{
    public class ExcelManager
    {
        public void Load(string filePath)
        {
            var MyApp = new Excel.Application();
            MyApp.Visible = false;
            var MyBook = MyApp.Workbooks.Open(@"D:\GitHub\smzdm\WebBrowser\data\smzdm_status.csv");
            var MySheet = (Excel.Worksheet)MyBook.Sheets[1]; // Explicit cast is not required here
            for (int row = 1; row < MySheet.UsedRange.Rows.Count; row++) // Count is 1048576 instead of 4
            {
                for (int col = 1; col < MySheet.UsedRange.Columns.Count; col++) // Count is 16384 instead of 4
                {
                    var dataRange = (Range)MySheet.UsedRange.Cells[row, col];
                    Console.Write(String.Format(dataRange.Value2.ToString() + " "));
                }
                Console.WriteLine();
            }
        }
    }
}
