using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmzdmBot
{
    public class DealSearchBot
    {
        bool login = false;
        private IWebDriver driver;
        public DealSearchBot()
        {
            driver = new FirefoxDriver();
        }
        public Price ParsePrice(string text, string source)
        {
            if (source.Contains("suning.com"))
            {
                return SUNINGPriceParser.Parse(text, source);
            }
            else if (source.Contains("item.jd.com"))
            {
                return JDPriceParser.Parse(text, source);
            }
            else if (source.Contains("tmall.com"))
            {
                return TMPriceParser.Parse(text, source);
            }
            return null;
        }
        public List<Price> Crawl(string startUrl, string outputPath, int limit)
        {
            Queue<string> queue = new Queue<string>();
            var deals = new List<Price>();
            var visited = new List<string>();
            queue.Enqueue(startUrl);
            while(queue.Count>0 && deals.Count < limit)
            {
                var url = queue.Dequeue();
                visited.Add(url);
                var price = Search(url);
                if (price!=null && IsGoodDeal(price))
                {
                    deals.Add(price);
                    File.AppendAllText(outputPath, JsonConvert.SerializeObject(price) + "\n");
                    File.AppendAllText(outputPath, price.sourceUrl + "\n");
                }
                var newLinks = GetNewLinks();
                if (newLinks == null) continue;
                newLinks = newLinks.Except(visited).ToList();
                foreach(var link in newLinks)
                {
                    queue.Enqueue(link);
                }
            }
            return deals;
        }
        public void SearchAll(List<string> urlList, string outputPath)
        {
            foreach (var line in urlList)
            {
                var url = Helper.CheckUrl(line);
                if (string.IsNullOrWhiteSpace(url)) continue;
                var price = Search(url);
                if (price != null && IsGoodDeal(price))
                {
                    File.AppendAllText(outputPath, JsonConvert.SerializeObject(price) + "\n");
                    File.AppendAllText(outputPath, price.sourceUrl + "\n");
                }
            }
        }
        public bool IsGoodDeal(Price price)
        {
            if (price.oldPrice != 0 && price.finalPrice != 0 && price.finalPrice < price.oldPrice)
            {
                Print(price.oldPrice + " " + price.finalPrice);
                return true;
            }
            else if (price.retainage > 0 && price.deposit > 0 && price.finalPrice < price.oldPrice)
            {
                Print(price.finalPrice.ToString());
                return true;
            }
            return false;
        }

        public Price Search(string url)
        {
            //var url = Helper.CheckUrl(link);
            if (string.IsNullOrWhiteSpace(url)) return null;
            if (url.StartsWith("https://product.suning.com/"))
            {
                driver.Navigate().GoToUrl(url);
                var priceText = driver.FindElement(By.Id("priceDom")).Text;
                var title = driver.FindElement(By.Id("itemDisplayName")).Text;
                var storeInfo = driver.FindElement(By.ClassName("si-intro-list")).Text;
                var storeName = SUNINGPriceParser.ParseShopName(storeInfo);
                Console.WriteLine("store: " + storeName);
                var price = ParsePrice(priceText, url);
                price.sourceUrl = url;
                price.storeName = storeName;
                price.ItemName = title;
                price.Calculate();
                Console.WriteLine(JsonConvert.SerializeObject(price));
                return price;
                //File.AppendAllText(@"D:\test.txt", JsonConvert.SerializeObject(price) + "\n");
                //if (price.oldPrice != 0 && price.finalPrice != 0 && price.finalPrice < price.oldPrice)
                //{
                //    Print(price.oldPrice + " " + price.finalPrice);
                //    return price;
                //}
                //else if (price.retainage > 0 && price.deposit > 0 && price.finalPrice < price.oldPrice)
                //{
                //    Print(price.finalPrice.ToString());
                //    return price;
                //}
            }
            else if (url.StartsWith("https://item.jd.com/"))
            {
                //driver.Navigate().GoToUrl(url);
                //var text = driver.FindElement(By.ClassName("summary-price-wrap")).Text;
                //var title = driver.FindElement(By.ClassName("sku-name")).Text;
                //var price = ParsePrice(text, url, outputPath);
            }
            else if (url.StartsWith("https://detail.tmall.com/item.htm?") || url.StartsWith("https://world.tmall.com/item/")) 
            {
                driver.Navigate().GoToUrl(url);
                if(!login)
                {
                    Console.WriteLine("Please login TMall");
                    Console.ReadKey();
                    login = true;
                }
                var priceText = "";
                var title = "";
                var storeName = "";
                try
                {
                    priceText = driver.FindElement(By.ClassName("tm-fcs-panel")).Text;
                    title = driver.FindElement(By.ClassName("tb-detail-hd")).FindElement(By.TagName("h1")).Text;
                    storeName = driver.FindElement(By.ClassName("shopLink")).Text;
                }
                catch(NoSuchElementException e)
                {
                    Console.WriteLine(e.Message);
                }
                if (String.IsNullOrWhiteSpace(priceText))
                {
                    return null;
                }   
                var price = ParsePrice(priceText, url);
                price.sourceUrl = url;
                price.storeName = storeName;
                price.ItemName = title;
                price.Calculate();
                Console.WriteLine(url);
                Console.WriteLine(priceText);
                Console.WriteLine(title);
                Console.WriteLine(JsonConvert.SerializeObject(price));
                return price;
            }
            return null;
        }

        public List<string> GetNewLinks()
        {
            try
            {
                var elements = driver.FindElements(By.TagName("a")).ToList();
                List<string> links = elements.Select(x => Helper.CheckUrl(x.GetAttribute("href"))).ToList();
                return links.Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToList();
            }
            catch(NoSuchElementException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        private void Print(string text)
        {
            Console.WriteLine("~~~~~~~Found good price " + text);
        }
    }
}
