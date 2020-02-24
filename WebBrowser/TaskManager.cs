using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmzdmBot
{
    public class TaskManager
    {
        static async Task WriteFileAsync(string path, string content)
        {
            //Console.WriteLine("Async Write File has started");
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                await outputFile.WriteAsync(content);
            }
            //Console.WriteLine("Async Write File has completed");
        }
        public static async Task Start(string[] args, string taskPath, string payeePath)
        {
            
            var opt = LoadOption(args);
            Console.Title = opt.username;
            DealPublisher publisher = new DealPublisher(opt);
            if(opt.Mode == "login")
            {
                publisher.Login();
            }
            else if(opt.Mode == "pay")
            {
                publisher.Login();
                TransferGold(publisher, payeePath);
            }
            else if(opt.Mode == "smzdm_share")
            {
                publisher.Login();
                publisher.driver.Navigate().GoToUrl(@"https://www.smzdm.com/baoliao/?old");
                Thread.Sleep(5000);
                publisher.ReadInfo();
                if(publisher.baoLiaoLeft != 0)
                {
                    DealFinder finder = new DealFinder(opt);
                    while (publisher.baoLiaoLeft != 0)
                    {
                        String content;
                        using (StreamReader reader = File.OpenText(taskPath))
                        {
                            //Console.WriteLine("Opened file.");
                            content = await reader.ReadToEndAsync();
                            //Console.WriteLine(content);
                        }
                        content = ProcessTask(content, out Tuple<string, int> task);
                        await WriteFileAsync(taskPath, content);
                        Console.WriteLine("Write done.");
                        opt.HotPickCategory = task.Item1;
                        Console.WriteLine(opt.HotPickCategory);
                        var urlroot = opt.ConvertHotPickCategory(opt.HotPickCategory);
                        if (urlroot == null) return;
                        var pageUrl = urlroot + task.Item2.ToString() + "/";
                        try { 
                            FindDealAndPublish(opt, pageUrl, finder, publisher);
                        }
                        catch (WebDriverException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    finder.driver.Quit();
                }
            }
            publisher.Follow();
            try
            {
                
                publisher.Punch();
                publisher.Like();
                TransferGold(publisher, payeePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            publisher.LogStatus();
            publisher.driver.Quit();
        }
        static void TransferGold(DealPublisher publisher, string payeePath)
        {
            using (StreamReader reader = File.OpenText(payeePath))
            {
                var payee = reader.ReadToEnd();
                var payeeList = payee.Split('\n');
                Console.WriteLine(payeeList[0]);
                try
                {
                    publisher.TransferGold2(payeeList[0]);
                }
                catch (NoSuchElementException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        static string ProcessTask(string content, out Tuple<string, int> task)
        {
            var lines = content.Split('\n').ToList();
            Console.WriteLine(content);
            var orderText = lines[0].TrimEnd().TrimEnd('\n');
            var categoryIdx = int.Parse(lines[1]);
            var pageNumber = int.Parse(lines[2]);
            var order = orderText.Split(',').ToList();
            task = new Tuple<string, int>(order[categoryIdx], pageNumber);
            categoryIdx++;
            if (categoryIdx >= order.Count)
            {
                categoryIdx = 0;
                pageNumber += 1;
            }
            if (pageNumber >= 250)//end 
            {
                pageNumber = 40;//start
            }
            return orderText + "\n" + categoryIdx + "\n" + pageNumber;
        }
        static Option LoadOption(string[] args)
        {
            var arguments = File.ReadAllText(args[0]); ;
            var account = JsonConvert.DeserializeObject<Account>(arguments);
            var option = new Option(account);
            //var mode = account.mode;
            Console.WriteLine(JsonConvert.SerializeObject(option));
            //Console.OutputEncoding = System.Text.Encoding.UTF8;
            return option;
        }
        static void FindDealAndPublish(Option option, string pageUrl, DealFinder dealFinder, DealPublisher publisher)
        {
            var smzdmItemList = new List<Dictionary<string, string>>();
            dealFinder.driver.Navigate().GoToUrl(pageUrl);
            //Helper.CloseOtherTabs(dealFinder.driver);
            var items = dealFinder.driver.FindElements(By.ClassName("z-feed-content")).ToList();

            foreach (IWebElement item in items)
            {
                var it = dealFinder.GetSmzdmItem(item);
                if (it != null)
                {
                    smzdmItemList.Add(it);
                }

            }

            Console.WriteLine("Collected good price count " + smzdmItemList.Count);
            foreach (var it in smzdmItemList)
            {
                var price = dealFinder.CheckPrice(it);
                if (price != null)
                {
                    price.Calculate();
                    var goodPrice = price.SmzdmGoodPrice;
                    var sourceUrl = price.sourceUrl;
                    var code = publisher.PasteItemUrl(sourceUrl, 0, option.waitBaoliao, option.baoLiaoStopNumber);
                    if (code == 1) publisher.SubmitBaoLiao(option.descriptionMode, goodPrice, 0.0, sourceUrl, option.PriceRate, 0.0, price);
                    else if (code == 2)
                    {
                        break;
                    }
                }
            }

            Console.WriteLine(option.HotPickCategory + " "+ pageUrl + " Task Finished");
            Console.WriteLine("baoliao left " + publisher.baoLiaoLeft);
        }
    }
}
