using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmzdmBot
{
    public class DealFinder
    {
        bool login = false;
        public IWebDriver driver;
        public DealFinder(Option option)
        {
            if (option.Browser == "firefox")
            {
                driver = new FirefoxDriver();
            }
            else
            {
                driver = new ChromeDriver();
            }
            //driver = new ChromeDriver();
        }
        public void Shutdown()
        {
            this.driver.Close();
        }
        public List<string> GetItemIdFromWikiPage(string wikiPage)
        {
            if (wikiPage == null) return null;
            var itemUrls = new List<string>();
            if (Helper.ToUrl(driver, "https://wiki.smzdm.com/" + wikiPage + "/"))
            {
                var emlist = driver.FindElement(By.ClassName("feed-main-list"));
                foreach (var elm in emlist.FindElements(By.TagName("a")))
                {
                    if (elm.GetAttribute("href").StartsWith("https://wiki.smzdm.com/p/"))
                    {
                        itemUrls.Add(elm.GetAttribute("href"));
                    }
                }
            }
            return itemUrls;
        }
        public Dictionary<string, string> GetLinkFromWiki(string wikiUrl)
        {
            if (wikiUrl == null) return null;
            
            if (Helper.ToUrl(driver, wikiUrl))
            {
                var linkList = new List<string>();
                try
                {
                    var title = driver.FindElement(By.ClassName("pd-title")).Text; 
                    var mainPriceText = driver.FindElement(By.ClassName("sku-pd-price")).Text;
                    var lastSharePriceText = driver.FindElement(By.ClassName("z-highlight")).Text;
                    var mainPrice = 0.0;
                    var lastSharePrice = 0.0;
                    if (title.Contains("羽绒服") || title.Contains("裙") || title.Contains("裤")) return null;
                    //var index = mainPriceText.IndexOf("元");
                    var index = lastSharePriceText.IndexOf("元");
                    if (index == -1) return null;
                    try
                    {
                        //mainPrice = double.Parse(mainPriceText.Substring(0, index));
                        //Console.WriteLine("main price " + mainPrice);
                        lastSharePrice = double.Parse(lastSharePriceText.Substring(0, index));
                        Console.WriteLine("lastSharePrice " + lastSharePrice);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return null;
                    }
                    var zButton = driver.FindElement(By.ClassName("feed-link-btn-inner"));
                    var goLink = "";
                    var link = zButton.FindElement(By.TagName("a")).GetAttribute("href");
                    if (link.StartsWith("https://go.smzdm.com/"))
                    {
                        goLink = link;
                    }

                    var feedFirst = driver.FindElement(By.ClassName("feed-block-title"));
                    var itemLink = "";
                    link = "";
                    link = feedFirst.FindElement(By.TagName("a")).GetAttribute("href");
                    if (link.StartsWith("https://www.smzdm.com/p/"))
                    {
                        itemLink = link;
                    }
                    //var malls = driver.FindElements(By.ClassName("mall-price")).ToList();
                    //foreach(IWebElement element in malls)
                    //{
                    //    var href = element.GetAttribute("href");
                    //    if (href.StartsWith("https://go.smzdm.com/"))
                    //    {
                    //        linkList.Add(href);
                    //    }
                    //}
                    //var wiki = new SmzdmWiki();
                    //wiki.SmzdmGoUrls.AddRange(linkList);
                    //wiki.SmzdmGoodPrice = lastSharePrice;
                    //wiki.SmzdmItemTitle = title;
                    var it = new Dictionary<string, string>();
                    it.Add("smzdmProduct", itemLink);
                    it.Add("smzdmGo", goLink);
                    it.Add("smzdmGoodPrice", lastSharePrice.ToString());
                    it.Add("smzdmItemTitle", title);
                    Console.WriteLine(JsonConvert.SerializeObject(it));
                    return it;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            }
            return null;
        }
        public Dictionary<string, string> GetSmzdmItem(IWebElement item)
        {
            Console.WriteLine(item.Text);
            var oldPrice = 0.0;
            var lines = item.Text.Split('\n');
            var itemTitle = "";
            if (lines.Length >= 2)
            {
                if (lines[0].IndexOf("件") != -1 || lines[1].IndexOf("合") != -1)
                {
                    Console.WriteLine("complicated price skip ");
                    return null;
                }
                int idx = lines[1].IndexOf("元");
                if (idx != -1)
                {
                    var priceStr = lines[1].Substring(0, idx);
                    try
                    {
                        oldPrice = double.Parse(Helper.ParseDigits(priceStr));
                        if (oldPrice < 10) return null;
                    }
                    catch (System.FormatException)
                    {
                        return null;
                    }
                    itemTitle = lines[0];
                    Console.WriteLine("old price " + oldPrice);
                }
            }
            else
            {
                return null;
            }

            var elements = item.FindElements(By.TagName("a")).ToList();
            var goSmzdmUrl = "";
            var itemSmzdmUrl = "";
            foreach (IWebElement e in elements)
            {
                var href = e.GetAttribute("href");
                if (goSmzdmUrl == "" && !string.IsNullOrWhiteSpace(href) && href.StartsWith("https://go.smzdm.com/"))
                {
                    goSmzdmUrl = href;
                    Console.WriteLine(href);
                }
                else if (itemSmzdmUrl == "" && !string.IsNullOrWhiteSpace(href) && href.StartsWith("https://www.smzdm.com/p/") && !href.EndsWith("comment"))
                {
                    itemSmzdmUrl = href;
                    Console.WriteLine(href);
                }

                if (goSmzdmUrl != "" && itemSmzdmUrl != "")
                {
                    var it = new Dictionary<string, string>();
                    it.Add("smzdmProduct", itemSmzdmUrl);
                    it.Add("smzdmGo", goSmzdmUrl);
                    it.Add("smzdmGoodPrice", oldPrice.ToString());
                    it.Add("smzdmItemTitle", itemTitle);
                    return it;
                }
            }
            return null;
        }
        public Price CheckSmzdmItem(Dictionary<string, string> it)
        {
            if (it == null) return null;
            if (Helper.ToUrl(driver, it["smzdmProduct"]))
            {
                Console.WriteLine(driver.Url);
                try
                {
                    var url = "";
                    driver.FindElement(By.ClassName("new-baike-card"));
                    if (Helper.ToUrl(driver, it["smzdmGo"]))
                    {
                        Console.WriteLine(driver.Url);
                        url = driver.Url;
                        var price = new Price();
                        if (driver.Url.StartsWith("https://product.suning.com/") || driver.Url.StartsWith("http://product.suning.com/"))
                        {
                            price = SUNINGPriceParser.ExtractPrice(driver);
                            if (price != null)
                            {
                                price.Calculate();
                                if (price.finalPrice <= 0)
                                {
                                    Console.WriteLine("Fail to process SUNING item, Skip");
                                    return null;
                                }
                            }
                        }
                        else if(driver.Url.StartsWith("https://re.jd.com/cps/item/") || driver.Url.StartsWith("http://re.jd.com/cps/item/"))
                        {
                            driver.FindElement(By.ClassName("gobuy")).Click();
                            Console.WriteLine("Wait 5s for redirect from re.jd.com");
                            Thread.Sleep(5000);
                            if (driver.Url.StartsWith("https://passport.jd.com/"))
                            {
                                var parts = driver.Url.Split("?=".ToCharArray());
                                foreach(var p in parts)
                                {
                                    if (p.StartsWith("https://item.jd.com/"))
                                    {
                                        driver.Navigate().GoToUrl(p);
                                        price = JDPriceParser.ExtractPrice(driver);
                                        //driver.Close();
                                        //Console.WriteLine("close page1");
                                        break;
                                    }
                                }
                                
                            }
                            
                        }
                        else if (driver.Url.StartsWith("https://item.jd.com/") || driver.Url.StartsWith("http://item.jd.com/"))
                        {
                            price = JDPriceParser.ExtractPrice(driver);
                            //driver.Close();
                            //Console.WriteLine("close page2");
                        }
                        //if(driver.Url.StartsWith("https://item.jd.com/") || driver.Url.StartsWith("http://item.jd.com/"))
                        //{
                        //    price = JDPriceParser.ExtractPrice();
                        //}
                        price.SmzdmGoodPrice = double.Parse(it["smzdmGoodPrice"]);
                        price.sourceUrl = url;
                        price.SmzdmItemTitle = it["smzdmItemTitle"];
                        price.SmzdmGoUrl = it["smzdmGo"];

                        Console.WriteLine("added " + price.sourceUrl + " " + price.SmzdmGoodPrice);
                        
                        return price;
                    }

                }
                catch (NoSuchElementException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return null;
        }

        public List<Price> GetSmzdmItems(Option option)
        {
            var deals = new List<Price>();
            var pages = new List<string>();
            foreach (var pn in option.pageNumbers.Split(','))
            {
                pages.Add("https://www.smzdm.com/jingxuan/p" + pn + "/");
            }
            foreach (var page in pages)
            {
                driver.Navigate().GoToUrl(page);

                var items = driver.FindElements(By.ClassName("z-feed-content")).ToList();
                Console.WriteLine(items.Count);
                var SmzdmList = new List<Dictionary<string, string>>();

                foreach (IWebElement item in items)
                {
                    Console.WriteLine(item.Text);
                    var oldPrice = 0.0;
                    var lines = item.Text.Split('\n');
                    var itemTitle = "";
                    if (lines.Length >= 2)
                    {
                        if (lines[0].IndexOf("件") != -1 || lines[1].IndexOf("合") != -1)
                        {
                            Console.WriteLine("complicated price skip ");
                            continue;
                        }
                        int idx = lines[1].IndexOf("元");
                        if (idx != -1)
                        {
                            var priceStr = lines[1].Substring(0, idx);
                            try
                            {
                                oldPrice = double.Parse(Helper.ParseDigits(priceStr));
                                if (oldPrice < 10) continue;
                            }
                            catch (System.FormatException)
                            {
                                continue;
                            }
                            itemTitle = lines[0];
                            Console.WriteLine("old price " + oldPrice);
                        }
                    }
                    else
                    {
                        continue;
                    }
                    
                    var elements = item.FindElements(By.TagName("a")).ToList();
                    var goSmzdmUrl = "";
                    var itemSmzdmUrl = "";
                    foreach (IWebElement e in elements)
                    {
                        var href = e.GetAttribute("href");
                        if (goSmzdmUrl == "" && !string.IsNullOrWhiteSpace(href) && href.StartsWith("https://go.smzdm.com/"))
                        {
                            goSmzdmUrl = href;
                            Console.WriteLine(href);
                        }
                        else if (itemSmzdmUrl == "" && !string.IsNullOrWhiteSpace(href) && href.StartsWith("https://www.smzdm.com/p/") && !href.EndsWith("comment"))
                        {
                            itemSmzdmUrl = href;
                            Console.WriteLine(href);
                        }

                        if (goSmzdmUrl != "" && itemSmzdmUrl != "")
                        {
                            var it = new Dictionary<string, string>();
                            it.Add("smzdmProduct", itemSmzdmUrl);
                            it.Add("smzdmGo", goSmzdmUrl);
                            it.Add("smzdmGoodPrice", oldPrice.ToString());
                            it.Add("smzdmItemTitle", itemTitle);
                            SmzdmList.Add(it);
                            break;
                        }
                    }

                }

                foreach (var it in SmzdmList)
                {
                    if (Helper.ToUrl(driver,it["smzdmProduct"]))
                    {
                        try
                        {
                            driver.FindElement(By.ClassName("new-baike-card"));
                            if (Helper.ToUrl(driver, it["smzdmGo"]))
                            {
                                var p = new Price();
                                p.SmzdmGoodPrice = double.Parse(it["smzdmGoodPrice"]);
                                p.sourceUrl = driver.Url;
                                p.SmzdmItemTitle = it["smzdmItemTitle"];
                                p.SmzdmGoUrl = it["smzdmGo"];
                                deals.Add(p);
                                Console.WriteLine("added " + p.sourceUrl + " " + p.SmzdmGoodPrice);
                            }
                        }
                        catch (NoSuchElementException e)
                        {
                            Console.WriteLine("No Baike, drop " + it["smzdmProduct"]);
                        }
                    }
                }
            }
            return deals;
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
                var newLinks = GetNewLinks(url);
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
            Console.WriteLine("input link count " +urlList.Count);
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
            if (url.Contains("product.suning.com"))
            {
                driver.Navigate().GoToUrl(url);
                var price = SUNINGPriceParser.ExtractPrice(driver);
                Console.WriteLine(JsonConvert.SerializeObject(price));
                return price;
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

        public List<string> GetNewLinks(string url)
        {
            try
            {
                driver.Navigate().GoToUrl(url);
                var elements = driver.FindElements(By.TagName("a")).ToList();
                //elements.ForEach(x => Console.WriteLine(x.GetAttribute("href")));
                List<string> links = elements.Select(x => Helper.CheckUrl(x.GetAttribute("href"))).ToList();
                return links.Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToList();
            }
            catch(NoSuchElementException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            catch(StaleElementReferenceException e)
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
