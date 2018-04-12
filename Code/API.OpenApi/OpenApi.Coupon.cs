using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
namespace API
{
    /// <summary>
    /// OpenApi 代金券相关操作
    /// </summary>
    public partial class OpenApi
    {
        bool TryGetCouponAmount(int orderamt, int couponid, int userid, out int couponamt)
        {
            couponamt = 0;
            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();
            var coupon = dbh.GetData("select top 1 amount,id,ruleid,dateBegin,dateEnd from [user.coupon] where id=@0 and userid=@1", couponid, userid);
            if (coupon != null)
            {
                DateTime dateBegin = Convert.ToDateTime(coupon["dateBegin"]);
                DateTime dateEnd = Convert.ToDateTime(coupon["dateEnd"]);
                var now = DateTime.Now;
                if (dateBegin <= now && dateEnd.AddDays(1) >= now)
                {
                    var limitamount = ConvertHelper.ToInt32(dbh.ExecuteScalar<object>("select amountLimit from [sys.couponRule] where id=@0", coupon["ruleid"]));
                    if (limitamount <= orderamt)
                    {
                        couponamt = Convert.ToInt32(coupon["amount"]);

                        if (couponamt > 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        void SetFirstCouponPid(int userid, int pid)
        {
            var user = Services.UserService.GetUserInfo(userid);
            if (user.olduser || userid == pid)
            {
                return;
            }

            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();

            dbh.ExecuteNoneQuery("update [user.coupon] set pid=@0 where userid=@1 and (pid=0 or pid is null)", pid, userid);
        }

        void UpdateCouponStatus(int couponid, Constant.CouponStatus status)
        {
            string sql = "update [user.coupon] set status=@0,statusdate=@1 where id=@2;";

            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();

            dbh.ExecuteNoneQuery(sql, status.GetHashCode(), DateTime.Now, couponid);
        }

        /// <summary>
        /// 用户代金券列表
        /// [GET] /open/coupon/list.json
        /// @authcode
        /// @orderno
        /// @amount
        /// @type
        /// </summary>
        public void coupon_list_json()
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



            string type = "all";
            if (Request.QueryString["type"] == "valid")
            {
                type = "valid";
            }
            if (Request.QueryString["type"] == "order")
            {
                type = "order";
            }
            var data = new List<Common.DB.NVCollection>();
            //Response.Write(type);
            if (type == "order" && !string.IsNullOrEmpty(Request.QueryString["orderno"]))
            {
                var datas = dbh.GetDataList("select c.id,c.amount,c.dateBegin,c.dateEnd,c.status,cr.amountLimit,c.type from [user.coupon] as c left join [sys.couponRule] as cr on c.ruleid=cr.id where c.userid=@0  and c.orderno=@1 and cr.amountLimit>=@2 order by c.date desc", userid, Request.QueryString["orderno"], Convert.ToInt32(Request.QueryString["amount"]));
                foreach (var d in datas)
                {
                    if ((Constant.CouponStatus)d["status"] == Constant.CouponStatus.Enabled && (Convert.ToDateTime(d["dateBegin"]) < DateTime.Now) && (Convert.ToDateTime(d["dateEnd"]) > DateTime.Now))
                    {
                        d["status"] = "enable";
                    }
                    else
                    {
                        d["status"] = "disable";
                    }
                    d["dateBegin"] = Convert.ToDateTime(d["dateBegin"]).ToString("yyyy-MM-dd");
                    d["dateEnd"] = Convert.ToDateTime(d["dateEnd"]).ToString("yyyy-MM-dd");
                    d["canGive"] = ((Constant.CouponType)d["type"] == Constant.CouponType.Order);
                    d["type"] = ((Constant.CouponType)d["type"]).ToString();
                }
                data = datas;
            }
            else
            {

                if (type == "all")
                {
                    var datas = dbh.GetDataList("select c.id,c.amount,c.dateBegin,c.dateEnd,c.status,cr.amountLimit,c.type from [user.coupon] as c left join [sys.couponRule] as cr on c.ruleid=cr.id where c.userid=@0 and cr.amountLimit>=@1 order by c.date desc", userid, Convert.ToInt32(Request.QueryString["amount"]));
                    foreach (var d in datas)
                    {
                        d["canGive"] = ((Constant.CouponType)d["type"] == Constant.CouponType.Order);
                        if ((Constant.CouponStatus)d["status"] == Constant.CouponStatus.Enabled && (Convert.ToDateTime(d["dateBegin"]) < DateTime.Now) && (Convert.ToDateTime(d["dateEnd"]) > DateTime.Now))
                        {
                            d["status"] = "enable";
                        }
                        else
                        {
                            d["status"] = "disable";
                        }
                        d["dateBegin"] = Convert.ToDateTime(d["dateBegin"]).ToString("yyyy-MM-dd");
                        d["dateEnd"] = Convert.ToDateTime(d["dateEnd"]).ToString("yyyy-MM-dd");
                        d["type"] = ((Constant.CouponType)d["type"]).ToString();
                    }
                    data = datas;
                }
                else
                {

                    var datas = dbh.GetDataList("select c.id,c.amount,c.dateBegin,c.dateEnd,c.status,cr.amountLimit,c.type from [user.coupon] as c left join [sys.couponRule] as cr on c.ruleid=cr.id  where c.userid=@0 and c.dateBegin<=@1 and c.dateEnd>=@2 and c.status=@3 " + (Convert.ToInt32(Request.QueryString["amount"]) > 0 ? "and cr.amountLimit<=@4" : "") + " order by c.date desc", userid, DateTime.Now, DateTime.Now.AddDays(-1), Constant.CouponStatus.Enabled.GetHashCode(), Convert.ToInt32(Request.QueryString["amount"]));

                    foreach (var d in datas)
                    {
                        d["status"] = "enable";
                        d["dateBegin"] = Convert.ToDateTime(d["dateBegin"]).ToString("yyyy-MM-dd");
                        d["dateEnd"] = Convert.ToDateTime(d["dateEnd"]).ToString("yyyy-MM-dd");
                        d["canGive"] = ((Constant.CouponType)d["type"] == Constant.CouponType.Order);
                        d["type"] = ((Constant.CouponType)d["type"]).ToString();
                    }
                    data = datas;
                }
            }

            rsp["code"] = 0;
            rsp["status"] = "succ";
            rsp["data"] = data;
            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
            return;

        }

        /// <summary>
        /// 系统代金券列表
        /// [GET] /open/sys/coupon/list.json
        /// @authcode
        /// </summary>
        public void sys_coupon_list_json()
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

            var rules = dbh.GetDataList("select id,name,amount,amountLimit,daysBegin,daysEnd from [sys.couponRule] where [type]='sys' and [status]=1 " + (Request.QueryString["type"] == "money" ? " and id=1" : "") + " order by sort desc");
            foreach (var d in rules)
            {
                d["dateBegin"] = DateTime.Now.AddDays(Convert.ToInt32(d["daysBegin"])).ToString("yyyy-MM-dd");
                d["dateEnd"] = DateTime.Now.AddDays(Convert.ToInt32(d["daysEnd"])).ToString("yyyy-MM-dd");
                d.Remove("daysBegin");
                d.Remove("daysEnd");
            }
            rsp["code"] = 0;
            rsp["status"] = "succ";
            rsp["data"] = rules;
            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
        }

        /// <summary>
        /// 代金券单条信息查询
        /// [GET] /open/sys/coupon/info.json
        /// @authcode
        /// @ruleid
        /// </summary>
        public void sys_coupon_info_json()
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



            int ruleid = int.Parse(Request.QueryString["ruleid"] ?? "0");
            if (ruleid <= 0)
            {
                EchoFailJson("ruleid<=0");
                return;
            }

            var rule = dbh.GetData("select id,name,amount,amountLimit,daysBegin,daysEnd from [sys.couponRule] where [type]='sys' and [status]=1 and id=@0", ruleid);
            if (rule == null)
            {
                EchoFailJson("ruleid not exists");
                return;
            }

            rule["dateBegin"] = DateTime.Now.AddDays(Convert.ToInt32(rule["daysBegin"])).ToString("yyyy-MM-dd");
            rule["dateEnd"] = DateTime.Now.AddDays(Convert.ToInt32(rule["daysEnd"])).ToString("yyyy-MM-dd");
            rule.Remove("daysBegin");
            rule.Remove("daysEnd");

            rsp["code"] = 0;
            rsp["status"] = "succ";
            rsp["data"] = rule;
            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
        }

        /// <summary>
        /// 获得单条代金券基本信息
        /// [GET] /open/coupon/info.json
        /// @authcode
        /// @authcodefriend
        /// @couponid
        /// </summary>
        public void coupon_info_json()
        {
            var rsp = new Common.DB.NVCollection();
            string authcode = Request.QueryString["authcode"];
            string authcodefriend = Request.QueryString["authcodefriend"];


            int userid = 0;

            if (!TryGetUserId(authcode, out userid))
            {
                EchoFailJson("!TryGetUserId");
                return;
            }

            int pid = 0;

            if (!TryGetUserId(authcodefriend, out pid))
            {
                EchoFailJson("!TryGetUserId");
                return;
            }

            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();



            int couponid = int.Parse(Request.QueryString["couponid"] ?? "0");
            if (couponid <= 0)
            {
                EchoFailJson("couponid is null or empty");
                return;
            }

            var coupon = dbh.GetData("select top 1 c.id,c.amount,c.dateBegin,c.dateEnd,c.status,cr.amountLimit from [user.coupon] as c left join [sys.couponRule] as cr on c.ruleid=cr.id  where c.id=@0", couponid);
            if (coupon == null)
            {
                EchoFailJson("coupon is null");
                return;
            }

            if (((Constant.CouponStatus)coupon["status"] == Constant.CouponStatus.Enabled || (Constant.CouponStatus)coupon["status"] == Constant.CouponStatus.Giving) && (Convert.ToDateTime(coupon["dateBegin"]) < DateTime.Now) && (Convert.ToDateTime(coupon["dateEnd"]) > DateTime.Now))
            {
                coupon["status"] = "enable";
            }
            else
            {
                coupon["status"] = "disable";
            }
            coupon["dateBegin"] = Convert.ToDateTime(coupon["dateBegin"]).ToString("yyyy-MM-dd");
            coupon["dateEnd"] = Convert.ToDateTime(coupon["dateEnd"]).ToString("yyyy-MM-dd");
            rsp["code"] = 0;
            rsp["status"] = "succ";
            rsp["data"] = coupon;
            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
        }

        /// <summary>
        /// 领取代金券
        /// [GET] /open/coupon/get.do
        /// @authcode
        /// @couponid
        /// @couponruleid
        /// @authcodefriend
        /// </summary>
        public void coupon_get_do()
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



            //int couponid = int.Parse(Request.QueryString["couponid"] ?? "0");

            int couponruleid = int.Parse(Request.QueryString["couponruleid"] ?? "0");
            if (couponruleid != 1)
            {
                EchoFailJson("couponruleid!=1");
                return;
            }

            int pid = 0;

            string authcodefriend = Request.QueryString["authcodefriend"] ?? string.Empty;


            int parentid = 0;
            if (TryGetUserId(authcodefriend, out parentid))
            {
                pid = parentid;
            }

            //(couponid == 0 && couponruleid == 0) 
            if (couponruleid == 0 || pid == userid)
            {
                EchoFailJson("(couponid == 0 && couponruleid == 0) || pid == userid");

                return;
            }
            if (couponruleid > 0)
            {
                //老顾客无法领取赚钱计划代金券
                //if (Convert.ToBoolean(user["olduser"]) && couponruleid == "1")
                //{
                //    rsp["code"] = -1;
                //    rsp["status"] = "fail";
                //    Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
                //    return;
                //}


                var rule = dbh.GetData("select * from [sys.couponRule] where id=@0", couponruleid);
                if (rule == null)
                {
                    EchoFailJson("rule == null");
                    return;
                }

                var o = dbh.ExecuteScalar<object>("select top 1 id from [user.coupon] where [type]=@0 and ruleid=@1 and userid=@2 and dateBegin<=@3 and dateEnd>=@4 and status=@5", Constant.CouponType.System.GetHashCode(), couponruleid, userid, DateTime.Now, DateTime.Now, Constant.CouponStatus.Enabled);

                if (o != null && o != DBNull.Value)
                {
                    //EchoFailJson(" exists coupon");

                    //return;

                    dbh.ExecuteNoneQuery("update [user.coupon] set pid=@0 where id=@1", pid, Convert.ToInt32(o));
                }
                else
                {
                    dbh.ExecuteNoneQuery("insert into [user.coupon] (userid,ruleid,amount,type,date,dateBegin,dateEnd,status,statusdate,pid) values (@0,@1,@2,@3,@4,@5,@6,@7,@8,@9);", userid, couponruleid, rule["amount"], Constant.CouponType.System.GetHashCode(), DateTime.Now, DateTime.Now.AddDays(Convert.ToInt32(rule["daysBegin"])), DateTime.Now.AddDays(Convert.ToInt32(rule["daysEnd"])), Constant.CouponStatus.Enabled.GetHashCode(), DateTime.Now, pid);
                }


                if (pid != 0)
                {
                    dbh.ExecuteNoneQuery("update [user] set pid=@0 where id=@1", pid, userid);
                }

                rsp["code"] = 0;
                rsp["status"] = "succ";
                Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
                return;
            }

            //if (couponid > 0)
            //{
            //    var coupon = dbh.GetData("select * from [user.coupon] where id=@0 and [status]=@1", couponid, Constant.CouponStatus.Giving.GetHashCode());
            //    if (coupon == null)
            //    {
            //        rsp["code"] = -1;
            //        rsp["status"] = "fail";
            //        Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
            //        return;
            //    }

            //    var cnn = dbh.GetConnection();
            //    cnn.Open();
            //    DbTransaction trans = cnn.BeginTransaction();

            //    DbCommand sqlcmd_update_usercoupon = dbh.CreateCommand();
            //    sqlcmd_update_usercoupon.Connection = cnn;
            //    sqlcmd_update_usercoupon.Transaction = trans;

            //    DbCommand sqlcmd_insert_usercoupon = dbh.CreateCommand();
            //    sqlcmd_insert_usercoupon.Connection = cnn;
            //    sqlcmd_insert_usercoupon.Transaction = trans;

            //    try
            //    {
            //        sqlcmd_update_usercoupon.CommandText = "update [user.coupon] set [status]=@0 where id=@1";
            //        sqlcmd_update_usercoupon.Parameters.Add(dbh.CreateParameter("0", Constant.CouponStatus.Disabled.GetHashCode()));
            //        sqlcmd_update_usercoupon.Parameters.Add(dbh.CreateParameter("1", couponid));
            //        int n = sqlcmd_update_usercoupon.ExecuteNonQuery();
            //        if (n == 0)
            //        {
            //            EchoFailJson("update usercoupon fail");
            //            trans.Rollback();
            //            return;
            //        }

            //        sqlcmd_insert_usercoupon.CommandText = "insert into [user.coupon] (userid,ruleid,amount,type,date,dateBegin,dateEnd,status,statusdate,pid) values (@0,@1,@2,@3,@4,@5,@6,@7,@8,@9)";
            //        sqlcmd_insert_usercoupon.Parameters.Add(dbh.CreateParameter("0", userid));
            //        sqlcmd_insert_usercoupon.Parameters.Add(dbh.CreateParameter("1", coupon["ruleid"]));
            //        sqlcmd_insert_usercoupon.Parameters.Add(dbh.CreateParameter("2", coupon["amount"]));
            //        sqlcmd_insert_usercoupon.Parameters.Add(dbh.CreateParameter("3", Constant.CouponType.Friend.GetHashCode()));
            //        sqlcmd_insert_usercoupon.Parameters.Add(dbh.CreateParameter("4", DateTime.Now));
            //        sqlcmd_insert_usercoupon.Parameters.Add(dbh.CreateParameter("5", coupon["dateBegin"]));
            //        sqlcmd_insert_usercoupon.Parameters.Add(dbh.CreateParameter("6", coupon["dateEnd"]));
            //        sqlcmd_insert_usercoupon.Parameters.Add(dbh.CreateParameter("7", Constant.CouponStatus.Enabled.GetHashCode()));
            //        sqlcmd_insert_usercoupon.Parameters.Add(dbh.CreateParameter("8", DateTime.Now));
            //        sqlcmd_insert_usercoupon.Parameters.Add(dbh.CreateParameter("9", pid));
            //        n = sqlcmd_insert_usercoupon.ExecuteNonQuery();
            //        if (n == 0)
            //        {
            //            EchoFailJson("insert usercoupon fail");
            //            trans.Rollback();
            //            return;
            //        }
            //        if (pid != 0)
            //        {
            //            dbh.ExecuteNoneQuery("update [user] set pid=@0 where id=@1", pid, userid);
            //        }

            //        trans.Commit();
            //    }
            //    catch (Exception e)
            //    {
            //        trans.Rollback();
            //        throw e;
            //    }
            //    finally
            //    {
            //        if (trans != null)
            //        {
            //            trans.Dispose();
            //        }

            //        if (cnn != null)
            //        {
            //            if (cnn.State == System.Data.ConnectionState.Open)
            //            {
            //                cnn.Close();
            //            }

            //            cnn.Dispose();
            //        }
            //    }
            //    rsp["code"] = 0;
            //    rsp["status"] = "succ";
            //    Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
            //    return;
            //}

        }


        public void InitUserGiveCoupon(int userid)
        {
            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();
            var couponrules = dbh.GetDataList("select * from [sys.couponRule] where type='sys'");
            foreach (var cr in couponrules)
            {
                dbh.ExecuteNoneQuery("insert into [user.coupon] (userid,ruleid,amount,type,date,dateBegin,dateEnd,status,statusdate) values (@0,@1,@2,@3,@4,@5,@6,@7,@8);", userid, cr["id"], cr["amount"], Constant.CouponType.System.GetHashCode(), DateTime.Now, DateTime.Now.AddDays(Convert.ToInt32(cr["daysBegin"])), DateTime.Now.AddDays(Convert.ToInt32(cr["daysEnd"])), Constant.CouponStatus.Enabled.GetHashCode(), DateTime.Now);
            }
        }

        /// <summary>
        /// 更改代金券状态
        /// [POST] /open/coupon/status.do
        /// @authcode
        /// @couponid
        /// @type
        /// </summary>
        //public void coupon_status_do()
        //{
        //    var postdata = ReadBodyData();
        //    string authcode = postdata.authcode ?? string.Empty;
        //    int userid = 0;

        //    if (!TryGetUserId(authcode, out userid))
        //    {
        //        EchoFailJson("!TryGetUserId");
        //        return;
        //    }


        //    var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();

        //    string couponid = postdata.couponid ?? string.Empty;
        //    string type = postdata.type ?? string.Empty;
        //    if (string.IsNullOrEmpty(couponid) || string.IsNullOrEmpty(type))
        //    {
        //        EchoFailJson("couponid or type is null or empty");
        //        return;
        //    }
        //    if (type == "send")
        //    {
        //        var coupon = dbh.GetData("select top 1 cr.type from [user.coupon] as c left join [sys.couponRule] as cr on c.ruleid=cr.id where c.id=@0", couponid);
        //        if (Convert.ToString(coupon["type"]) != "sys")
        //        {
        //            dbh.ExecuteNoneQuery("update [user.coupon] set [status]=@0 where id=@1", Constant.CouponStatus.Giving.GetHashCode(), couponid);
        //        }
        //    }

        //    var rsp = new Common.DB.NVCollection();
        //    rsp["code"] = 0;
        //    rsp["status"] = "succ";
        //    Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
        //}
    }

}