using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmzdmBot
{
    public class TMPriceParser
    {
        public static Price Parse(string text, string url)
        {
            text = text.Replace("\r", "");
            var oldPrice = 0.0;
            var currentPrice = 0.0;
            var textLine = text.Split('\n').ToList();
            var loc = textLine.IndexOf("价格");
            var price = new Price();

            for(int i =0; i< textLine.Count; i++)
            {
                if(textLine[i] == "价格")
                {
                    if (i + 1 < textLine.Count)
                    {
                        oldPrice = Double.Parse(Helper.ParseDigits(textLine[i + 1]));
                        price.oldPrice = oldPrice;
                    }
                }
                else if(textLine[i] == "促销价")
                {
                    if (i + 1 < textLine.Count)
                    {
                        currentPrice = Double.Parse(Helper.ParseDigits(textLine[i + 1]));
                        price.currentPrice = currentPrice;
                    }
                }
                else if(textLine[i].StartsWith("满") && textLine[i].Contains("元减"))
                {
                    var tokens = textLine[i].Split(new string[] { "元" }, StringSplitOptions.None);
                    if(tokens.Length == 3)
                    {
                        var reach = Double.Parse(Helper.ParseDigits(tokens[0]));
                        var cut = Double.Parse(Helper.ParseDigits(tokens[1]));
                        price.coupons.Add(new List<double>() { reach, cut, -1.0 });
                    }
                }
            }
            return price;

        }
    }
}
