using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public string StatusFilePath = "";
        public int CrawlCount = 50;
        public string browser = "firefox";
        public string pageNumbers = "500,502,504";
        public string HotPickCategory = "";
        public string SmzdmWikiPages { get; set; }
        public double PriceRate = 1.1;
        public string Browser = "firefox";
        public string GoldTransferTarget { get; set; }
        private static Dictionary<string, string>  HotPickCategoryMap = new Dictionary<string, string>();
        public Option()
        {

        }
        public Option(Account account)
        {
            if (!String.IsNullOrWhiteSpace(account.email))
            {
                username = account.email;
            }
            else
            {
                username = account.phone;
            }
            password = account.password;
            sourcePath = account.deal;
            itemLinkOrder = account.order;
            waitBaoliao = account.waitTime;
            descriptionMode = account.descriptionMode;
            baoLiaoStopNumber = account.limit;
            CustomDescriptionPrefix = account.customDespPrefix;
            output = account.output;
            StatusFilePath = account.StatusFilePath;
            pageNumbers = account.pages;
            PriceRate = account.discountRate;
            HotPickCategory = account.category;
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
        public string ConvertHotPickCategory(string name)
        {
            if(HotPickCategoryMap.Count == 0)
            {
                HotPickCategoryMap.Add("computers", "s0f163t0b0d0r0p");
                HotPickCategoryMap.Add("ele", "s0f27t0b0d0r0p");
                HotPickCategoryMap.Add("sports", "s0f191t0b0d0r0p");
                HotPickCategoryMap.Add("beauty", "s0f113t0b0d0r0p");
                HotPickCategoryMap.Add("mother", "s0f75t0b0d0r0p");
                HotPickCategoryMap.Add("home", "s0f37t0b0d0r0p");
                HotPickCategoryMap.Add("things","s0f1515t0b0d0r0p");
            }
            return HotPickCategoryMap[name];
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
        public static T RandomSelect<T>(List<T> list)
        {
            var random = new Random();
            int index = random.Next(list.Count);
            return list[index];
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
