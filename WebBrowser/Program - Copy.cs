﻿//using Newtonsoft.Json;
//using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Firefox;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace SmzdmBot
//{

//    class Program
//    {
//        static void Main(string[] args)
//        {
//            TaskManager.Start(args).Wait();
//            //, @"C:\Users\jiatwang\Documents\smzdm_config\task.txt", @"C:\Users\jiatwang\Documents\smzdm_config\payee.txt"
//            return;
//            var arguments = File.ReadAllText(args[0]); ;
//            var account = JsonConvert.DeserializeObject<Account>(arguments);
//            var option = new Option(account);
//            var mode = account.mode;
//            Console.WriteLine(JsonConvert.SerializeObject(option));
//            //Console.ReadKey();
//            Console.OutputEncoding = System.Text.Encoding.UTF8;
//            if(mode == "smzdm")
//            {
//                DealFinder bot = new DealFinder(option);
//                var list = bot.GetSmzdmItems(option).ToList();
//                var output = new List<string>();
//                list.ForEach(x => output.Add(JsonConvert.SerializeObject(x)));
//                if (File.Exists(option.output))
//                {
//                    File.WriteAllLines(option.output, output.ToArray());
//                }
//                else
//                {
//                    Console.WriteLine(option.output + " file does not exist.");
//                    return;
//                }
//            }
//            else if (mode == "smzdm_share")
//            {
//                DealFinder bot = new DealFinder(option);
//                var pages = new List<string>();
//                if(option.pageNumbers == "auto")
//                {

//                }
//                else
//                {
//                    var pagesArr = option.pageNumbers.Split('-');
//                    var pageCode = option.ConvertHotPickCategory(option.HotPickCategory);
//                    var st = int.Parse(pagesArr[0]);
//                    var end = st;
//                    if (pagesArr.Length > 1)
//                    {
//                        end = int.Parse(pagesArr[1]);
//                    }
//                    for (int i = st; i <= end; i++)
//                    {
//                        pages.Add(pageCode + i.ToString() + "/");
//                    }
//                }

//                var helper = new DealPublisher(option);
//                if (!helper.Login())
//                {
//                    return;
//                }

//                var smzdmItemList = new List<Dictionary<string, string>>();
//                foreach (var page in pages)
//                {
//                    bot.driver.Navigate().GoToUrl(page);
//                    var items = bot.driver.FindElements(By.ClassName("z-feed-content")).ToList();

//                    foreach (IWebElement item in items)
//                    {
//                        var it = bot.GetSmzdmItem(item);
//                        if (it != null)
//                        {
//                            smzdmItemList.Add(it);
//                        }

//                    }
//                }
//                Console.WriteLine("Collected good price count " + smzdmItemList.Count);
//                foreach (var it in smzdmItemList)
//                {
//                    var price = bot.CheckPrice(it);
//                    if (price != null)
//                    {
//                        price.Calculate();
//                        var goodPrice = price.SmzdmGoodPrice;
//                        var sourceUrl = price.sourceUrl;
//                        var code = helper.PasteItemUrl(sourceUrl, 0, option.waitBaoliao, option.baoLiaoStopNumber);
//                        if (code == 1) helper.SubmitBaoLiao(option.descriptionMode, goodPrice, 0.0, sourceUrl, option.PriceRate, 0.0, price);
//                        else if (code == 2)
//                        {
//                            break;
//                        }
//                    }
//                }
//                //helper.Like();
//                Console.WriteLine("Finished.");
//                helper.LogStatus();
//                if (helper.gold > 1)
//                {
//                    Console.WriteLine(option.username + " gold=" + helper.gold);
//                    //helper.TransferGold("https://post.smzdm.com/p/amm539rz/");
//                }
//                else
//                {
//                    helper.Shutdown();
//                }
//                return;
//            }
//            else if(mode == "wiki_share")
//            {
//                DealFinder bot = new DealFinder(option);
//                var priceList = new List<Price>();
//                var pages = option.SmzdmWikiPages.Split(',');
//                var itemUrls = new List<string>();
//                foreach (var page in pages)
//                {
//                    itemUrls.AddRange(bot.GetItemIdFromWikiPage(page));
//                }
//                itemUrls = itemUrls.Distinct().ToList();

