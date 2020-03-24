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
        private string ArgsPath = "";
        private string ExePath = "";
        private string TaskPath = "";
        private string PayeePath = "";
        private string StatusPath = "";

        public SmzdmUserControl()
        {
            InitializeComponent();
            try
            {
                LoadSetting();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
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
        private void LoadSetting()
        {
            Worksheet ws = Globals.ThisAddIn.Application.ActiveWorkbook.Sheets["setting"];
            var columnCount = ws.UsedRange.Columns.Count;
            var rowCount = ws.UsedRange.Rows.Count;
            var lists = DataTableExt.RangeToLists(ws.UsedRange, columnCount);
            ExePath = lists[0][1];
            ArgsPath = lists[1][1];
            TaskPath = lists[2][1];
            PayeePath = lists[3][1];
            StatusPath = lists[4][1];
            //MessageBox.Show(String.Join(",", lists[0]));
        }
        private void LaunchButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var accounts = LoadAccounts();
                foreach(var account in accounts)
                {
                    if(account.login.ToLower() == "y")
                    {
                        //var selectedOption = MessageBox.Show(JsonConvert.SerializeObject(account) + "  Please confirm.", "Ready to run?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        //if (selectedOption == DialogResult.Yes)
                        //{
                            File.WriteAllText(ArgsPath, JsonConvert.SerializeObject(account));
                            try
                            {
                                Process.Start(ExePath, ArgsPath+" "+TaskPath+" "+PayeePath+" "+StatusPath);
                                Thread.Sleep(5000);
                            }
                            catch (Exception es)
                            {
                                MessageBox.Show(es.Message);
                            }
                        //}
                    }
                }

            }
            catch (System.Runtime.InteropServices.COMException)
            {
                MessageBox.Show("无权限");
            }
        }

        private void ReLoadButton_Clicked(object sender, EventArgs e)
        {
            var path = StatusPath;
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
