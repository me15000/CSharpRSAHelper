using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace API
{
    /// <summary>
    /// OpenApi 订单相关操作
    /// </summary>
    public partial class OpenApi
    {

        string genOrderNo()
        {
            var rnd = new Random();

            string abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";


            string prefix = string.Empty;

            for (int i = 0; i < 3; i++)
            {
                prefix += abc[rnd.Next(0, abc.Length)].ToString();
            }


            string orderno = prefix + DateTime.Now.ToString("yyyyMMdd") + "T" + DateTime.Now.ToString("HHmmss");


            return orderno;
        }

        /// <summary>
        /// 下单接口
        /// [POST] /open/order/order.do
        /// @authcode
        /// @amount
        /// @couponid
        /// </summary>
        public void order_order_do()
        {
            var postdata = ReadBodyData();



            string authcode = postdata.authcode ?? string.Empty;

            int userid = 0;
            if (!TryGetUserId(authcode, out userid))
            {
                EchoFailJson("!TryGetUserId");
                return;
            }

            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();
            var userinfo = Services.UserService.GetUserInfo(userid);

            if (userinfo == null)
            {
                EchoFailJson("!userinfo");
                return;
            }






            int orderamt = postdata.amount ?? 0;
            int couponid = postdata.couponid ?? 0;
            int amount = orderamt;
            int couponamt = 0;
            if (couponid != 0)
            {

                if (TryGetCouponAmount(orderamt, couponid, userid, out couponamt))
                {
                    amount = orderamt - couponamt;
                }
                #region oldcode
                //var coupon = dbh.GetData("select top 1 amount,id,ruleid from [user.coupon] where id=@0 and userid=@1", couponid,userid);
                //if (coupon != null)
                //{

                //    var limitamount = ConvertHelper.ToInt32( dbh.ExecuteScalar<object>("select amountLimit from [sys.couponRule] where id=@0", coupon["ruleid"]));
                //    if (limitamount <= orderamt)
                //    {
                //        couponamt = Convert.ToInt32(coupon["amount"]);
                //    }
                //    else
                //    {
                //        EchoFailJson("limitamount" + limitamount + "," + orderamt);
                //        return;

                //    }
                //}
                #endregion
            }

            if (amount <= 0)
            {
                EchoFailJson("amount <= 0");
                return;
            }

            string orderno = null;

            do
            {
                orderno = genOrderNo();

            } while (Convert.ToInt32(dbh.ExecuteScalar<object>("select top 1 1 from [user.order] where orderno=@0", orderno)) == 1);

            var rsp = new Common.DB.NVCollection();

            dbh.ExecuteNoneQuery("insert into [user.order] ([orderno],[userid],[date],[amountTotal],[couponid],[amountCoupon],[amount],[status],[statusdate]) values (@0,@1,@2,@3,@4,@5,@6,@7,@8)", orderno, userid, DateTime.Now, orderamt, couponid, couponamt, amount, Constant.OrderStatus.Start.GetHashCode(), DateTime.Now);

            var ph = new Common.Helpers.PayHelper();
            var result = ph.Order(orderno, amount, "https://" + Request.Url.Host + "/open/wx/notify.do", "诗科尼", userinfo.openid, GetClientIP(), "empty");

            rsp["code"] = 0;
            rsp["status"] = "succ";
            rsp["orderno"] = orderno;
            rsp["payinfo"] = result.Data;
            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
        }

        /// <summary>
        /// 订单状态
        /// [GET] /open/order/info.json
        /// @authcode
        /// @orderno
        /// </summary>
        public void order_info_json()
        {
            var rsp = new Common.DB.NVCollection();
            string authcode = Request.QueryString["authcode"];

            int userid = 0;
            if (!TryGetUserId(authcode, out userid))
            {
                EchoFailJson("!TryGetUserId");
                return;
            }

            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();


            string orderno = Request.QueryString["orderno"];
            if (string.IsNullOrEmpty(orderno))
            {
                EchoFailJson("orderno is null");
                return;
            }
            var order = dbh.GetData("select top 1 * from [user.order] where orderno=@0", orderno);
            if (order == null)
            {
                EchoFailJson("order is null");
                return;
            }

            if ((Constant.OrderStatus)Convert.ToInt32(order["status"]) == Constant.OrderStatus.Finish)
            {
                rsp["code"] = 0;
                rsp["status"] = "succ";
            }
            else
            {
                rsp["code"] = 0;
                rsp["status"] = "fail";
            }

            rsp["orderno"] = orderno;
            rsp["amount"] = order["amount"];
            rsp["amountCoupon"] = order["amountCoupon"];
            rsp["amountTotal"] = order["amountTotal"];
            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
            return;
        }

        /// <summary>
        /// 历史订单列表
        /// [GET] /open/order/list.json
        /// @authcode
        /// @ts
        /// </summary>
        public void order_list_json()
        {
            var rsp = new Common.DB.NVCollection();
            string authcode = Request.QueryString["authcode"];

            int userid = 0;
            if (!TryGetUserId(authcode, out userid))
            {
                EchoFailJson("!TryGetUserId");
                return;
            }

            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();


            long ts = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["ts"]))
            {
                ts = Convert.ToInt64(Request.QueryString["ts"]);
            }
            int pagesize = 20;

            var datas = new List<Common.DB.NVCollection>();
            if (ts == 0)
            {
                datas = dbh.GetDataList("select top " + pagesize + " orderno,status,date,amount from [user.order]  where userid=@0 order by date desc", userid);
            }
            else
            {
                datas = dbh.GetDataList("select top " + pagesize + " orderno,status,date,amount from [user.order]  where userid=@0 and date<@1 order by date desc", userid, Common.Helpers.TimeHelper.GetDateTimeFrom1970Ticks(ts));
            }
            rsp["code"] = 0;
            rsp["status"] = "succ";
            foreach (var data in datas)
            {
                data["datets"] = Common.Helpers.TimeHelper.GetTimeStamp(Convert.ToDateTime(data["date"]), 10);
                data["date"] = Convert.ToDateTime(data["date"]).ToString("yyyy-MM-dd");

                data["status"] = ((Constant.OrderStatus)Convert.ToInt32(data["status"])).ToString();
            }
            rsp["data"] = datas;
            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
        }



    }

}