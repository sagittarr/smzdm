using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SmzdmBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmzdmBot
{

    class Program
    {



        //public static SmzdmWorker[] RunTasks()
        //{
        //    var option = new Option();
        //    option.username = "smzdm68194@outlook.com";
        //    option.password = "smzdm9876";
        //    Task<SmzdmWorker>[] taskArray = { Task<SmzdmWorker>.Factory.StartNew(() => GetNewWorker(option))};

        //    var results = new SmzdmWorker[taskArray.Length];

        //    for (int i = 0; i < taskArray.Length; i++)
        //    {
        //        results[i] = taskArray[i].Result;   
        //    }
        //    return results;
        //}

        //private static SmzdmWorker GetNewWorker(Option option)
        //{
        //    var worker = new SmzdmWorker(option);
        //    worker.Login();
        //    return worker;
        //}

        static void Main(string[] args)
        {
            string mode = "";
            //var accounts = ExcelManager.Load(@"D:\GitHub\smzdm\WebBrowser\data\smzdm.xlsx");
            //Console.WriteLine(JsonConvert.SerializeObject(accounts));
            //Console.ReadKey();
            //RunTasks();
            //Console.ReadKey();
            Option option = new Option();
            if (args.Length>1)
            {
                mode = args[0];
                if(mode == "search" || mode == "crawl")
                {
                    option.input = args[1];
                    option.output = args[2];
                }
                else if(mode == "crawl")
                {
                    option.input = args[1];
                    option.output = args[2];
                    if (args.Length >= 4)
                    {
                        option.CrawlCount = int.Parse(args[3]);
                    }
                    
                }
                else if(mode == "login")
                {
                    option.username = args[1];
                    option.password = args[2];
                    option.output = args[3];
                }
                else if(mode == "share")
                {
                    option = new Option(args.Skip(1).ToArray());
                }
            }
            else
            {
                Console.WriteLine("unknown command");
                Console.ReadKey();
                return;
            }
            Console.WriteLine(JsonConvert.SerializeObject(option));
            Console.WriteLine("Type any key to continue");
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (mode == "search")
            {
                if (File.Exists(option.input))
                {
                    var outputPath = option.output;
                    var lines = new List<string>();
                    foreach (var line in File.ReadAllLines(option.input))
                    {
                        lines.Add(Helper.CheckUrl(line));
                    }
                    var urls = lines.Distinct().ToList();
                    if (urls.Count == 0) return;
                    DealSearchBot bot = new DealSearchBot();
                    bot.SearchAll(urls, outputPath);
                }
                else
                {
                    Console.WriteLine(args[0] + " file does not exist.");
                }
                Console.WriteLine("Finished Deal Search.");
                return;
            }
            else if (mode == "crawl")
            {
                if (File.Exists(option.input))
                {
                    var outputPath = option.output;
                    var lines = File.ReadAllLines(option.input).ToList();
                    if (lines.Count == 0) return;
                    DealSearchBot bot = new DealSearchBot();
                    bot.Crawl(lines[0], outputPath, option.CrawlCount);
                }
                else
                {
                    Console.WriteLine(args[0] + " file does not exist.");
                }
                Console.WriteLine("Finished Deal Search.");
                return;
            }
            else if (mode == "share")
            {
                var helper = new SmzdmWorker(option);
                var list = GetItemList(helper);
                if (!helper.Login())
                {
                    return;
                }
                int i = 0;
                while (i < list.Count)
                {
                    var code = 0;
                    while (0 == code && i < list.Count)
                    {
                        code = helper.PasteItemUrl(list[i], i, option.waitBaoliao, option.baoLiaoStopNumber);
                        i++;
                    }
                    if (code == 2) break;
                    helper.SubmitBaoLiao(option.descriptionMode);
                }
                Console.WriteLine("Finished.");
                helper.OutputStatus();
                if (helper.gold > 1)
                {
                    Console.WriteLine(option.username + " gold=" + helper.gold);
                }
                else
                {
                    helper.Shutdown();
                }
            }
            else if (mode == "login")
            {
                var helper = new SmzdmWorker(option);
                if (!helper.Login())
                {
                    return;
                }
                helper.ReadInfo();
                helper.OutputStatus();
                if (helper.gold > 1)
                {
                    Console.WriteLine(option.username + " gold=" + helper.gold);
                }
                else
                {
                    helper.Shutdown();
                }
            }
            else
            {
                Console.WriteLine("unknown mode " + mode);

                return;
            }
            return;
        }

        public static List<string> GetItemList(SmzdmWorker bot)
        {
            var list = new List<string>();
            var _list = new List<string>();
            if (bot.option.sourcePath.StartsWith("http"))
            {
                list = getGoodsItemListByDriver(bot.driver, bot.option.sourcePath);
            }
            else
            {
                var lines = File.ReadAllLines(bot.option.sourcePath);
                _list.AddRange(lines);
                var priceList = new List<Price>();
                foreach (var line in _list)
                {
                    if (line.StartsWith("{\""))
                    {
                        var p = JsonConvert.DeserializeObject<Price>(line);
                        if (p != null)
                        {
                            priceList.Add(p);
                        }
                    }
                    var u = Helper.CheckUrl(line);

                    if (!string.IsNullOrWhiteSpace(u))
                    {
                        list.Add(u);
                    }
                }
            }
            list = list.Distinct().ToList().Where(x => !string.IsNullOrWhiteSpace(x)).ToList();


            if (bot.option.itemLinkOrder == 1)
            {
                list.Reverse();
            }
            if (bot.option.itemLinkOrder == 2)
            {
                list = Helper.ShuffleList<string>(list);
            }
            return list;
        }

        private static List<string> getGoodsItemListByDriver(IWebDriver driver, string url, int retry = 10)
        {
            driver.Navigate().GoToUrl(url);
            var res = new List<string>();
            while (res.Count == 0 && retry > 0)
            {
                Thread.Sleep(3000);
                Console.WriteLine("Sleep 3s for webpage loading");
                retry--;
                var links = driver.FindElements(By.TagName("a"));
                foreach (var l in links)
                {
                    var href = l.GetAttribute("href");
                    if (href == null) continue;
                    var link = Helper.CheckUrl(href);
                    if (!string.IsNullOrWhiteSpace(link))
                    {
                        res.Add(link);
                    }
                }
            }
            return res.Distinct().ToList();
        }
    }
}
