using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;
using SmzdmBot;
using Newtonsoft.Json;
using System.Diagnostics;

namespace SmzdmExcelAddin
{
    public partial class SmzdmUserControl : UserControl
    {
        public SmzdmUserControl()
        {
            InitializeComponent();
        }

        private List<string> PickData(List<string> data, List<int> index)

        {
            List<string> output = new List<string>();
            foreach (var i in index)
            {
                output.Add(data[i]);
            }
            return output;
        }
        private List<int> PickIndex(List<string> data, List<string> index)

        {
            List<int> output = new List<int>();
            for (var i = 0; i < index.Count; i++)
            {
                if (String.Equals("P", index[i]))
                {
                    output.Add(i);
                }
                else if (String.Equals("C", index[i]))
                {
                    if (data[i] != null && data[i] != "")
                    {
                        //MessageBox.Show("/" + data[i] + "/");
                        output.Add(i);
                    }
                }
            }
            return output;
        }
        private System.Data.DataTable toDataTable(List<List<string>> lists)
        {
            //DataSet ds = new DataSet("New_DataSet");
            System.Data.DataTable dt = new System.Data.DataTable("New_DataTable");
            if (lists.Count > 0)
            {
                foreach (var v in lists[0])
                {
                    //if (dt.Columns.Contains(v))
                    //{
                    //    dt.Columns.Add(new DataColumn(v+'*'));
                    //}
                    //else
                    //{
                    dt.Columns.Add(new DataColumn(v));
                    //

                }
            }

            for (var i = 0; i < lists.Count; i++)
            {
                for (var j = 0; j < lists[i].Count; j++)
                {
                    if (j < lists[0].Count)
                    {
                        try
                        {
                            DataRow row = dt.Rows.Add();
                            row[lists[0][j]] = lists[i][j];
                        }
                        catch (System.ArgumentException)
                        {
                            MessageBox.Show(lists[0][j]);

                        }
                    }
                }
            }
            return dt;
        }
        private List<int> getIndexFromHeader(List<string> names, List<string> values)
        {
            var indexList = new List<int>();
            names.ForEach(x => indexList.Add(values.IndexOf(x)));
            return indexList;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Worksheet ws = Globals.ThisAddIn.Application.ActiveWorkbook.ActiveSheet;

                var columnCount = ws.UsedRange.Columns.Count;
                var rowCount = ws.UsedRange.Rows.Count;
                var lists = DataTableExt.RangeToLists(ws.UsedRange, columnCount);
                var names = new List<string> { "phone","password","email", "password", "nikname", "level", "deals", "login", "mode", "category", "pages", "discount rate", "comment" };
                var indexList = getIndexFromHeader(names, lists[0]);

                int loginIndex = lists[0].IndexOf("login");
                if (loginIndex == -1)
                {
                    MessageBox.Show("login missing");
                    return;
                }
                //Globals.ThisAddIn.Application.DisplayAlerts = !fileOverrideCheckBox.Checked;
                //@"D:\GitHub\smzdm\WebBrowser\bin\Release\SmzdmBot.exe"
                var exePath = "";
                if (String.IsNullOrWhiteSpace(textBox1.Text))
                {
                    MessageBox.Show("exe path is empty");
                    return;
                }
                else
                {
                    exePath = textBox1.Text;
                }
                var argPath = "";
                if (String.IsNullOrWhiteSpace(textBox2.Text))
                {
                    MessageBox.Show("arg path is empty");
                    return;
                }
                else
                {
                    argPath = textBox2.Text;
                }
                for (var i = 1; i < lists.Count; i++)
                {
                    if(lists[i][loginIndex] == "y")
                    {
                        var account = new Account();
                        account.email = lists[i][indexList[names.IndexOf("email")]];
                        account.phone = lists[i][indexList[names.IndexOf("phone")]];
                        account.password = lists[i][indexList[names.IndexOf("password")]];
                        account.mode = lists[i][indexList[names.IndexOf("mode")]] ;
                        account.category = lists[i][indexList[names.IndexOf("category")]];
                        account.pages = lists[i][indexList[names.IndexOf("pages")]];
                        account.category = lists[i][indexList[names.IndexOf("category")]];
                        account.discountRate = Double.Parse(lists[i][indexList[names.IndexOf("discount rate")]]);
                        var selectedOption = MessageBox.Show(JsonConvert.SerializeObject(account) + "  Please confirm.", "Ready to run?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        if(selectedOption == DialogResult.Yes)
                        {
                            System.IO.File.WriteAllText(argPath, JsonConvert.SerializeObject(account));
                            try
                            {
                                Process.Start(exePath, argPath);
                            }
                            catch (Exception es)
                            {
                                MessageBox.Show(es.Message);
                            }
                        }
                    }
                }

            }
            catch (System.Runtime.InteropServices.COMException)
            {
                MessageBox.Show("无权限");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
