using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace WebBrowser
{
    public class Option
    {
        public string username = "";
        public string password = "";
        public string sourceUrl = "";
        public int itemLinkOrder = 0;
        public int waitBaoliao = 0;
        public int descriptionMode = 0;
        public int baoLiaoStopNumber = 0;
        public string CustomDescriptionPrefix = "";
        public string output = "";
        public Option()
        {

        }
        public Option(string[] args)
        {
            username = args[0];
            password = args[1];
            sourceUrl = args[2];
            itemLinkOrder = int.Parse(args[3]);
            waitBaoliao = int.Parse(args[4]);
            descriptionMode = int.Parse(args[5]);
            baoLiaoStopNumber = int.Parse(args[6]);
            CustomDescriptionPrefix = args[7];
            output = args[8];
        }
    }
    class Program
    {
        //public static int Counter = 0;
        public static int Level = -1;
        public static int BaoLiaoLeft = -1;
        public static string nickName = "";
        public static int gold = -1;
        public static Option option = new Option();

        //public static string CustomPrefix = "";
        static string CheckUrl(string url)
        {
            if (url.StartsWith("https://product.suning.com/"))
            {
                var index = url.IndexOf(".html");
                if (index != -1)
                {
                    return url.Substring(0,index) + ".html";
                }
            }
            if ((url.StartsWith(@"https://item.jd.com/") || url.StartsWith(@"https://item.jd.hk/")) && !url.EndsWith("comment"))
            {
                return url;
            }
            if ((url.StartsWith(@"//item.jd.com/") || url.StartsWith(@"//item.jd.hk/")) && !url.EndsWith("comment"))
            {
                return url.Replace("//", "");
            }
            if (url.StartsWith("https://goods.kaola.com/product/"))
            {
                var index = url.IndexOf(".html");
                if (index != -1)
                {
                    return url.Substring(0, index) + ".html";
                }
                return url;
            }
            if (url.StartsWith("//goods.kaola.com/product/"))
            {
                var index = url.IndexOf(".html");
                if (index != -1)
                {
                    return url.Substring(0, index) + ".html";
                }
                return "https:" + url;
            }
            return "";
        }
        static void Main(string[] args)
        {
            string mode = "";
            if (args.Length == 1)
            {
                Console.WriteLine("Try to read arguments from file " + args[0]);
                if (File.Exists(args[0]))
                {
                    var lines = File.ReadAllText(args[0]);
                    option = JsonConvert.DeserializeObject<Option>(lines);
                    mode = "baoliao";
                }
                else
                {
                    Console.WriteLine(args[0] + " file does not exist.");
                    return;
                }
            }
            else if(args.Length == 3)
            {
                option.username = args[0];
                option.password = args[1];
                option.output = args[2];
                mode = "login";
            }
            else if(args.Length == 9)
            {
                option = new Option(args);
                mode = "baoliao";
            }
            else
            {
                Console.WriteLine("Please provide argument through file.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine(JsonConvert.SerializeObject(option));
            Console.WriteLine("Type any key to continue");
            Console.ReadKey();

            IWebDriver driver = new FirefoxDriver();
            try
            {
                if (mode == "baoliao")
                {
                    var list = new List<string>();
                    var _list = new List<string>();
                    if (option.sourceUrl.StartsWith("http")) {
                        list = getGoodsItemListByDriver(driver, option.sourceUrl);
                    }
                    else
                    {
                        var lines = File.ReadAllLines(option.sourceUrl);
                        _list.AddRange(lines);
                        foreach( var url in _list)
                        {
                            var u = CheckUrl(url);
                            if (!string.IsNullOrWhiteSpace(u))
                            {
                                list.Add(u);
                            }
                        }
                    }
                    list = list.Distinct().ToList().Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    foreach (var item in list)
                    {
                        Console.WriteLine(item);
                    }
                    if (option.itemLinkOrder == 1)
                    {
                        list.Reverse();
                    }
                    Console.WriteLine("Total item count " + list.Count);
                    Console.WriteLine("Type any key to continue");
                    Console.ReadKey();

                    if (!Login(driver))
                    {
                        return;
                    }
                    int i = 0;
                    while (i < list.Count)
                    {
                        var code = 0;
                        while (0 == code && i < list.Count)
                        {
                            code = PasteItemUrl(driver, list[i], i, option.waitBaoliao, option.baoLiaoStopNumber);
                            i++;
                        }
                        if (code == 2) break;
                        SubmitBaoLiao(driver, option.descriptionMode);
                    }
                    Console.WriteLine("Finished.");
                }
                else if (mode == "login")
                {
                    if (!Login(driver))
                    {
                        return;
                    }
                    ReadInfo(driver);
                }
                else
                {
                    Console.WriteLine("unknown mode " + mode);

                    return;
                }
            }
            catch(Exception e)
            {
                if (File.Exists(option.output))
                {
                    File.AppendAllText(option.output, "username:" + option.username + "," + "error message:" + e.Message.Replace(","," ") + "\n");
                    Console.WriteLine(e.Message);
                }
                else
                {
                    Console.WriteLine(option.output + " file does not exist.");
                    return;
                }
                return;
            }

            OutputStatus(driver);
            if (gold > 1)
            {
                Console.WriteLine(option.username + " gold=" + gold);
            }
            else
            {
                driver.Close();
            }
            return;
        }
        public static void OutputStatus(IWebDriver driver)
        {
            var status = getStatus(driver);
            Console.WriteLine(status);
            var output = option.output;
            
            var arr = status.Replace("\r", "").Split('\n');
            for (int i= 0; i<arr.Length; i++)
            {
                //Console.WriteLine(a);
                
                if (arr[i].Contains("金币") && i>0)
                {
                    var num = new string(arr[i-1].Where(x => Char.IsDigit(x)).ToArray());
                    Console.WriteLine("gold " + num);
                    if (!string.IsNullOrWhiteSpace(num))
                    {
                        gold = int.Parse(num);
                    }
                }
            }
            if (File.Exists(output))
            {
                File.AppendAllText(output, "username:"+option.username + ","  + "nickName:" + nickName+ ",level:"+ Level + ",gold:" + gold + ",left:" + BaoLiaoLeft + ",loginTime:" + DateTime.Now +",status:"+ status.Replace("\r", "").Replace("\n", "") +"\n");
            }
            else
            {
                Console.WriteLine(output + " file does not exist.");
                return;
            }
        }
        public static bool Login(IWebDriver driver)
        {
            driver.Navigate().GoToUrl(@"https://www.smzdm.com/baoliao/?old");
            driver.Navigate().Refresh();
            driver.FindElement(By.Id("username")).SendKeys(option.username); //"18604568194" smzdm24202@outlook.com
            driver.FindElement(By.Id("password")).SendKeys(option.password);

            var login = driver.FindElement(By.Id("login_submit"));
            if (login != null)
            {
                login.Click();
            }
            Console.WriteLine("Type Y to continue, after human check.");
            if (Console.ReadLine().ToLower().Contains("y"))
            {
                return true;
            }
            return false;
        }


        public static bool SubmitBaoLiao(IWebDriver driver, int despMode)
        {
            var price = driver.FindElement(By.Id("un-item-price")).GetAttribute("value");
            var name = driver.FindElement(By.Id("un-item-goods")).GetAttribute("value");
            var brand = driver.FindElement(By.Id("un-item-brand")).GetAttribute("value");

            if (!String.IsNullOrWhiteSpace(price) && !String.IsNullOrWhiteSpace(name) && !String.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("price " + price);
                var desp = driver.FindElement(By.Id("un-content-price"));
                if (desp != null)
                {

                    if (despMode == 1)
                    {
                        desp.SendKeys("京东秒杀价" + " 只要" + price + "!");
                    }
                    else if (despMode == 0)
                    {
                        desp.SendKeys(name + " 只要" + price + "!");
                    }
                    else
                    {
                        desp.SendKeys(option.CustomDescriptionPrefix + " 只要" + price + "!");
                    }

                }
                Console.WriteLine("Continue to submit");
                //Console.ReadKey();
                //driver.FindElement(By.Id("un-feedback-submit")).Submit();
                try
                {
                    driver.FindElement(By.Id("un-feedback-submit")).Click();
                }
                catch (Exception)
                {
                    return true;
                }
                Console.WriteLine("Sleep for 5 seconds.");
                Thread.Sleep(5000);
                return true;
            }
            return true;
        }

        private static bool CheckFrequecyNotice(IWebDriver driver)
        {
            var fails = driver.FindElements(By.ClassName("layerSubInfo"));
            if (fails != null)
            {
                foreach (var f in fails)
                {
                    if (f.Text.Contains("分钟"))
                    {
                        Console.WriteLine("Too frequent submit, Sleep for 70 seconds.");
                        var count = 70;
                        while (count > 0)
                        {
                            Thread.Sleep(10000);
                            count = count - 10;
                            Console.WriteLine(count);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        private static bool CheckWarnNotice(IWebDriver driver)
        {
            var noticeDisplayed = false;
            try
            {
                var red_font = driver.FindElement(By.ClassName("red-font"));
                Console.WriteLine(red_font.GetAttribute("value"));
                Console.WriteLine("T" + red_font.Text);
                noticeDisplayed = true;
            }
            catch (NoSuchElementException e)
            {

                noticeDisplayed = false;
            }
            return noticeDisplayed;
        }
        private static bool CheckForm(IWebDriver driver)
        {
            try
            {
                var v1 = !String.IsNullOrEmpty(driver.FindElement(By.Id("un-item-price")).GetAttribute("value"));
                var v2 = !String.IsNullOrEmpty(driver.FindElement(By.Id("un-item-goods")).GetAttribute("value"));
                var v3 = !String.IsNullOrEmpty(driver.FindElement(By.Id("un-item-brand")).GetAttribute("value"));
                if (v1 && v2 && v3) return true;
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine("form not render well");
                return false;
            }
            return false;
        }
        
        private static int ReadInfo(IWebDriver driver)
        {
            var notice = driver.FindElement(By.Id("bhNotice")).Text;
            if (!String.IsNullOrWhiteSpace(notice))
            {
                Console.WriteLine(notice);
                var ns = notice.Split('，');
                if (ns.Length == 2)
                {
                    var level = new string(ns[0].Where(c => Char.IsDigit(c)).ToArray());
                    var baoLiaoLeft = new string(ns[1].Where(c => Char.IsDigit(c)).ToArray());
                    if (level != null && baoLiaoLeft != null)
                    {
                        Level = int.Parse(level);
                        //baoLiaoLeft = int.Parse(baoLiaoLeft);
                        //Console.WriteLine("Level " + level + " Bao Liao left " + baoLiaoLeft);
                        BaoLiaoLeft = int.Parse(baoLiaoLeft);
                        return BaoLiaoLeft;
                    }
                }
            }
            return -1;
        }
        //return code
        //0=warn notice=>next item
        //1=looks good=> submit
        //2=stop and quit
        public static int PasteItemUrl(IWebDriver driver, string url, int index, int timeWait, int stopNumber)
        {
            if (BaoLiaoLeft != -1 && BaoLiaoLeft <= stopNumber) return 2;
            driver.Navigate().GoToUrl(@"https://www.smzdm.com/baoliao/?old");
            Console.WriteLine("Pasting " + index + " " + url);
            driver.FindElement(By.Name("item_link")).SendKeys(url); //item.jd.com/100005093980.html
            driver.FindElement(By.Id("get_info_btn")).Click();

            int countDown = 3;
            while (countDown > 0)
            {
                if (CheckFrequecyNotice(driver))
                {
                    return 0;
                }
                Console.WriteLine("Sleep for " + timeWait + " seconds.");
                Thread.Sleep(timeWait * 1000);
                //var pop2 = driver.FindElement(By.Id("pop2"));
                int left = ReadInfo(driver);
                if (left != -1 && left <= option.baoLiaoStopNumber)
                {
                    Console.WriteLine("Quit loop");
                    return 2;
                }
                else if (CheckWarnNotice(driver))
                {
                    Console.WriteLine("Notice catched");
                    return 0;
                }
                else if (CheckForm(driver))
                {
                    Console.WriteLine("looks good");
                    return 1; // form rendered succuss
                }
                else
                {
                    countDown--; // keep wait
                }
            }
            return 0; // next item
        }

        public static string getStatus(IWebDriver driver)
        {
            driver.Navigate().GoToUrl("https://zhiyou.smzdm.com/user/");
            nickName = driver.FindElement(By.ClassName("info-stuff-nickname")).Text;
            return driver.FindElement(By.ClassName("info-stuff-assets")).Text;
        }

        private static List<string> getGoodsItemListByDriver(IWebDriver driver, string url, int retry=10)
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
                    var item = CheckJDHref(href);
                    if (item != "") res.Add(item);
                    item = CheckKaoLaHref(href);
                    if (item != "") res.Add(item);
                }
            }
            return res.Distinct().ToList();
        }
        private static string CheckJDHref(string href)
        {
            int index = href.IndexOf(".html");
            if (index > 0)
            {
                href = href.Substring(0, index) + ".html";
            }
            if (href == null) return "";
            if (href.StartsWith("https://item.jd.com/"))
            {
                return href.Replace("https://", "");
            }
            //item.jd.com/5861956.html
            if ((href.StartsWith(@"//item.jd.com/") || href.StartsWith(@"//item.jd.hk/")))
            {
                return href.Replace("//", "");
            }
            return "";
        }

        private static string CheckKaoLaHref(string href)
        {
            if (href == null) return "";
            int index = href.IndexOf(".html");
            if (index > 0)
            {
                href = href.Substring(0, index) + ".html";
            }
            if (href.StartsWith("https://goods.kaola.com/product/"))
            {
                return href.Replace("https://", "");
            }
            //item.jd.com/5861956.html
            if ((href.StartsWith(@"//goods.kaola.com/product/")))
            {
                return href.Replace("//", "");
            }
            return "";
        }
        private static List<string> getJDItemList(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            var list = new List<string>();
            var a = doc.DocumentNode.Descendants().ToList();

            foreach (var node in doc.DocumentNode.Descendants())
            {
                Console.WriteLine(node.InnerHtml);
                var href = node.Attributes["href"];
                if (node.Name == "a" && href != null && href.Value != null)
                {

                    if (href.Value.StartsWith("https://item.jd.com/"))
                    {
                        list.Add(href.Value.Replace("https://", ""));
                    }
                    //item.jd.com/5861956.html
                    if ((href.Value.StartsWith(@"//item.jd.com/") || href.Value.StartsWith(@"//item.jd.hk/")) && !href.Value.EndsWith("comment"))
                    {
                        list.Add(href.Value.Replace("//", ""));
                    }
                }
            }
            return list.Distinct().ToList();
        }
    }
}
