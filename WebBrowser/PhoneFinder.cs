//using HtmlAgilityPack;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Firefox;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Threading.Tasks;

//namespace WebBrowser
//{
//    public class PhoneFinder
//    {
//        public static void Main(string[] args)
//        {
//            IWebDriver driver = new FirefoxDriver();
//            List<string> phones = new List<string>();
//            var fp = @"D:\smzdm\sms2.txt";
//            if (File.Exists(fp))
//            {
//                var lines = File.ReadAllLines(fp);
//                foreach (var l in lines)
//                {
//                    phones.Add(l);
//                }
//                foreach (var p in phones)
//                {
//                    TryLogin(driver, p.Trim());
//                }
//                Console.ReadKey();
//            }
//        }
//        public static string GetVCode(string phone)
//        {
//            HtmlWeb web = new HtmlWeb();
//            //https://yunduanxin.net/info/86
//            //"
//            HtmlDocument doc = web.Load("https://www.yinsixiaohao.com/message/" + phone + ".html");
//            var text = doc.DocumentNode.InnerText;

//            var index = text.IndexOf("什么值得买");
//            if (index != -1)
//            {
//                var code = text.Substring(index, 30);
//                Console.WriteLine(code);
//                code = new String(code.Where(c => Char.IsDigit(c)).ToArray());
//                if (code.Length == 6)
//                {
//                    Console.WriteLine(code);
//                    return code;
//                }
//            }
//            Console.WriteLine("code not found");
//            return "";
//        }
//        public static bool TryLogin(IWebDriver driver, string phone)
//        {
//            //IWebDriver driver = new FirefoxDriver();
//            driver.Navigate().GoToUrl("https://zhiyou.smzdm.com/user/login/quick/0/?redirect_to=http%3A%2F%2Fzhiyou.smzdm.com%2Fuser");
//            int foundCountDown = 3;
//            while (foundCountDown>0)
//            {
//                Thread.Sleep(3000);
//                try
//                {
//                    driver.FindElement(By.Id("mobile")).SendKeys(phone); //"18604568194" smzdm24202@outlook.com
//                    break;
//                }
//                catch (NoSuchElementException)
//                {
//                    Console.WriteLine("Wait redirect..." + foundCountDown--);
//                }
//            }

//            driver.FindElement(By.Id("login_mobile_code")).Click();
//            Console.WriteLine("Wait 20s");
//            Thread.Sleep(20000);
//            var code = GetVCode(phone);
//            if (code != "")
//            {
//                driver.FindElement(By.Id("mobile_code")).SendKeys(code);
//                driver.FindElement(By.Id("login_submit")).Click();
//                var cmd = Console.ReadLine();
//                if (cmd.Contains("1"))
//                {
//                    Console.WriteLine("G " + phone);
//                    driver.Navigate().GoToUrl("https://zhiyou.smzdm.com/user/logout/");
//                    return true;
//                }
//            }
//            Console.WriteLine("X " + phone);
//            return false;
//        }
//    }
//}


