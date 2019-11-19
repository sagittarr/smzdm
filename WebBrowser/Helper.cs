using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmzdmBot
{
    public class Option
    {
        public string username = "";
        public string password = "";
        public string sourcePath = "";
        public int itemLinkOrder = 0;
        public int waitBaoliao = 0;
        public int descriptionMode = 0;
        public int baoLiaoStopNumber = 0;
        public string CustomDescriptionPrefix = "";
        public string output = "";
        public string input = "";
        public int CrawlCount = 50;
        public string browser = "firefox";
        public string pageNumbers = "500,502,504";
        public string SmzdmWikiPages { get; set; }
        public double PriceRate = 1.1;
        public Option()
        {

        }
        public Option(string[] args)
        {
            username = args[0];
            password = args[1];
            sourcePath = args[2];
            itemLinkOrder = int.Parse(args[3]);
            waitBaoliao = int.Parse(args[4]);
            descriptionMode = int.Parse(args[5]);
            baoLiaoStopNumber = int.Parse(args[6]);
            CustomDescriptionPrefix = args[7];
            output = args[8];
        }
    }
    public class Helper
    {
        public static bool ToUrl(IWebDriver driver, string url)
        {
            try
            {
                driver.Navigate().GoToUrl(url);
            }
            catch (WebDriverException e)
            {
                try
                {
                    driver.Navigate().Refresh();
                }
                catch (WebDriverException e1)
                {
                    Console.WriteLine("Web Exception skip " + url);
                    return false;
                }
            }
            return true;
        }
        public static string ParseDigits(string input)
        {
            return new string(input.Where(x => x == '.' || Char.IsDigit(x)).ToArray());
        }
        public static List<E> ShuffleList<E>(List<E> inputList)
        {
            List<E> randomList = new List<E>();

            Random r = new Random();
            int randomIndex = 0;
            while (inputList.Count > 0)
            {
                randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            }
            return randomList; //return the new random list
        }
        public static string ParseShoppingPlatform(string url)
        {
            if (url.StartsWith("https://product.suning.com/") || url.StartsWith("http://product.suning.com/"))
            {
                return "苏宁易购";
            }
            else if(url.StartsWith(@"https://item.jd.com/") || url.StartsWith(@"https://item.jd.hk/") || url.StartsWith(@"https://re.jd.com/"))
            {
                return "京东";
            }
            else if (url.StartsWith("https://goods.kaola.com/product/"))
            {
                return "考拉海购";
            }
            else if (url.StartsWith("https://www.xiaomiyoupin.com/"))
            {
                return "小米有品";
            }
            else if (url.StartsWith("https://world.tmall.com/item/") || url.StartsWith("https://detail.tmall.com/item.htm?"))
            {
                return "天猫";
            }
            else if (url.StartsWith("https://mobile.yangkeduo.com/"))
            {
                return "拼多多";
            }
            return "";
        }
        public static string CheckUrl(string url)
        {
            if (url == null) return "";
            if (url.StartsWith("https://product.suning.com/") || url.StartsWith("http://product.suning.com/"))
            {
                var index = url.IndexOf(".html");
                if (index != -1)
                {
                    return url.Substring(0, index) + ".html";
                }
            }
            else if ((url.StartsWith(@"https://item.jd.com/") || url.StartsWith(@"https://item.jd.hk/") || url.StartsWith("https://re.jd.com/")) && !url.EndsWith("comment"))
            {
                return url;
            }
            else if ((url.StartsWith(@"//item.jd.com/") || url.StartsWith(@"//item.jd.hk/") || url.StartsWith(@"//re.jd.com/")) && !url.EndsWith("comment"))
            {
                return url.Replace("//", "");
            }
            else if (url.StartsWith("https://goods.kaola.com/product/"))
            {
                var index = url.IndexOf(".html");
                if (index != -1)
                {
                    return url.Substring(0, index) + ".html";
                }
                return url;
            }
            else if (url.StartsWith("//goods.kaola.com/product/"))
            {
                var index = url.IndexOf(".html");
                if (index != -1)
                {
                    return url.Substring(0, index) + ".html";
                }
                return "https:" + url;
            }
            else if (url.StartsWith("https://www.xiaomiyoupin.com/detail?gid="))
            {
                var tokens = url.Split('&');
                if (tokens.Length == 2)
                {
                    return tokens[0];
                }
            }
            else if (url.StartsWith("https://world.tmall.com/item/"))
            {
                var tokens = url.Split('?');
                if (tokens.Length == 2)
                {
                    return tokens[0];
                }
            }
            else if (url.StartsWith("https://detail.tmall.com/item.htm?"))
            {
                url = url.Substring("https://detail.tmall.com/item.htm?".Length);
                var tokens = url.Split('&');
                foreach (var t in tokens)
                {
                    if (t.StartsWith("id="))
                    {
                        return "https://detail.tmall.com/item.htm?" + t;
                    }
                }
            }
            return "";
        }
    }
}
