using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmzdmBot
{

    class Program
    {
        static void Main(string[] args)
        {
            //Console.ReadKey();
            TaskManager.Start(args).Wait();
            // @"C:\Users\jiatwang\Documents\smzdm_config\task.txt", @"C:\Users\jiatwang\Documents\smzdm_config\payee.txt"
            return;
        }
    }
}
