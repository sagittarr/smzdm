using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;
using SmzdmBot;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SmzdmExcelAddin
{
    public partial class SmzdmUserControl : UserControl
    {
        private string ConfigPath = "./smzdm_config.txt";
        private string ExePath = "";
        private string StatusPath = "";
        public SmzdmUserControl()
        {
            InitializeComponent();
            try
            {
                var configText = File.ReadAllText(ConfigPath);
                var config = JsonConvert.DeserializeObject<string[]>(configText);
                ExePath = config[0];
                StatusPath = config[1];
                textBox1.Text = ExePath;
                textBox3.Text = StatusPath;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
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
        //private List<string> ColumnNames = new List<string>(){ "phone", "password", "email", "email password", "nikname", "BaoLiaoLeft", "level", "deals", "login", "mode", "category", "pages", "discount rate", "comment" };
        //
        private List<Account> LoadAccounts()
        {
            try
            {
                Worksheet ws = Globals.ThisAddIn.Application.ActiveWorkbook.ActiveSheet;

                var columnCount = ws.UsedRange.Columns.Count;
                var rowCount = ws.UsedRange.Rows.Count;
                var lists = DataTableExt.RangeToLists(ws.UsedRange, columnCount);
                Account.ColumnIndexMapper = new Dictionary<string, int>();
                for(int i = 0; i<lists[0].Count; i++)
                {
                    Account.ColumnIndexMapper.Add(lists[0][i], i);
                }
                var accounts = new List<Account>();
                for (var i = 1; i < lists.Count; i++)
                {
                    var account = new Account
                    {
                        email = lists[i][Account.ColumnIndexMapper["email"]],
                        phone = lists[i][Account.ColumnIndexMapper["phone"]],
                        password = lists[i][Account.ColumnIndexMapper["password"]],
                        mode = lists[i][Account.ColumnIndexMapper["mode"]],
                        pages = lists[i][Account.ColumnIndexMapper["pages"]],
                        category = lists[i][Account.ColumnIndexMapper["category"]],
                        login = lists[i][Account.ColumnIndexMapper["login"]],
                        StatusFilePath = textBox3.Text,
                        discountRate = Double.Parse(lists[i][Account.ColumnIndexMapper["discount rate"]]),
                        RowIndex = i
                    };
                    accounts.Add(account);
                }
                return accounts;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                MessageBox.Show("无权限");
            }
            return null;
        }
        private void LaunchButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                Worksheet ws = Globals.ThisAddIn.Application.ActiveWorkbook.ActiveSheet;

                var columnCount = ws.UsedRange.Columns.Count;
                var rowCount = ws.UsedRange.Rows.Count;
                var lists = DataTableExt.RangeToLists(ws.UsedRange, columnCount);
                var names = new List<string> { "phone","password","email", "password", "nikname", "BaoLiaoLeft", "level", "deals", "login", "mode", "category", "pages", "discount rate", "comment" };
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
                //var argPath = "";
                //if (String.IsNullOrWhiteSpace(textBox2.Text))
                //{
                //    MessageBox.Show("arg path is empty");
                //    return;
                //}
                //else
                //{
                //    argPath = textBox2.Text;
                //}
                var accounts = LoadAccounts();
                foreach(var account in accounts)
                {
                    if(account.login == "y")
                    {
                        var selectedOption = MessageBox.Show(JsonConvert.SerializeObject(account) + "  Please confirm.", "Ready to run?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        if (selectedOption == DialogResult.Yes)
                        {
                            var argPath = "./arguments.txt";
                            File.WriteAllText(argPath, JsonConvert.SerializeObject(account));
                            try
                            {
                                Process.Start(exePath, argPath);
                                Thread.Sleep(5000);
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
            ExePath = textBox1.Text;
            StatusPath = textBox3.Text;
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new string[] { ExePath, StatusPath }));
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            ExePath = textBox1.Text;
            StatusPath = textBox3.Text;
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new string[] { ExePath, StatusPath }));
        }

        private void ReLoadButton_Clicked(object sender, EventArgs e)
        {
            var path = textBox3.Text;
            var newOnes = new List<Account>();
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                foreach(var line in lines)
                {
                    var account = JsonConvert.DeserializeObject<Account>(line);
                    newOnes.Add(account);
                    //MessageBox.Show(JsonConvert.SerializeObject(account));
                }
            }
            else
            {
                MessageBox.Show(path + " not exsits.");
            }
            var current = LoadAccounts();
            Worksheet ws = Globals.ThisAddIn.Application.ActiveWorkbook.ActiveSheet;
            //ws.Cells[8, 2].Value = "Salary";
            foreach (var item in current)
            {
                foreach(var newOne in newOnes)
                {
                    if (item.phone == newOne.phone || item.email == newOne.email)
                    {
                        item.Level = newOne.Level;
                        item.BaoLiaoLeftCount = newOne.BaoLiaoLeftCount;
                        item.GoldLeft = newOne.GoldLeft;
                        //MessageBox.Show(item.RowIndex + " " + Account.ColumnIndexMapper["level"]);
                        ws.Cells[item.RowIndex + 1, Account.ColumnIndexMapper["BaoLiaoLeft"] + 1].Value = item.BaoLiaoLeftCount.ToString();
                        ws.Cells[item.RowIndex + 1, Account.ColumnIndexMapper["level"] + 1].Value = item.Level.ToString();
                        ws.Cells[item.RowIndex + 1, Account.ColumnIndexMapper["gold"] + 1].Value = item.GoldLeft.ToString();
                        //MessageBox.Show(value.ToString());
                    }
                }
                
            }
            File.Delete(path);
        }
    }
}
