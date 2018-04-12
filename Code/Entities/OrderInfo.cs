using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Entities
{

    /// <summary>
    /// OrderInfo 的摘要说明
    /// </summary>
    public class OrderInfo
    {
        public string orderno { get; set; }
        public int userid { get; set; }
        public DateTime date { get; set; }
        public int amountTotal { get; set; }
        public int couponid { get; set; }
        public int amountCoupon { get; set; }
        public int amount { get; set; }
        public string status { get; set; }
        public DateTime statusdate { get; set; }


        public OrderInfo(Common.DB.NVCollection nvc)
        {
            userid = ConvertHelper.ToInt32(nvc["userid"]);
            orderno = ConvertHelper.ToString(nvc["orderno"]);
            status = ConvertHelper.ToString(nvc["status"]);
            statusdate= Convert.ToDateTime(nvc["statusdate"]);
            amountCoupon = ConvertHelper.ToInt32(nvc["amountCoupon"]);
            amount = ConvertHelper.ToInt32(nvc["amount"]);
            couponid = ConvertHelper.ToInt32(nvc["couponid"]);
            amountTotal = ConvertHelper.ToInt32(nvc["amountTotal"]);
            couponid = ConvertHelper.ToInt32(nvc["couponid"]);
            date = Convert.ToDateTime(nvc["date"]);
        }
    }

}