//                Console.WriteLine("Collected " + itemUrls.Count + " wiki items");
//                if (itemUrls!=null && itemUrls.Count > 0)
//                {
//                    var helper = new DealPublisher(option);
//                    if (!helper.Login())
//                    {
//                        return;
//                    }
//                    foreach (var itemUrl in itemUrls)
//                    {
//                        var price = bot.CheckPrice(bot.GetLinkFromWiki(itemUrl));
//                        if (price == null) continue;
//                        //if (prices.Count > 1)
//                        //{
//                        //    prices = prices.Take(2).ToList();
//                        //}
//                        //foreach (var price in prices)
//                        //{
//                            //if (price != null)
//                            //{
//                                Console.WriteLine("Process " + JsonConvert.SerializeObject(price));
//                                var goodPrice = price.SmzdmGoodPrice;
//                                var sourceUrl = price.sourceUrl;
//                                var code = helper.PasteItemUrl(sourceUrl, 0, option.waitBaoliao, option.baoLiaoStopNumber);
//                                if (code == 1) helper.SubmitBaoLiao(option.descriptionMode, goodPrice, 0.0, sourceUrl, option.PriceRate, 0.0);
//                                else if (code == 2)
//                                {
//                                    break;
//                                }
//                            //}
//                        //}
//                    }
//                    Console.WriteLine("Finished.");
//                    helper.LogStatus();
//                    if (helper.gold > 1)
//                    {
//                        Console.WriteLine(option.username + " gold=" + helper.gold);
//                    }
//                    else
//                    {
//                        helper.Shutdown();
//                    }
//                    return;
//                }
//            }
//            else if (mode == "search")
//            {
//                if (File.Exists(option.input))
//                {
//                    var outputPath = option.output;
//                    var lines = new List<string>();
//                    foreach (var line in File.ReadAllLines(option.input))
//                    {
//                        lines.Add(Helper.CheckUrl(line));
//                    }
//                    var urls = lines.Distinct().ToList();
//                    if (urls.Count == 0) return;
//                    DealFinder bot = new DealFinder(option);
//                    bot.SearchAll(urls, outputPath);
//                }
//                else
//                {
//                    Console.WriteLine(args[0] + " file does not exist.");
//                }
//                Console.WriteLine("Finished Deal Search.");
//                return;
//            }
//            else if (mode == "crawl")
//            {
//                if (File.Exists(option.input))
//                {
//                    var outputPath = option.output;
//                    var lines = File.ReadAllLines(option.input).ToList();
//                    if (lines.Count == 0) return;
//                    DealFinder bot = new DealFinder(option);
//                    bot.Crawl(lines[0], outputPath, option.CrawlCount);
//                }
//                else
//                {
//                    Console.WriteLine(args[0] + " file does not exist.");
//                }
//                Console.WriteLine("Finished Deal Search.");
//                return;
//            }
//            else if (mode == "share")
//            {
//                var helper = new DealPublisher(option);
//                //var list = GetItemList(helper);
//                var list =  File.ReadAllLines(option.sourcePath).ToList();
//                if(list == null || list.Count == 0)
//                {
//                    return;
//                }
//                if (!helper.Login())
//                {
//                    return;
//                }
//                int i = 0;
//                list = Helper.ShuffleList<string>(list);
//                var visited = new HashSet<string>();
//                while (i < list.Count)
//                {
//                    var goodPrice = 0.0;
//                    var oldPrice = 0.0;
//                    var sourceUrl = "";
//                    var code = 0;
//                    Price priceObject = null;
//                    while (0 == code && i < list.Count)
//                    {
//                        if (list[i].StartsWith("{"))
//                        {
//                            priceObject = JsonConvert.DeserializeObject<Price>(list[i]);
//                            goodPrice = priceObject.SmzdmGoodPrice;
//                            sourceUrl = Helper.CheckUrl(priceObject.sourceUrl);
//                            oldPrice = priceObject.oldPrice;
//                        }
//                        //else
//                        //{
//                        //    var url = Helper.CheckUrl(list[i]);
//                        //    if (url != null)
//                        //    {
//                        //        sourceUrl = url;
//                        //    }
//                        //}
//                        if (!String.IsNullOrWhiteSpace(sourceUrl) && !visited.Contains(sourceUrl))
//                        {
//                            visited.Add(sourceUrl);
//                            code = helper.PasteItemUrl(sourceUrl, i, option.waitBaoliao, option.baoLiaoStopNumber);
//                        }
//                        i++;
//                    }
//                    if (code == 2) break; // 2 reach end condition, quit // 1 continue submit

//                    if (priceObject != null)
//                    {
//                        helper.SubmitBaoLiao(option.descriptionMode, goodPrice, oldPrice, sourceUrl, 0.0, 0.9, priceObject);
//                    }
//                    else
//                    {
//                        helper.SubmitBaoLiao(option.descriptionMode, goodPrice, oldPrice, sourceUrl, 0.0, 0.9);
//                    }

//                }
//                Console.WriteLine("Finished.");
//                helper.LogStatus();
//                if (helper.gold > 1)
//                {
//                    Console.WriteLine(option.username + " gold=" + helper.gold);
//                }
//                else
//                {
//                    helper.Shutdown();
//                }
//            }
//            else
//            {
//                Console.WriteLine("unknown mode " + mode);

//                return;
//            }
//            return;
//        }
//    }
//}
