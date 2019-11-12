using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser
{
    public class Price
    {
        //public string itemTitle { get; set; }
        public double oldPrice { get; set; }
        public double currentPrice { get; set; }
        public List<List<double>> coupons { get; set; }
        public string sourceUrl { get; set; }
        public double deposit { get; set; }
        public double retainage { get; set; }

        public double finalPrice { get; set; }
        public Price()
        {
            coupons = new List<List<double>>();
        }
        private double ApplyCoupon(List<List<double>> _coupons, double price)
        {
            var _finalPrice = 0.0;
            foreach (var c in _coupons)
            {
                if (c.Count == 2)
                {
                    if (price > c[0])
                    {
                        if (_finalPrice == 0 || price - c[1] < _finalPrice)
                        {
                            _finalPrice = price - c[1];
                        }
                    }
                }
                else if (c.Count == 3)
                {
                    var totalCut = 0.0;
                    var top = c[2];
                    if (c[2] == -1.0) top = price + 1.0;
                    while (totalCut < top)
                    {
                        if (price >= c[0])
                        {
                            totalCut += c[1];
                            price = price - totalCut;
                        }
                        else
                        {
                            break;
                        }
                    }
                    _finalPrice = price;
                }
            }
            return _finalPrice;
        }
        public void Calculate()
        {
            if (sourceUrl.Contains("suning.com"))
            {
                if(currentPrice>0)
                {
                    var newPrice = ApplyCoupon(coupons, currentPrice);
                    if (newPrice == 0) newPrice = currentPrice;
                    finalPrice = newPrice < currentPrice ? newPrice : currentPrice;
                }
                else if(deposit>0 && retainage > 0)
                {
                    var price = deposit + retainage;
                    var newPrice = ApplyCoupon(coupons, deposit + retainage);
                    if (newPrice == 0) newPrice = price;
                    finalPrice = newPrice < deposit+ retainage ? newPrice : deposit + retainage;
                }
            }
        }
    }
}
