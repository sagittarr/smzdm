using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmzdmBot
{
    public class SUNINGPriceParser
    {
        public static string ParseDigits(string input)
        {
            return new string(input.Where(x => x == '.' || Char.IsDigit(x)).ToArray());
        }
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
        public static Price Parse(string text, string url, string outputPath)
        {
            text = text.Replace("\r", "");
            Console.WriteLine(url);
            
            //File.AppendAllText(outputPath, url + "\n");
            Console.WriteLine(text);
            //File.AppendAllLines(outputPath, text.Split('\n'));
            var textLine = text.Split('\n');
            var price = new Price();
            price.sourceUrl = url;
            var whatToRead = "";
            foreach (var line in textLine)
            {
                if (whatToRead == "")
                {
                    if (line.StartsWith("参考价") || line.StartsWith("预售价"))
                    {
                        whatToRead = "old";
                        continue;
                    }
                    else if (line.Contains("易购价") || line.Contains("活动价"))
                    {
                        whatToRead = "current";
                        continue;
                    }
                    else if (line.Contains("满 减"))
                    {
                        whatToRead = "cut1";
                        continue;
                    }
                    else if (line.Contains("参加以下活动") || line.Contains("可参加以下优惠活动"))
                    {
                        whatToRead = "cut2";
                        continue;
                    }
                    else if (line.StartsWith("定金"))
                    {
                        whatToRead = "deposit";
                        continue;
                    }
                    else if (line.StartsWith("尾款"))
                    {
                        whatToRead = "retainage";
                        continue;
                    }
                }
                else
                {
                    try
                    {
                        switch (whatToRead)
                        {
                            case "old":
                                price.oldPrice = Double.Parse(ParseDigits(line));
                                whatToRead = "";
                                break;
                            case "current":
                                price.currentPrice = Double.Parse(ParseDigits(line));
                                whatToRead = "";
                                break;
                            case "deposit":
                                price.deposit = Double.Parse(ParseDigits(line));
                                whatToRead = "";
                                break;
                            case "retainage":
                                price.retainage = Double.Parse(ParseDigits(line));
                                whatToRead = "";
                                break;
                            case "cut1":
                                var tokens = line.Split(new string[] { "元" }, StringSplitOptions.None);
                                var reach = -1.0;
                                var cut = -1.0;
                                var top = -1.0;
                                if (tokens.Length == 3)
                                {
                                    if (tokens[0].StartsWith("每") && tokens[1].StartsWith("减"))
                                    {
                                        reach = Double.Parse(ParseDigits(tokens[0]));
                                        cut = Double.Parse(ParseDigits(tokens[1]));
                                        price.coupons.Add(new List<double>() { reach, cut, top });
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Parse Cut1 " + line);
                                    }
                                }
                                if (tokens.Length == 4)
                                {
                                    if (tokens[0].StartsWith("每") && tokens[1].StartsWith("减") && tokens[2].Contains("最多减"))
                                    {
                                        reach = Double.Parse(ParseDigits(tokens[0]));
                                        cut = Double.Parse(ParseDigits(tokens[1]));
                                        top = Double.Parse(ParseDigits(tokens[2]));
                                        price.coupons.Add(new List<double>() { reach, cut, top });
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Parse Cut1 " + line);
                                    }
                                }
                                whatToRead = "";
                                break;
                            case "cut2":
                                var coupon = line.Split(new string[] { "满" }, StringSplitOptions.None);
                                if (coupon.Length == 2)
                                {
                                    coupon = coupon[1].Split(new string[] { "用" }, StringSplitOptions.None);
                                    if (coupon.Length == 2)
                                    {
                                        var reach2 = Double.Parse(ParseDigits(coupon[0]));
                                        var cut2 = Double.Parse(ParseDigits(coupon[1]));
                                        price.coupons.Add(new List<double>() { reach2, cut2 });
                                    }
                                }
                                else
                                {
                                    whatToRead = "";
                                    //Console.WriteLine("Error Parse Cut1 " + line);
                                }
                                break;
                            default:
                                break;

                        }
                    }
                    catch(System.FormatException e)
                    {
                        Console.WriteLine(e.Message);
                        whatToRead = "";
                    }
                }
            }

            return price;
        }
    }
}
