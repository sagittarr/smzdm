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
        static void Main(string[] args)
        {
            string mode = "";
            //var accounts = ExcelManager.Load(@"D:\GitHub\smzdm\WebBrowser\data\smzdm.xlsx");
            //Console.WriteLine(JsonConvert.SerializeObject(accounts));
            //Console.ReadKey();
            //RunTasks();
            //Console.ReadKey();
            Option option = new Option();
            if (args.Length>=1)
            {
                mode = args[0];
                if(mode == "smzdm")
                {
                    option.output = args[1];
                    option.pageNumbers = args[2];
                }
                else if(mode == "search")
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

                else if (mode == "smzdm_share")
                {
                    option = new Option(args.Skip(1).ToArray());
                    option.pageNumbers = args[10];
                    if (args.Length >= 12) option.PriceRate = Double.Parse(args[11]);
                }
                else if(mode == "wiki_share")
                {
                    option = new Option(args.Skip(1).ToArray());
                    option.SmzdmWikiPages = args[10];
                    if(args.Length>=12) option.PriceRate = Double.Parse(args[11]);
                }
            }
            else
            {
                Console.WriteLine("unknown command");
                Console.ReadKey();
                return;
            }
            Console.WriteLine(JsonConvert.SerializeObject(option));
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if(mode == "smzdm")
            {
                DealSearchBot bot = new DealSearchBot();
                var list = bot.GetSmzdmItems(option).ToList();
                var output = new List<string>();
                list.ForEach(x => output.Add(JsonConvert.SerializeObject(x)));
                if (File.Exists(option.output))
                {
                    File.WriteAllLines(option.output, output.ToArray());
                }
                else
                {
                    Console.WriteLine(option.output + " file does not exist.");
                    return;
                }
            }
            else if (mode == "smzdm_share")
            {
                DealSearchBot bot = new DealSearchBot();

                var pages = new List<string>();
                var pagesArr = option.pageNumbers.Split('-');
                var st = int.Parse(pagesArr[0]);
                var end = int.Parse(pagesArr[1]);
                for(int i = st; i<= end; i++)
                {
                    pages.Add("https://www.smzdm.com/jingxuan/p" + i.ToString() + "/");
                }
                var helper = new SmzdmWorker(option);
                if (!helper.Login())
                {
                    return;
                }
                var smzdmItemList = new List<Dictionary<string, string>>();
                foreach (var page in pages)
                {
                    bot.driver.Navigate().GoToUrl(page);
                    var items = bot.driver.FindElements(By.ClassName("z-feed-content")).ToList();
                    
                    foreach(IWebElement item in items)
                    {
                        var it = bot.GetSmzdmItem(item);
                        if (it != null)
                        {
                            smzdmItemList.Add(it);
                        }

                    }
                }
                Console.WriteLine("Potential good price count " + smzdmItemList.Count);
                foreach (var it in smzdmItemList)
                {
                    var price = bot.CheckSmzdmItem(it);
                    if (price != null)
                    {
                        var goodPrice = price.SmzdmGoodPrice;
                        var sourceUrl = price.sourceUrl;
                        var code = helper.PasteItemUrl(sourceUrl, 0, option.waitBaoliao, option.baoLiaoStopNumber);
                        if (code == 1) helper.SubmitBaoLiao(option.descriptionMode, goodPrice, 0.0, sourceUrl, option.PriceRate, 0.0);
                        else if (code == 2)
                        {
                            break;
                        }
                    }
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
                return;
            }
            else if(mode == "wiki_share")
            {
                DealSearchBot bot = new DealSearchBot();
                var priceList = new List<Price>();
                var pages = option.SmzdmWikiPages.Split(',');
                var itemUrls = new List<string>();
                foreach (var page in pages)
                {
                    itemUrls.AddRange(bot.GetItemIdFromWikiPage(page));
                }
                itemUrls = itemUrls.Distinct().ToList();

                Console.WriteLine("Collected " + itemUrls.Count + " wiki items");
                if (itemUrls!=null && itemUrls.Count > 0)
                {
                    var helper = new SmzdmWorker(option);
                    if (!helper.Login())
                    {
                        return;
                    }
                    foreach (var itemUrl in itemUrls)
                    {
                        var prices = bot.CheckSmzdmItem(bot.GetLinkFromWiki(itemUrl));
                        if (prices == null) continue;
                        if (prices.Count > 1)
                        {
                            prices = prices.Take(2).ToList();
                        }
                        foreach (var price in prices)
                        {
                            if (price != null)
                            {
                                Console.WriteLine("Process " + JsonConvert.SerializeObject(price));
                                var goodPrice = price.SmzdmGoodPrice;
                                var sourceUrl = price.sourceUrl;
                                var code = helper.PasteItemUrl(sourceUrl, 0, option.waitBaoliao, option.baoLiaoStopNumber);
                                if (code == 1) helper.SubmitBaoLiao(option.descriptionMode, goodPrice, 0.0, sourceUrl, option.PriceRate, 0.0);
                                else if (code == 2)
                                {
                                    break;
                                }
                            }
                        }
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
                    return;
                }
            }
            else if (mode == "search")
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
                //var list = GetItemList(helper);
                var list =  File.ReadAllLines(option.sourcePath).ToList();
                if(list == null || list.Count == 0)
                {
                    return;
                }
                if (!helper.Login())
                {
                    return;
                }
                int i = 0;
                list = Helper.ShuffleList<string>(list);
                var visited = new HashSet<string>();
                while (i < list.Count)
                {
                    var goodPrice = 0.0;
                    var oldPrice = 0.0;
                    var sourceUrl = "";
                    var code = 0;
                    while (0 == code && i < list.Count)
                    {
                        if (list[i].StartsWith("{"))
                        {
                            var price = JsonConvert.DeserializeObject<Price>(list[i]);
                            goodPrice = price.SmzdmGoodPrice;
                            sourceUrl = Helper.CheckUrl(price.sourceUrl);
                            oldPrice = price.oldPrice;
                        }
                        //else 
                        //{
                        //    var url = Helper.CheckUrl(list[i]);
                        //    if (url != null)
                        //    {
                        //        sourceUrl = url;
                        //    }
                        //}
                        if (!String.IsNullOrWhiteSpace(sourceUrl) && !visited.Contains(sourceUrl))
                        {
                            visited.Add(sourceUrl);
                            code = helper.PasteItemUrl(sourceUrl, i, option.waitBaoliao, option.baoLiaoStopNumber);
                        }
                        i++;
                    }
                    if (code == 2) break; // 2 reach end condition, quit
                                          // 1 continue submit
                    helper.SubmitBaoLiao(option.descriptionMode, goodPrice, oldPrice, sourceUrl,0.0,0.5);
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
