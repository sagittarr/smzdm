﻿
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SmzdmBot
{
    public class ExcelManager
    {

        //public static void Save(DataTable dataTable, string path)
        //{
        //    using (ExcelPackage pck = new ExcelPackage(new File(path)))
        //    {
        //        ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Account");
        //        ws.Cells["A1"].LoadFromDataTable(dataTable, true);
        //        pck.Save();
        //    }
        //}
        public static void Load(string path)
        {
            DataTable dataTable = GetDataTableFromExcel(path);
            Console.WriteLine(dataTable.Rows[1]["nickname"].ToString());
            var account = new Account(dataTable.Rows[1]["phone"].ToString(), dataTable.Rows[1]["password"].ToString(), dataTable.Rows[1]["email"].ToString());
            account.login = dataTable.Rows[1]["login"].ToString();
            Console.WriteLine(JsonConvert.SerializeObject(account));
        }

        public static DataTable GetDataTableFromExcel(string path, bool hasHeader = true)
        {
            using (var pck = new OfficeOpenXml.ExcelPackage())
            {
                using (var stream = File.OpenRead(path))
                {
                    pck.Load(stream);
                }
                var ws = pck.Workbook.Worksheets.First();
                DataTable tbl = new DataTable();
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
                }
                var startRow = hasHeader ? 2 : 1;
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    DataRow row = tbl.Rows.Add();
                    foreach (var cell in wsRow)
                    {
                        row[cell.Start.Column - 1] = cell.Text;
                    }
                }
                return tbl;
            }
        }

    }

}