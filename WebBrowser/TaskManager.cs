using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public static async Task Start(string[] args)
        {
            Console.WriteLine(string.Join("\n", args));
            var opt = LoadOption(args[0]);
            var taskPath = args[1];
            var commandPath = args[2];
            var statusPath = args[3];
            //var commandPath = args[4];
            Console.Title = opt.username;
            DealPublisher publisher = new DealPublisher(opt);

            if (opt.Mode == "login")
            {
                publisher.Login();
            }
            else if (opt.Mode == "smzdm_share")
            {
                publisher.Login();
                await Share(publisher, opt, taskPath, statusPath);
                Finish(publisher, opt, taskPath, statusPath);
            }
            else if(opt.Mode == "auto")
            {
                publisher.Login();
                while (true)
                {
                    await Share(publisher, opt, taskPath, statusPath);
                    Finish(publisher, opt, taskPath, statusPath);
                    await ReadCommand(commandPath, opt);
                    if (opt.Command == "stop")
                    {
                        Console.WriteLine("Stop");
                        break;
                    }
                    Console.WriteLine(DateTime.Now.ToString());
                    await Task.Delay(opt.Freq*1000);
                }
            }
            //Finish(publisher, opt, statusPath);
            publisher.driver.Quit();
        }
        static async Task ReadCommand(string commandPath, Option opt)
        {
            string content = await Helper.OpenFile(commandPath);
            Console.WriteLine(content);
            dynamic job = JObject.Parse(content);
            var freq = job.freq.ToObject<int>();
            opt.Freq = freq;
            //var runList = job.runList.ToObject<string>().Split(',').ToList();
            List<string> stopList = new List<string>(job.stopList.ToObject<string[]>());
            List<string> payList = new List<string>(job.payList.ToObject<string[]>());
            List<string> likeList = new List<string>(job.likeList.ToObject<string[]>());
            bool modified = false; 
            if (stopList.Contains(opt.username))
            {
                opt.Command = "stop";
                stopList.Remove(opt.username);
                modified = true;
            }
            else if (payList.Contains(opt.username))
            {
                opt.Command = "pay";
                payList.Remove(opt.username);
                modified = true;
            }
            else if (likeList.Contains(opt.username))
            {
                opt.Command = "like";
                likeList.Remove(opt.username);
                modified = true;
            }
            if (modified)
            {
                job.stopList = JArray.FromObject(stopList);
                job.payList = JArray.FromObject(payList);
                job.likeList = JArray.FromObject(likeList);
                await WriteFileAsync(commandPath, job.ToString());
            }
        }
        static async Task GetPayee(string taskPath, Option opt)
        {
            String content;
            using (StreamReader reader = File.OpenText(taskPath))
            {
                content = await reader.ReadToEndAsync();
            }
            dynamic job = JObject.Parse(content);
            opt.Payee = job.payee.ToObject<string>();
            Console.WriteLine(opt.Payee);
        }
        static void Finish(DealPublisher publisher, Option opt, string taskPath, string statusPath)
        {
            try
            {
                publisher.Follow();
                publisher.Punch();
                publisher.Like();
                //Console.WriteLine(opt.Payee);
                TransferGold(publisher, taskPath, opt);
            }
            catch (AggregateException e)
            {
                MyLogger.LogWarnning(e.Message);
                MyLogger.LogWarnning(e.InnerException.Message);
            }
            catch (Exception e)
            {
                MyLogger.LogWarnning(e.InnerException.Message);
            }
        }
        static async Task Share(DealPublisher publisher, Option opt, string taskPath, string statusPath)
        {
            publisher.driver.Navigate().GoToUrl(@"https://www.smzdm.com/baoliao/?old");
            Thread.Sleep(5000);
            publisher.ReadInfo();
            if (publisher.baoLiaoLeft != 0)
            {
                DealFinder finder = new DealFinder(opt);
                while (publisher.baoLiaoLeft != 0)
                {

                    String content;
                    using (StreamReader reader = File.OpenText(taskPath))
                    {
                        content = await reader.ReadToEndAsync();
                    }
                    content = ProcessTask(content, opt, out Tuple<string, int> task);
                    await WriteFileAsync(taskPath, content);
                    Console.WriteLine("Write done.");
                    opt.HotPickCategory = task.Item1;
                    Console.WriteLine(opt.HotPickCategory);
                    Console.Title = opt.username + " " + task.Item1 + task.Item2 + " " + publisher.baoLiaoLeft;
                    var urlroot = opt.ConvertHotPickCategory(opt.HotPickCategory);
                    if (urlroot == null) return;
                    string pageUrl = "";
                    if (opt.HotPickCategory == "office")
                    {
                        pageUrl = urlroot + task.Item2.ToString() + "/#feed-main/";
                    }
                    else
                    {
                        pageUrl = urlroot + task.Item2.ToString() + "/";
                    }
                    var exceptionCount = 0;
                    while (exceptionCount < 5)
                    {
                        try
                        {
                            FindDealAndPublish(opt, pageUrl, finder, publisher);
                            break;
                        }
                        catch (Exception e1)
                        {
                            MyLogger.LogWarnning(e1.Message);
                            exceptionCount += 1;
                        }
                    }
                    if (exceptionCount >= 5)
                    {
                        break;
                    }
                }
                finder.driver.Quit();

            }
        }
        static Account CreateAccount(Option opt)
        {
            var account = new Account();
            if (opt.username.Contains('@'))
            {
                account.email = opt.username;
            }
            else
            {
                account.phone = opt.username;
            }
            account.Level = '?';
            account.GoldLeft = '?';
            account.BaoLiaoLeftCount = -2;
            return account;
        }
        static void TransferGold(DealPublisher publisher, string taskPath, Option opt)
        {
            try
            {
                GetPayee(taskPath, opt).Wait();
                publisher.TransferGold2(opt.Payee);
            }
            catch (NoSuchElementException e)
            {
                MyLogger.LogWarnning(e.Message);
            }
            catch(NullReferenceException e)
            {
                MyLogger.LogWarnning(e.Message);
            }
        }
        public static string ProcessTask(string content, Option opt, out Tuple<string, int> task)
        {
            dynamic job = JObject.Parse(content);
            Console.WriteLine("Input");
            Console.WriteLine(job);
            var order = job.topic.ToObject<string[]>();
            var topicOrder = new List<string>();
            var pageStopCondition = new Dictionary<string, int>();
            opt.Payee = job.payee.ToObject<string>();
            foreach(var topic in order)
            {
                if (topic.Contains("<"))
                {
                    var token = topic.Split('<');
                    topicOrder.Add(token[0]);
                    pageStopCondition.Add(token[0], int.Parse(token[1]));
                }
                else
                {
                    topicOrder.Add(topic);
                }
            }
            var index = job.index.ToObject<int>();
            var page = job.page.ToObject<int>();
            var start = job.start.ToObject<int>();
            var end = job.end.ToObject<int>();
            opt.PriceRate = job.priceRate.ToObject<double>();
            opt.Payee = job.payee.ToObject<string>();
            while (pageStopCondition.ContainsKey(topicOrder[index]) && pageStopCondition[topicOrder[index]]<page)
            {
                index += 1;
            }
            task = new Tuple<string, int>(topicOrder[index], page);
            Console.WriteLine("Task");
            Console.WriteLine(task.Item1 + " " + task.Item2);
            index += 1;
            
            if (index >= topicOrder.Count)
            {
                index = 0;
                page += 1;
            }
            if (page >= end)//end 
            {
                page = start;//start
            }
            job.index = index;
            job.page = page;
            Console.WriteLine("Output");
            Console.WriteLine(job.ToString());
            return job.ToString();
        }
        static Option LoadOption(string optionPath)
        {
            var arguments = File.ReadAllText(optionPath);
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
                    var continueSubmit = publisher.PasteItemUrl(sourceUrl, 0, option.waitBaoliao, option.baoLiaoStopNumber);
                    if (continueSubmit)
                    {
                        Console.WriteLine("Continue to submit.");
                        publisher.SubmitBaoLiao(option.descriptionMode, goodPrice, 0.0, sourceUrl, option.PriceRate, 0.0, price);
                    }
                    if(publisher.baoLiaoLeft == 0)
                    {
                        Console.WriteLine("BaoLiao left 0, break.");
                        break;
                    }
                    Console.WriteLine("Move to next item");
                }
            }

            Console.WriteLine(option.HotPickCategory + " " + pageUrl + " Task Finished");
            Console.WriteLine("baoliao left " + publisher.baoLiaoLeft);
        }
    }
}
