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

        public static string ParseShopName(string text)
        {
            text = text.Replace("\r", "").Replace(" ","");
            List<string> lines = text.Split('\n').ToList();
            var loc = lines.IndexOf("商家：");
            if(loc!=-1 && loc+1 < lines.Count)
            {
                return lines[loc + 1];
            }
            return "";
        }
        public static Price Parse(string text, string url)
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
                                price.oldPrice = Double.Parse(Helper.ParseDigits(line));
                                whatToRead = "";
                                break;
                            case "current":
                                price.currentPrice = Double.Parse(Helper.ParseDigits(line));
                                whatToRead = "";
                                break;
                            case "deposit":
                                price.deposit = Double.Parse(Helper.ParseDigits(line));
                                whatToRead = "";
                                break;
                            case "retainage":
                                price.retainage = Double.Parse(Helper.ParseDigits(line));
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
                                        reach = Double.Parse(Helper.ParseDigits(tokens[0]));
                                        cut = Double.Parse(Helper.ParseDigits(tokens[1]));
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
                                        reach = Double.Parse(Helper.ParseDigits(tokens[0]));
                                        cut = Double.Parse(Helper.ParseDigits(tokens[1]));
                                        top = Double.Parse(Helper.ParseDigits(tokens[2]));
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
                                        var reach2 = Double.Parse(Helper.ParseDigits(coupon[0]));
                                        var cut2 = Double.Parse(Helper.ParseDigits(coupon[1]));
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
