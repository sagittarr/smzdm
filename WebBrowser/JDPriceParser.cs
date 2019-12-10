using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmzdmBot
{
    public class JDPriceParser
    {
        //public static string ParseDigits(string input)
        //{
        //    return new string(input.Where(x => x == '.' || Char.IsDigit(x)).ToArray());
        //}
        //public static string  GenerateDescription(Price price, string url){
        //    if (price.oldPrice != 0 && price.finalPrice != 0 && price.finalPrice < price.oldPrice)
        //    {
        //        Console.WriteLine("~~~~~~~GoodPrice " + price.oldPrice + " " + price.finalPrice);
        //        return "原价" + (int)price.oldPrice + "元，预计到手价" + (int)price.finalPrice + "元";
        //        //File.AppendAllText(@"D:\test.txt", "~~~~~~~GoodPrice " + price.oldPrice + " " + price.finalPrice + "\n");
        //    }
        //    else if (price.retainage > 0 && price.deposit > 0)
        //    {
        //        Console.WriteLine("~~~~~~~GoodPrice " + price.finalPrice);
        //        //File.AppendAllText(@"D:\test.txt", "~~~~~~~GoodPrice " + price.finalPrice + "\n");
        //    }
        //}
        public static Price ExtractPrice(IWebDriver driver)
        {
            //if (driver.Url.StartsWith("https://item.jd.com/"))
            //{
            var price = new Price();
            var info = driver.FindElement(By.ClassName("itemInfo-wrap")).Text;
            info = info.Replace("\r", "");
            Console.WriteLine("info\n" + info);
            //File.WriteAllText(@"D:\test.txt", info);
            var lines = info.Split('\n');
            for(int i =0; i< lines.Length; i++)
            {
                if (lines[i] == "京 东 价" || lines[i] == "秒 杀 价" || lines[i] == "闪 购 价")
                {
                    if (i + 1 < lines.Length)
                    {
                        var priceText = lines[i + 1];
                        var parts = priceText.Split('[');
                        if(parts.Length == 2)
                        {
                            var current = Helper.ParseDigits(parts[0]);
                            var reference = Helper.ParseDigits(parts[1]);
                            price.currentPrice = Double.Parse(current);
                            Console.WriteLine("current " + current + " vs reference " + reference);
                        }
                        else if(parts.Length == 1)
                        {
                            var current = Helper.ParseDigits(parts[0]);
                            price.currentPrice = Double.Parse(current);
                            Console.WriteLine("current " + current );
                        }
                    }
                   
                }
                if (lines[i].StartsWith("满减"))
                {
                    var idex = lines[i].IndexOf("元减");
                    if (idex > 0)
                    {
                        var part1 = lines[i].Substring(0, idex);
                        var part2 = lines[i].Substring(idex);
                        var condition = Helper.ParseDigits(part1);
                        var cut = Helper.ParseDigits(part2);
                        price.PromoteNote = "满" + condition + "减" + cut;
                        price.coupons.Add(new List<double>() { Double.Parse(condition), Double.Parse(cut) });
                        Console.WriteLine("condition " + condition + " vs cut " + cut);
                    }
                }
            }
            var storeName = "";
            foreach (var element in driver.FindElements(By.ClassName("item")))
            {
                if (element.FindElements(By.ClassName("name")).Count == 1)
                {
                    storeName = element.FindElement(By.ClassName("name")).Text;
                    price.storeName = storeName;
                    Console.WriteLine("storeName:" + storeName);
                }

            }
            if (driver.FindElements(By.Id("p-ad")).Count == 1)
            {
                var note = driver.FindElement(By.Id("p-ad")).Text;
                Console.WriteLine("note\n" + note);
                var idx = note.IndexOf("立减");
                if (idx != -1)
                {
                    var text = note.Substring(idx);
                    var idx2 = text.IndexOf("元");
                    if (idx2 != -1)
                    {
                        var cutText = text.Substring(0, idx2);
                        var cut = Helper.ParseDigits(cutText);
                        price.Cut = Double.Parse(cut);
                        Console.WriteLine("promote cut\n" + cut);
                    }
                }
                idx = note.IndexOf("12.12");
                if (idx != -1)
                {
                    price.Notes.Add("双十二特惠");
                }
                //idx = note.IndexOf("保价");
                //if (idx != -1)
                //{
                //    var part = note.Substring(idx);
                //    var idx2 = part.IndexOf("天");
                //    if (idx2 != -1)
                //    {
                //        var part2 = part.Substring(0, idx2);

                //    }
                //}
            }
            if (driver.FindElements(By.ClassName("activity-message")).Count == 1)
            {
                var note = driver.FindElement(By.ClassName("activity-message")).Text;
                Console.WriteLine("act\n" + note);
                //File.AppendAllText(@"D:\test.txt", note);
            }
            if (driver.FindElements(By.ClassName("p-parameter")).Count == 1)
            {
                var note = driver.FindElement(By.ClassName("p-parameter")).Text;
                Console.WriteLine("parameter\n" + note);
                var _lines = note.Split('\n');
                foreach (var l in _lines)
                {
                    if (l.StartsWith("能效等级："))
                    {
                        price.Notes.Add(l);
                        Console.WriteLine("parameter note\n" + l);
                    }
                }
                //File.AppendAllText(@"D:\test.txt", note);
            }
            //}
            return price;
        }
        public static Price Parse(string text, string url)
        {
            text = text.Replace("\r", "");
            Console.WriteLine(url);
            
            //File.AppendAllText(outputPath, url + "\n");
            Console.WriteLine(text);
            var textLine = text.Split('\n');
            var price = new Price();
            price.sourceUrl = url;
            var whatToRead = "";
            //foreach (var line in textLine)
            //{
            //    if (whatToRead == "")
            //    {
            //        if (line.StartsWith("参考价") || line.StartsWith("预售价"))
            //        {
            //            whatToRead = "old";
            //            continue;
            //        }
            //        else if (line.Contains("易购价") || line.Contains("活动价"))
            //        {
            //            whatToRead = "current";
            //            continue;
            //        }
            //        else if (line.Contains("满 减"))
            //        {
            //            whatToRead = "cut1";
            //            continue;
            //        }
            //        else if (line.Contains("参加以下活动") || line.Contains("可参加以下优惠活动"))
            //        {
            //            whatToRead = "cut2";
            //            continue;
            //        }
            //        else if (line.StartsWith("定金"))
            //        {
            //            whatToRead = "deposit";
            //            continue;
            //        }
            //        else if (line.StartsWith("尾款"))
            //        {
            //            whatToRead = "retainage";
            //            continue;
            //        }
            //    }
            //    else
            //    {
            //        try
            //        {
            //            switch (whatToRead)
            //            {
            //                case "old":
            //                    price.oldPrice = Double.Parse(ParseDigits(line));
            //                    whatToRead = "";
            //                    break;
            //                case "current":
            //                    price.currentPrice = Double.Parse(ParseDigits(line));
            //                    whatToRead = "";
            //                    break;
            //                case "deposit":
            //                    price.deposit = Double.Parse(ParseDigits(line));
            //                    whatToRead = "";
            //                    break;
            //                case "retainage":
            //                    price.retainage = Double.Parse(ParseDigits(line));
            //                    whatToRead = "";
            //                    break;
            //                case "cut1":
            //                    var tokens = line.Split(new string[] { "元" }, StringSplitOptions.None);
            //                    var reach = -1.0;
            //                    var cut = -1.0;
            //                    var top = -1.0;
            //                    if (tokens.Length == 3)
            //                    {
            //                        if (tokens[0].StartsWith("每") && tokens[1].StartsWith("减"))
            //                        {
            //                            reach = Double.Parse(ParseDigits(tokens[0]));
            //                            cut = Double.Parse(ParseDigits(tokens[1]));
            //                            price.coupons.Add(new List<double>() { reach, cut, top });
            //                        }
            //                        else
            //                        {
            //                            Console.WriteLine("Error Parse Cut1 " + line);
            //                        }
            //                    }
            //                    if (tokens.Length == 4)
            //                    {
            //                        if (tokens[0].StartsWith("每") && tokens[1].StartsWith("减") && tokens[2].Contains("最多减"))
            //                        {
            //                            reach = Double.Parse(ParseDigits(tokens[0]));
            //                            cut = Double.Parse(ParseDigits(tokens[1]));
            //                            top = Double.Parse(ParseDigits(tokens[2]));
            //                            price.coupons.Add(new List<double>() { reach, cut, top });
            //                        }
            //                        else
            //                        {
            //                            Console.WriteLine("Error Parse Cut1 " + line);
            //                        }
            //                    }
            //                    whatToRead = "";
            //                    break;
            //                case "cut2":
            //                    var coupon = line.Split(new string[] { "满" }, StringSplitOptions.None);
            //                    if (coupon.Length == 2)
            //                    {
            //                        coupon = coupon[1].Split(new string[] { "用" }, StringSplitOptions.None);
            //                        if (coupon.Length == 2)
            //                        {
            //                            var reach2 = Double.Parse(ParseDigits(coupon[0]));
            //                            var cut2 = Double.Parse(ParseDigits(coupon[1]));
            //                            price.coupons.Add(new List<double>() { reach2, cut2 });
            //                        }
            //                    }
            //                    else
            //                    {
            //                        whatToRead = "";
            //                        //Console.WriteLine("Error Parse Cut1 " + line);
            //                    }
            //                    break;
            //                default:
            //                    break;

            //            }
            //        }
            //        catch(System.FormatException e)
            //        {
            //            Console.WriteLine(e.Message);
            //            whatToRead = "";
            //        }
            //    }
            //}

            return price;
        }
    }
}
