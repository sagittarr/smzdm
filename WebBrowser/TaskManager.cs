using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmzdmBot
{
    public class TaskManager
    {
        static async Task WriteFileAsync(string path, string content)
        {
            Console.WriteLine("Async Write File has started");
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                await outputFile.WriteAsync(content);
            }
            Console.WriteLine("Async Write File has completed");
        }
        public static async Task LoadTasks(string[] args, string taskPath)
        {
            var opt = LoadOption(args);
            DealFinder finder = new DealFinder(opt);
            DealPublisher publisher = new DealPublisher(opt);
            if (!publisher.Login()) return;
            publisher.Punch();
            publisher.driver.Navigate().GoToUrl(@"https://www.smzdm.com/baoliao/?old");
            while (publisher ==  null || publisher.baoLiaoLeft != 0)
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
                var pageUrl =  urlroot + task.Item2.ToString() + "/";
                Run(opt, pageUrl, finder, publisher);
            }
            finder.driver.Quit();
            publisher.NewLike();
            publisher.TransferGoldAndLogStatus();
            if (publisher.gold > 1)
            {
                Console.WriteLine(opt.username + " gold=" + publisher.gold);
                //helper.TransferGold("https://post.smzdm.com/p/amm539rz/");
            }
            else
            {
                publisher.driver.Quit();
            }
        }
        static string ProcessTask(string content, out Tuple<string, int> task)
        {
            var lines = content.Split('\n').ToList();
            //Console.WriteLine("Read " + status);
            var orderText = lines[0];
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
                pageNumber = 50;//start
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
        static void Run(Option option, string pageUrl, DealFinder dealFinder, DealPublisher publisher)
        {


            var smzdmItemList = new List<Dictionary<string, string>>();
            dealFinder.driver.Navigate().GoToUrl(pageUrl);
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
                var price = dealFinder.CheckSmzdmItem(it);
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
            //worker.TransferGoldAndLogStatus();
            //if (worker.gold > 1)
            //{
            //    Console.WriteLine(option.username + " gold=" + worker.gold);
            //    //helper.TransferGold("https://post.smzdm.com/p/amm539rz/");
            //}
            //else
            //{
            //    worker.Shutdown();
            //}
            //return;
        }
    }
}
