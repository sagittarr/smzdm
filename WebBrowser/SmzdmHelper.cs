using OpenQA.Selenium;
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
            driver = new FirefoxDriver();
            option = opt;
        }

        public SmzdmWorker(Option opt, FirefoxDriver driver)
        {
            this.driver = driver;
            this.option = opt;
        }

        public void OutputStatus()
        {
            var status = getStatus();
            Console.WriteLine(status);
            var output = option.output;

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
            if (File.Exists(output))
            {
                File.AppendAllText(output, "username:" + option.username + "," + "nickName:" + nickName + ",level:" + Level + ",gold:" + gold + ",left:" + baoLiaoLeft + ",loginTime:" + DateTime.Now + ",status:" + status.Replace("\r", "").Replace("\n", "") + "\n");
            }
            else
            {
                Console.WriteLine(output + " file does not exist.");
                return;
            }
        }
        public bool Login()
        {
            driver.Navigate().GoToUrl(@"https://www.smzdm.com/baoliao/?old");
            driver.Navigate().Refresh();
            driver.FindElement(By.Id("username")).SendKeys(option.username); //"18604568194" smzdm24202@outlook.com
            driver.FindElement(By.Id("password")).SendKeys(option.password);

            var login = driver.FindElement(By.Id("login_submit"));
            if (login != null)
            {
                login.Click();
            }
            Console.WriteLine("Type Y to continue, after human check.");
            if (Console.ReadLine().ToLower().Contains("y"))
            {
                return true;
            }
            return false;
        }


        public bool SubmitBaoLiao(int despMode)
        {
            var price = driver.FindElement(By.Id("un-item-price")).GetAttribute("value");
            var name = driver.FindElement(By.Id("un-item-goods")).GetAttribute("value");
            var brand = driver.FindElement(By.Id("un-item-brand")).GetAttribute("value");

            if (!String.IsNullOrWhiteSpace(price) && !String.IsNullOrWhiteSpace(name) && !String.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("price " + price);
                var desp = driver.FindElement(By.Id("un-content-price"));
                if (desp != null)
                {

                    if (despMode == 1)
                    {
                        desp.SendKeys("预计到手价" + price + "!");
                    }
                    else if (despMode == 0)
                    {
                        desp.SendKeys(name + " 预计到手价" + price + "!");
                    }
                    else
                    {
                        desp.SendKeys(option.CustomDescriptionPrefix + " 预计到手价" + price + "!");
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

        private bool CheckFrequecyNotice()
        {
            var fails = driver.FindElements(By.ClassName("layerSubInfo"));
            if (fails != null)
            {
                foreach (var f in fails)
                {
                    if (f.Text.Contains("分钟"))
                    {
                        Console.WriteLine("Too frequent submit, Sleep for 70 seconds.");
                        var count = 70;
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
            if (baoLiaoLeft != -1 && startNumber - baoLiaoLeft >= stopNumber) return 2;
            driver.Navigate().GoToUrl(@"https://www.smzdm.com/baoliao/?old");
            Console.WriteLine("Pasting " + index + " " + url);
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

        public string getStatus()
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
