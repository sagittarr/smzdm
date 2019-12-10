using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SmzdmBot
{
    public class SmzdmWorker
    {
        public int Level = -1;
        public int baoLiaoLeft = -1;
        public int startNumber = -1;
        public string nickName = "";
        public int gold = -1;
        public Option option;
        public IWebDriver driver;
        public SmzdmWorker(Option opt)
        {
            //
            if(opt.Browser == "firefox")
            {
                driver = new FirefoxDriver();
            }
            else
            {
                driver = new ChromeDriver();
            }
            
            option = opt;
        }

        //public SmzdmWorker(Option opt, FirefoxDriver driver)
        //{
        //    this.driver = driver;
        //    this.option = opt;
        //}

        public void TransferGoldAndLogStatus()
        {
            if (!String.IsNullOrWhiteSpace(option.GoldTransferTarget))
            {
                this.TransferGold(option.GoldTransferTarget);
            }
            var status = getStatus();
            Console.WriteLine(status);

            var arr = status.Replace("\r", "").Split('\n');
            for (int i = 0; i < arr.Length; i++)
            {
                //Console.WriteLine(a);

                if (arr[i].Contains("金币") && i > 0)
                {
                    var num = new string(arr[i - 1].Where(x => Char.IsDigit(x)).ToArray());
                    Console.WriteLine("gold " + num);
                    if (!string.IsNullOrWhiteSpace(num))
                    {
                        gold = int.Parse(num);
                    }
                }
            }

            var account = new Account();
            if (option.username.Contains('@'))
            {
                account.email = option.username;
            }
            else
            {
                account.phone = option.username;
            }
            account.Level = Level;
            account.BaoLiaoLeftCount = baoLiaoLeft;
            account.GoldLeft = gold;
            File.AppendAllText(option.StatusFilePath, JsonConvert.SerializeObject(account) + "\n");
        }
        public bool Login()
        {
            driver.Navigate().GoToUrl(@"https://www.smzdm.com/baoliao/?old");
            driver.Navigate().Refresh();
            driver.FindElement(By.Id("username")).SendKeys(option.username); //"18604568194" smzdm24202@outlook.com
            driver.FindElement(By.Id("password")).Click();
            driver.FindElement(By.Id("password")).SendKeys(option.password);

            var login = driver.FindElement(By.Id("login_submit"));
            if (login != null)
            {
                login.Click();
            }
            int counter = 100;
            bool logined = false;
            while (counter > 0)
            {
                counter--;
                Console.Write(".");
                Thread.Sleep(3000);
                try
                {
                    driver.FindElement(By.Id("get_info_btn"));
                    logined = true;
                    Console.WriteLine("Login Successfully");
                    break;

                }
                catch (NoSuchElementException)
                {

                }
            }
            if (logined)
            {
                driver.Navigate().GoToUrl(@"https://www.smzdm.com/");
                driver.Navigate().Refresh();
                driver.FindElement(By.ClassName("J_punch")).Click();
                Console.WriteLine("Check-in punched.");
                driver.Navigate().GoToUrl(@"https://www.smzdm.com/baoliao/?old");
                return true;
            }
            else
            {
                Console.WriteLine("Login timeout");
                return false;
            }
            //Console.WriteLine("Type Y to continue, after human check.");
            //if (Console.ReadLine().ToLower().Contains("y"))
            //{
            //    driver.Navigate().GoToUrl(@"https://www.smzdm.com/");
            //    driver.FindElement(By.ClassName("J_punch")).Click();
            //    Console.WriteLine("Check in punched.");
            //    driver.Navigate().GoToUrl(@"https://www.smzdm.com/baoliao/?old");
            //    return true;
            //}
           
        }



        public bool SubmitBaoLiao(int despMode, double smzdmGoodPrice, double oldPrice, string url, double rate1, double rate2, Price priceObject = null)
        {
            var priceText = driver.FindElement(By.Id("un-item-price")).GetAttribute("value");
            var name = driver.FindElement(By.Id("un-item-goods")).GetAttribute("value");
            var brand = driver.FindElement(By.Id("un-item-brand")).GetAttribute("value");
            var type1 = driver.FindElement(By.Id("search_type_one")).Text;
            var type2 = driver.FindElement(By.Id("search_type_two")).Text;
            var type3 = driver.FindElement(By.Id("search_type_three")).Text;
            var type4 = driver.FindElement(By.Id("search_type_four")).Text;
            var currency = driver.FindElement(By.Id("money_unit")).Text;
            if (!String.IsNullOrWhiteSpace(priceText) && !String.IsNullOrWhiteSpace(name) && !String.IsNullOrWhiteSpace(name))
            {

                Console.WriteLine("current category is " + type1 + " "+ type2 + " "+ type3 + " "+ type4);
                if (type1 != null && type1.Contains("服饰鞋包"))
                {
                    Console.WriteLine("Skip clothes category");
                    return true;
                }
                var currentPrice = double.Parse(priceText);
                priceText += currency;
                Console.WriteLine(priceText);
                if (rate1>0.0 && smzdmGoodPrice > 0 && currentPrice > smzdmGoodPrice * rate1)
                {
                    Console.WriteLine("price is not good " + currentPrice + " " + smzdmGoodPrice);
                    return true;
                }
                if (smzdmGoodPrice > 0 && currentPrice < smzdmGoodPrice * 0.5)
                {
                    Console.WriteLine("price is too good " + currentPrice + " " + smzdmGoodPrice);
                    return true;
                }
                if (rate2>0.0 && oldPrice > 0 && currentPrice > oldPrice * rate2)
                {
                    Console.WriteLine("price is not good " + currentPrice + " " + oldPrice);
                    return true;
                }
                driver.FindElement(By.Id("un-content-price")).Click();
                Thread.Sleep(2000);
                var desp = driver.FindElement(By.Id("un-content-price"));
                if (desp != null)
                {
                    Console.WriteLine("current:" + currentPrice + " vs reference:"+ smzdmGoodPrice);
                    var vender = Helper.GenerateVenderName(url, priceObject);
                    
                    if (despMode == 1)
                    {
                        desp.SendKeys("预计到手价" + priceText +"!");
                    }
                    else if (despMode == 0)
                    { 
                        desp.SendKeys(GenerateDesp(priceText, vender, priceObject));
                    }
                    else
                    {
                        desp.SendKeys(option.CustomDescriptionPrefix + " 预计到手价" + priceText + "!");
                    }

                }
                Console.WriteLine("Continue to submit");
                //Console.ReadKey();
                //driver.FindElement(By.Id("un-feedback-submit")).Submit();
                try
                {

                    driver.FindElement(By.Id("un-feedback-submit")).Click();
                }
                catch (Exception)
                {
                    return true;
                }
                Console.WriteLine("Sleep for 5 seconds.");
                Thread.Sleep(5000);
                return true;
            }
            return true;
        }

        private string GetRandomOne(List<string> candidates)
        {
            var random = new Random();
            var index = random.Next(candidates.Count);
            return candidates[index];
        }
        private string GenerateDesp(string priceText, string vender, Price priceObject = null)
        {
            var desp = "";
            desp += GetRandomOne(new List<string>() { "预计到手价", "预计到手", "目前优惠价", "现活动价", "优惠后售价" }) + priceText;
            if (priceObject != null)
            {
                double quote = Double.Parse(Helper.ParseDigits(priceText));
                Console.WriteLine("Quote " + quote + " vs MyPrice " + priceObject.finalPrice);
                if (priceObject.finalPrice <= quote && vender == "京东")
                {
                    if (!string.IsNullOrWhiteSpace(priceObject.PromoteNote))
                    {
                        desp += " " + priceObject.PromoteNote + " ";
                    }
                }
                if (priceObject.Notes.Count > 0)
                {
                    desp += " " + string.Join(" ", priceObject.Notes);
                }
                if (!string.IsNullOrWhiteSpace(vender))
                {
                    desp += " 来自" + vender;
                    if (!String.IsNullOrWhiteSpace(priceObject.storeName))
                    {
                        desp += " " + priceObject.storeName;
                    }
                }

            }
            desp += " " + GetRandomOne(new List<string>() { "喜欢的可以入手", "有兴趣的值友可以看看", "刚需可入" });
            return desp;
        }
        private int GetNumber(string input)
        {
            bool start = false;
            var num = "";
            for(var i=0; i<input.Length; i++)
            {
                if (Char.IsDigit(input[i]))
                {
                    num += input[i];
                    start = true;
                }
                else if(start == true)
                {
                    break;
                }
            }
            return int.Parse(num);
        }
        public  void ScrollIntoView(IWebElement ele)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(" + ele.Location.X + "," + ele.Location.Y + ")");
        }
        private void Pay(int gold)
        {
            var gratuityProcess = driver.FindElement(By.ClassName("gratuity-process"));
            var inputs = gratuityProcess.FindElements(By.ClassName("custom"));
            Console.WriteLine("c" + inputs.Count);
            foreach (var p in inputs)
            {
                try
                {
                    p.Click();
                    Thread.Sleep(1000);
                    p.SendKeys(gold.ToString());
                }
                catch (ElementNotInteractableException)
                {
                }
            }
            Thread.Sleep(1000);
            var element = driver.FindElement(By.ClassName("pop-interval-30"));
            var btns = element.FindElements(By.TagName("a"));
            var payClicked = false;
            foreach (var btn in btns)
            {
                if (btn.Text.Contains("打") && btn.Text.Contains("赏") && btn.GetAttribute("href") == "javascript:void(0)")
                {
                    btn.Click();
                    payClicked = true;
                    Console.WriteLine("pay clicked");
                }
            }
            if (payClicked)
            {
                Thread.Sleep(1000);
                foreach (var btn in btns)
                {
                    if (btn.Text.Contains("确") && btn.Text.Contains("认") && btn.GetAttribute("href") == "javascript:void(0)")
                    {
                        btn.Click();
                        Console.WriteLine("confirm clicked");
                    }
                }
            }
        }
        public void TransferGold(string url)
        {
            driver.Navigate().GoToUrl(url);
            var count = 0;
            //security-input
            while (count<4)
            {
                count++;
                var feed = driver.FindElement(By.Id("feed-side"));
                var spans = feed.FindElements(By.TagName("span"));
                foreach (var span in spans)
                {
                    if (span.Text == "打赏")
                    {
                        span.Click();
                    }
                }
                Thread.Sleep(1000);
                var gratuity = driver.FindElement(By.ClassName("gratuity-option"));
                var labels = gratuity.FindElements(By.TagName("label"));
                var silver = -1;
                var gold = 0;
                foreach (var l in labels)
                {
                    if (silver == -1)
                    {
                        silver = GetNumber(l.Text);
                    }
                    else
                    {
                        gold = GetNumber(l.Text);
                    }

                }
                Console.WriteLine(silver);
                Console.WriteLine(gold);

                if (gold > 5)
                {
                    driver.FindElement(By.Id("gold")).Click();
                    Console.WriteLine("Gold Selected");
                    var amount = gold >= 49 ? 49 : gold;
                    Thread.Sleep(1000);
                    Pay(amount);
                    Thread.Sleep(4000);
                }
                else
                {
                    break;
                }
            }
            //Console.WriteLine("Gold Left " + goldText);
        }
        public void Like(string Id= "2825621472")
        {
            var url = @"https://zhiyou.smzdm.com/member/"+Id+"/friendships/followers/";
            driver.Navigate().GoToUrl(url);
            var friends = driver.FindElements(By.ClassName("user-avatar")).ToList();
            var links = new List<string>();
            foreach(var element in friends)
            {
                var link = element.GetAttribute("href");
                if (link.StartsWith("https://zhiyou.smzdm.com/member/"))
                {
                    links.Add(link);
                }
            }
            Console.WriteLine("total "+links.Count + " friends");
            foreach(var link in links)
            {
                driver.Navigate().GoToUrl(link+ "baoliao/");
                var items = driver.FindElements(By.ClassName("pandect-content-stuff")).ToList();
                var itemLinkList = new List<string>();
                foreach (var item in items)
                {
                    var timeStamp = item.FindElement(By.ClassName("pandect-content-time")).Text;
                    if(timeStamp.Contains("前") || timeStamp.Contains("刚"))
                    {
                        var itemLink = item.FindElement(By.TagName("a")).GetAttribute("href");
                        if (itemLink.StartsWith("https://www.smzdm.com/p/"))
                        {
                            itemLinkList.Add(itemLink);
                        }
                    }
                    else if (timeStamp.Contains("-"))
                    {
                        var today = DateTime.Today;
                        CultureInfo provider = CultureInfo.InvariantCulture;
                        Console.WriteLine("'" + timeStamp + "'");
                        DateTime date = DateTime.ParseExact(timeStamp.Split(' ')[0], "mm-dd", provider);
                        var diff = (today - date).Days;
                        Console.WriteLine(today.Day + " " + date.Day + " " + diff);
                    }
                }
                foreach(var itemLink in itemLinkList)
                {
                    driver.Navigate().GoToUrl(itemLink);
                    var worth = driver.FindElement(By.Id("rating_worthy_num")).Text;
                    var unWorth = driver.FindElement(By.Id("rating_unworthy_num")).Text;
                    
                    Console.WriteLine(worth + ":" + unWorth);
                    var worthV = double.Parse(worth);
                    var unWorthV = double.Parse(unWorth);
                    
                    if (worthV<=3 || (unWorthV>0 && worthV / (worthV+unWorthV) <= 0.6))
                    {
                        driver.FindElement(By.Id("rating_worthy_num")).Click();
                        Console.WriteLine("Worth Clicked");
                    }
                    var counter = GetFaverCount(driver);
                    if (counter !=null)
                    {
                        var mark = driver.FindElement(By.ClassName("icon-star-o"));
                        mark.Click();
                        var count = int.Parse(counter.Text);
                        Console.WriteLine("Current Favor Count " + count);
                        var newCount = int.Parse(counter.Text);
                        if (newCount < count)
                        {
                            Console.WriteLine("Current Favor Count " + newCount);
                            mark.Click();
                            Console.WriteLine("Current Favor Count " + int.Parse(counter.Text));
                        }
                    }
                }
                break;
            }
        }
        private IWebElement GetFaverCount(IWebDriver driver)
        {
            var divs = driver.FindElements(By.TagName("div"));
            foreach (var div in divs)
            {
                var divTitle = div.GetAttribute("title");
                if (divTitle == "收藏")
                {
                    return div.FindElement(By.TagName("span"));
                }
            }
            return null;
        }
        private bool CheckFrequecyNotice()
        {
            var fails = driver.FindElements(By.ClassName("layerSubInfo"));
            if (fails != null)
            {
                foreach (var f in fails)
                {
                    if (f.Text.Contains("分钟"))
                    {
                        Console.WriteLine("Too frequent submit, Sleep for 80 seconds.");
                        var count = 80;
                        while (count > 0)
                        {
                            Thread.Sleep(10000);
                            count = count - 10;
                            Console.WriteLine(count);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        private bool CheckWarnNotice()
        {
            var noticeDisplayed = false;
            try
            {
                var red_font = driver.FindElement(By.ClassName("red-font"));
                Console.WriteLine(red_font.GetAttribute("value"));
                Console.WriteLine("T" + red_font.Text);
                noticeDisplayed = true;
            }
            catch (NoSuchElementException e)
            {

                noticeDisplayed = false;
            }
            return noticeDisplayed;
        }
        private bool CheckForm()
        {
            try
            {
                var v1 = !String.IsNullOrEmpty(driver.FindElement(By.Id("un-item-price")).GetAttribute("value"));
                var v2 = !String.IsNullOrEmpty(driver.FindElement(By.Id("un-item-goods")).GetAttribute("value"));
                var v3 = !String.IsNullOrEmpty(driver.FindElement(By.Id("un-item-brand")).GetAttribute("value"));
                if (v1 && v2 && v3) return true;
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine("form not render well");
                return false;
            }
            return false;
        }

        public int ReadInfo()
        {
            var notice = driver.FindElement(By.Id("bhNotice")).Text;
            if (!String.IsNullOrWhiteSpace(notice))
            {
                Console.WriteLine(notice);
                var ns = notice.Split('，');
                if (ns.Length == 2)
                {
                    var level = new string(ns[0].Where(c => Char.IsDigit(c)).ToArray());
                    var baoLiaoLeftStr = new string(ns[1].Where(c => Char.IsDigit(c)).ToArray());
                    if (level != null && baoLiaoLeftStr != null)
                    {
                        Level = int.Parse(level);
                        //baoLiaoLeft = int.Parse(baoLiaoLeft);
                        //Console.WriteLine("Level " + level + " Bao Liao left " + baoLiaoLeft);
                        this.baoLiaoLeft = int.Parse(baoLiaoLeftStr);
                        if (startNumber == -1)
                        {
                            startNumber = this.baoLiaoLeft;
                        }
                        return this.baoLiaoLeft;
                    }
                }
            }
            return -1;
        }
        //return code
        //0=warn notice=>next item
        //1=looks good=> submit
        //2=stop and quit
        public int PasteItemUrl(string url, int index, int timeWait, int stopNumber)
        {
            if (baoLiaoLeft == 0) return 2;
            if (baoLiaoLeft != -1 && startNumber - baoLiaoLeft >= stopNumber) return 2;
            Helper.ToUrl(driver, @"https://www.smzdm.com/baoliao/?old");
            Console.WriteLine("Pasting " + url);
            driver.FindElement(By.Name("item_link")).SendKeys(url); //item.jd.com/100005093980.html
            driver.FindElement(By.Id("get_info_btn")).Click();
            int countDown = 3;
            while (countDown > 0)
            {
                if (CheckFrequecyNotice())
                {
                    return 0;
                }
                Console.WriteLine("Sleep for " + timeWait + " seconds.");
                Thread.Sleep(timeWait * 1000);
                //var pop2 = driver.FindElement(By.Id("pop2"));
                int left = ReadInfo();
                if (baoLiaoLeft == 0) return 2;
                if (left != -1 && startNumber - baoLiaoLeft >= stopNumber)
                {
                    Console.WriteLine("Quit loop");
                    return 2;
                }
                else if (CheckWarnNotice())
                {
                    Console.WriteLine("Notice catched");
                    return 0;
                }
                else if (CheckForm())
                {
                    Console.WriteLine("looks good");
                    return 1; // form rendered succuss
                }
                else
                {
                    countDown--; // keep wait
                }
            }
            return 0; // next item
        }

        private string getStatus()
        {
            driver.Navigate().GoToUrl("https://zhiyou.smzdm.com/user/");
            nickName = driver.FindElement(By.ClassName("info-stuff-nickname")).Text;
            return driver.FindElement(By.ClassName("info-stuff-assets")).Text;
        }

        public void Shutdown()
        {
            this.driver.Close();
        }
    }
}
