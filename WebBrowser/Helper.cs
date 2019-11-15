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

        public static string CheckUrl(string url)
        {
            if (url == null) return "";
            if (url.StartsWith("https://product.suning.com/"))
            {
                var index = url.IndexOf(".html");
                if (index != -1)
                {
                    return url.Substring(0, index) + ".html";
                }
            }
            else if ((url.StartsWith(@"https://item.jd.com/") || url.StartsWith(@"https://item.jd.hk/")) && !url.EndsWith("comment"))
            {
                return url;
            }
            else if ((url.StartsWith(@"//item.jd.com/") || url.StartsWith(@"//item.jd.hk/")) && !url.EndsWith("comment"))
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
