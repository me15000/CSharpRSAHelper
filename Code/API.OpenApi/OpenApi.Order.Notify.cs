using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
namespace API
{

    /// <summary>
    /// 微信支付回调
    /// </summary>
    public partial class OpenApi
    {
        void wx_notify()
        {
            string cont = getPostStr();

            if (string.IsNullOrEmpty(cont))
            {
                EchoFailCallbackInfo();
                return;
            }

            var info = Common.Helpers.PayHelper.ParseCallbackInfo(cont);
            if (info == null)
            {
                EchoFailCallbackInfo();
                return;
            }

            bool isvalidcallback = false;
            if (info.Status == "SUCC")
            {
                bool succ = Common.Helpers.PayHelper.CheckCallbackInfo(info);
                if (succ)
                {
                    if (info.Data["result_code"] == "SUCCESS")
                    {
                        isvalidcallback = true;

                        SuccessCall(info);
                        return;
                    }
                }
            }


            if (!isvalidcallback)
            {
                EchoFailCallbackInfo();
                return;
            }

        }

        Entities.OrderInfo GetOrderInfo(string orderno)
        {
            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();

            var ent = dbh.GetData("select [orderno],[userid],[date],[amountTotal],[couponid],[amountCoupon],[amount],[status],[statusdate] from [user.order] where orderno=@0", orderno);

            if (ent != null)
            {
                return new Entities.OrderInfo(ent);
            }

            return null;
        }

        void SuccessCall(PayHelper.CallbackInfo info)
        {

            string out_trade_no = info.Data["out_trade_no"];
            if (string.IsNullOrEmpty(out_trade_no))
            {
                EchoFailCallbackInfo();
                return;
            }


            var order = GetOrderInfo(out_trade_no);
            if (order == null)
            {
                EchoFailCallbackInfo();
                return;
            }

            //订单已经是成功状态；不重复执行逻辑
            if (order.status == Constant.OrderStatus.Finish.GetHashCode().ToString())
            {
                EchoSuccessCallbackInfo();
                return;
            }

            var user = Services.UserService.GetUserInfo(order.userid);
            if (user == null)
            {
                EchoFailCallbackInfo();
                return;
            }




            #region 注释
            //if (user != null && !Convert.ToBoolean(user["olduser"]))
            //{
            ////是
            //int pid = Convert.ToInt32(user["pid"] == DBNull.Value ? "0" : user["pid"]);
            //if (pid > 0)
            //{
            //    var config = dbh.GetData("select top 1 [userpercentLimitAmount],[userpercentAmount] from [sys.config] where [enabled]=1");
            //    int userpercentLimitAmount = Convert.ToInt32(config["userpercentLimitAmount"]);
            //    int userpercentAmount = Convert.ToInt32(config["userpercentAmount"]);
            //    int orderamount = Convert.ToInt32(order["amount"]);
            //    if (orderamount >= userpercentLimitAmount)
            //    {
            //        //达到返现最低消费
            //        decimal percentamt = (decimal)orderamount * ((decimal)userpercentAmount / 100m);
            //        if (percentamt > 0)
            //        {
            //            //查询流水
            //            int amountNow = 0;
            //            int amountTotal = 0;
            //            var cash = dbh.GetData("select top 1 * from [user.cashnow] where userid=@0", pid);
            //            if (cash != null)
            //            {
            //                amountNow = Convert.ToInt32(cash["amountNow"]);
            //                amountTotal = Convert.ToInt32(cash["amountTotal"]);
            //            }

            //            int amountnext = Convert.ToInt32((decimal)amountNow + percentamt);

            //            var cashid = dbh.ExecuteScalar<int>("insert into [user.cash] ([userid],[orderno],[amount],[date],[datets],[amountNow],[amountPrev],[amountTotal],[type],[info]) values (@0,@1,@2,@3,@4,@5,@6,@7,@8,@9) select @@identity", pid, out_trade_no, percentamt, DateTime.Now, Common.Helpers.TimeHelper.GetTimeStamp(DateTime.Now, 10), amountnext, amountNow, amountTotal + percentamt, "give", "好友" + user["name"] + "到店消费");
            //            if (dbh.ExecuteNoneQuery("update [user.cashnow] set amountNow=@0,amountPrev=@1,amountTotal=@2,[date]=@3 where userid=@4", amountnext, amountNow, amountTotal + percentamt, DateTime.Now, pid) == 0)
            //            {
            //                dbh.ExecuteNoneQuery("insert into [user.cashnow] (userid,amountNow,amountPrev,amountTotal,[date]) values (@0,@1,@2,@3,@4)", pid, amountnext, amountNow, amountTotal + percentamt, DateTime.Now);
            //            }


            //            var puser = dbh.GetData("select * from [user] where id=@0", pid);
            //            XCXMsgHelper xcxmh = new XCXMsgHelper();
            //            xcxmh.SendMessage(puser["openid"].ToString(), "yOzW09wrT_VtGsqva-K8rdQq3FQEsuzMFRZRakEznJ0", "pages/friends/friends?id=" + cashid, puser["formid"].ToString(), (percentamt * (decimal)0.01).ToString("F2"), (amountNow * (decimal)0.01).ToString("F2"));
            //        }
            //    }
            //}
            //}
            #endregion

            //设置订单状态已完成支付
            SetOrderStatus(order.orderno, Constant.OrderStatus.Finish);

            //更新代金券状态
            try
            {
                if (order.couponid > 0)
                {
                    UpdateCouponStatus(order.couponid, Constant.CouponStatus.Disabled);
                }
            }
            catch (Exception ex)
            {
                //记录错误
                throw ex;
            }

            //更新用户为老用户
            try
            {

                Services.UserService.SetUserOld(user.id);
            }
            catch (Exception ex)
            {
                //记录错误
                throw ex;
            }


            //赠送代金券
            try
            {
                GiveCoupon(order);
            }
            catch (Exception ex)
            {
                //记录错误
                throw ex;
            }


            //判断是否给父用户返现
            if (order.couponid > 0)
            {
                ParentUserOrderCash(user, order);
            }

            //dbh.ExecuteNoneQuery("insert into logc values (@0)", "测试");
            EchoSuccessCallbackInfo();

        }

        void SetOrderStatus(string orderno, Constant.OrderStatus status)
        {
            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();

            dbh.ExecuteNoneQuery("update [user.order] set status=@0,statusdate=@1 where orderno=@2;"
                , status.GetHashCode()
                , DateTime.Now
                , orderno);
        }

        //判断是否给用户赠送代金券
        //本过程应该用事务处理
        void GiveCoupon(Entities.OrderInfo order)
        {
            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();

            //判断是否达到消费金额
            var couponRule = dbh.GetData("select top 1 id,amount,daysBegin,daysEnd from [sys.couponRule] where amountMin<=@0 and amountMax>=@0 and [type]='normal' and [status]=1", order.amount);

            if (couponRule == null)
            {
                return;
            }

            int ruleid = ConvertHelper.ToInt32(couponRule["id"]);
            int ruleAmount = ConvertHelper.ToInt32(couponRule["amount"]);
            int daysBegin = ConvertHelper.ToInt32(couponRule["daysBegin"]);
            int daysEnd = ConvertHelper.ToInt32(couponRule["daysEnd"]);
            DateTime now = DateTime.Now;

            var conn = dbh.GetConnection();
            System.Data.Common.DbTransaction tran = null;

            try
            {


                conn.Open();
                tran = conn.BeginTransaction();

                var cmdExists = dbh.CreateCommand();
                cmdExists.Connection = conn;
                cmdExists.Transaction = tran;
                cmdExists.CommandText = "select top 1 1 from [user.coupon] where orderno=@orderno";
                cmdExists.Parameters.Add(dbh.CreateParameter("@orderno", order.orderno));
                object exo = cmdExists.ExecuteScalar();
                if (exo != null && exo != DBNull.Value)
                {
                    tran.Commit();
                }
                else
                {
                    var cmdInsert = dbh.CreateCommand();
                    cmdInsert.Connection = conn;
                    cmdInsert.Transaction = tran;
                    cmdInsert.CommandText = "insert into [user.coupon] (userid,ruleid,amount,type,date,dateBegin,dateEnd,status,statusdate,pid,orderno) values (@userid,@ruleid,@amount,@type,@date,@dateBegin,@dateEnd,@status,@statusdate,@pid,@orderno);";

                    cmdInsert.Parameters.Add(dbh.CreateParameter("@userid", order.userid));
                    cmdInsert.Parameters.Add(dbh.CreateParameter("@ruleid", ruleid));
                    cmdInsert.Parameters.Add(dbh.CreateParameter("@amount", ruleAmount));
                    cmdInsert.Parameters.Add(dbh.CreateParameter("@type", Constant.CouponType.Order.GetHashCode()));
                    cmdInsert.Parameters.Add(dbh.CreateParameter("@date", now));

                    cmdInsert.Parameters.Add(dbh.CreateParameter("@dateBegin", now.AddDays(daysBegin)));
                    cmdInsert.Parameters.Add(dbh.CreateParameter("@dateEnd", now.AddDays(daysEnd)));

                    cmdInsert.Parameters.Add(dbh.CreateParameter("@status", Constant.CouponStatus.Enabled.GetHashCode()));
                    cmdInsert.Parameters.Add(dbh.CreateParameter("@statusdate", now));
                    cmdInsert.Parameters.Add(dbh.CreateParameter("@pid", 0));
                    cmdInsert.Parameters.Add(dbh.CreateParameter("@orderno", order.orderno));

                    int rnum = cmdInsert.ExecuteNonQuery();

                    tran.Commit();
                }

            }
            catch (Exception ex)
            {
                tran.Rollback();
                throw ex;
            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                }

                if (conn != null)
                {
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }

                    conn.Dispose();
                }
            }

            //判断该订单是否赠送过代金券
            //if (Convert.ToInt32(dbh.ExecuteScalar<object>("select top 1 1 from [user.coupon] where orderno=@0", order.orderno)) != 1)
            //{
            //    dbh.ExecuteNoneQuery("insert into [user.coupon] (userid,ruleid,amount,type,date,dateBegin,dateEnd,status,statusdate,pid,orderno) values (@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10);", order.userid, couponRule["id"], couponRule["amount"], Constant.CouponType.Order.GetHashCode(), DateTime.Now, DateTime.Now, DateTime.Now.AddDays(30), Constant.CouponStatus.Enabled.GetHashCode(), DateTime.Now, 0, order.orderno);
            //}

        }

        //判断是否给父用户返现
        //本过程应该用事务处理
        void ParentUserOrderCash(Entities.UserInfo user, Entities.OrderInfo order)
        {
            if (order.couponid <= 0)
            {
                return;
            }

            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();

            var coupon = dbh.GetData("select pid from [user.coupon] where id=@0", order.couponid);



            //判断该代金券是不是别人送的
            int pid = ConvertHelper.ToInt32(coupon["pid"]);
            if (pid <= 0)
            {
                return;
            }

            //用户不可以自己给自己返现
            if (pid == user.id)
            {
                return;
            }

            //判断该笔订单是否已经返现
            object exists_cash_obj = dbh.ExecuteScalar<object>("select top 1 1 from [user.cash] where orderno=@0", order.orderno);
            bool exists_cash = ConvertHelper.ToInt32(exists_cash_obj) > 0;
            if (exists_cash)
            {
                return;
            }



            var config = dbh.GetData("select top 1 [userpercentLimitAmount],[userpercentAmount] from [sys.config] where [enabled]=1");


            int userpercentLimitAmount = Convert.ToInt32(config["userpercentLimitAmount"]);
            int userpercentAmount = Convert.ToInt32(config["userpercentAmount"]);

            //订单金额小于最小返现金额
            if (order.amount < userpercentLimitAmount)
            {
                throw new Exception("订单金额小于最小返现金额");
                return;
            }


            //达到返现最低消费
            int percentamt = Convert.ToInt32((decimal)order.amount * ((decimal)userpercentAmount / 100m));
            if (percentamt <= 0)
            {
                throw new Exception("达到返现最低消费");
                return;
            }

            var now = DateTime.Now;


            var conn = dbh.GetConnection();
            System.Data.Common.DbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();


                var amountCmd = dbh.CreateCommand();
                amountCmd.Connection = conn;
                amountCmd.Transaction = tran;
                amountCmd.CommandText = "select top 1 amountNow,amountTotal from [user.cashnow] where userid=@userid";
                amountCmd.Parameters.Add(dbh.CreateParameter("@userid", pid));


                //查询流水
                int amountNow = 0;
                int amountTotal = 0;
                using (var reader = amountCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        amountNow = reader.GetInt32(0);
                        amountTotal = reader.GetInt32(1);
                    }

                }


                int amountNext = amountNow + percentamt;
                var cmdExe = dbh.CreateCommand();
                cmdExe.Transaction = tran;
                cmdExe.Connection = conn;
                cmdExe.CommandText = "insert into [user.cash] ([userid],[orderno],[amount],[date],[datets],[amountNow],[amountPrev],[amountTotal],[type],[info]) values (@userid,@orderno,@amount,@date,@datets,@amountNow,@amountPrev,@amountTotal,@type,@info) select @@identity";

                cmdExe.Parameters.Add(dbh.CreateParameter("@userid", pid));
                cmdExe.Parameters.Add(dbh.CreateParameter("@orderno", order.orderno));
                cmdExe.Parameters.Add(dbh.CreateParameter("@amount", percentamt));
                cmdExe.Parameters.Add(dbh.CreateParameter("@date", now));
                cmdExe.Parameters.Add(dbh.CreateParameter("@datets", Common.Helpers.TimeHelper.GetTimeStamp(now, 10)));

                //amountNext, amountNow, amountTotal + percentamt
                cmdExe.Parameters.Add(dbh.CreateParameter("@amountNow", amountNext));
                cmdExe.Parameters.Add(dbh.CreateParameter("@amountPrev", amountNow));
                cmdExe.Parameters.Add(dbh.CreateParameter("@amountTotal", amountTotal + percentamt));
                cmdExe.Parameters.Add(dbh.CreateParameter("@type", "give"));
                cmdExe.Parameters.Add(dbh.CreateParameter("@info", "好友" + user.name + "到店消费"));
                object idobj = cmdExe.ExecuteScalar();

                int cashid = 0;

                if (idobj != null && idobj != DBNull.Value)
                {
                    cashid = Convert.ToInt32(idobj);
                }


                var cmdUpdate = dbh.CreateCommand();
                cmdUpdate.Transaction = tran;
                cmdUpdate.Connection = conn;
                cmdUpdate.CommandText = "update [user.cashnow] set amountNow=@amountNow,amountPrev=@amountPrev,amountTotal=@amountTotal,[date]=@date where userid=@userid";

                cmdUpdate.Parameters.Add(dbh.CreateParameter("@userid", pid));
                cmdUpdate.Parameters.Add(dbh.CreateParameter("@amountNow", amountNext));
                cmdUpdate.Parameters.Add(dbh.CreateParameter("@amountPrev", amountNow));
                cmdUpdate.Parameters.Add(dbh.CreateParameter("@amountTotal", amountTotal + percentamt));
                cmdUpdate.Parameters.Add(dbh.CreateParameter("@date", now));

                int cmdUN = cmdUpdate.ExecuteNonQuery();
                if (cmdUN == 0)
                {
                    var cmdInsert = dbh.CreateCommand();
                    cmdInsert.Transaction = tran;
                    cmdInsert.Connection = conn;
                    cmdInsert.CommandText = "insert into [user.cashnow] (userid,amountNow,amountPrev,amountTotal,[date]) values (@userid,@amountNow,@amountPrev,@amountTotal,@date)";

                    cmdInsert.Parameters.Add(dbh.CreateParameter("@userid", pid));
                    cmdInsert.Parameters.Add(dbh.CreateParameter("@amountNow", amountNext));
                    cmdInsert.Parameters.Add(dbh.CreateParameter("@amountPrev", amountNow));
                    cmdInsert.Parameters.Add(dbh.CreateParameter("@amountTotal", amountTotal + percentamt));
                    cmdInsert.Parameters.Add(dbh.CreateParameter("@date", now));

                    cmdInsert.ExecuteNonQuery();
                }


                tran.Commit();


                new Thread(() =>
                {
                    SendCashMessage(pid, cashid, percentamt, amountNow);

                }).Start();



            }
            catch (Exception e)
            {

                tran.Rollback();

                throw e;
            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                }

                if (conn != null)
                {
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }

                    conn.Dispose();
                }
            }



            //var cash = dbh.GetData("select top 1 amountNow,amountTotal from [user.cashnow] where userid=@0", pid);
            //if (cash != null)
            //{
            //    amountNow = Convert.ToInt32(cash["amountNow"]);
            //    amountTotal = Convert.ToInt32(cash["amountTotal"]);
            //}



            //var cashid = dbh.ExecuteScalar<int>("insert into [user.cash] ([userid],[orderno],[amount],[date],[datets],[amountNow],[amountPrev],[amountTotal],[type],[info]) values (@0,@1,@2,@3,@4,@5,@6,@7,@8,@9) select @@identity", pid, order.orderno, percentamt, DateTime.Now, Common.Helpers.TimeHelper.GetTimeStamp(DateTime.Now, 10), amountNext, amountNow, amountTotal + percentamt, "give", "好友" + user.name + "到店消费");

            //if (dbh.ExecuteNoneQuery("update [user.cashnow] set amountNow=@0,amountPrev=@1,amountTotal=@2,[date]=@3 where userid=@4", amountnext, amountNow, amountTotal + percentamt, DateTime.Now, pid) == 0)
            //{
            //    dbh.ExecuteNoneQuery("insert into [user.cashnow] (userid,amountNow,amountPrev,amountTotal,[date]) values (@0,@1,@2,@3,@4)", pid, amountnext, amountNow, amountTotal + percentamt, DateTime.Now);
            //}



        }


        void SendCashMessage(int userid, int cashid, int percentamt, int amountNow)
        {
            var puser = Services.UserService.GetUserInfo(userid);

            XCXMsgHelper xcxmh = new XCXMsgHelper();
            xcxmh.SendMessage(puser.openid, "yOzW09wrT_VtGsqva-K8rdQq3FQEsuzMFRZRakEznJ0", "pages/friends/friends?id=" + cashid, puser.formid, (percentamt * (decimal)0.01).ToString("F2"), (amountNow * (decimal)0.01).ToString("F2"));
        }

        void EchoSuccessCallbackInfo()
        {
            Response.Write(Common.Helpers.PayHelper.SuccessCallbackInfo);
        }

        void EchoFailCallbackInfo()
        {
            Response.Write(Common.Helpers.PayHelper.FailCallbackInfo);
        }

        public string getPostStr()
        {
            Int32 intLen = Convert.ToInt32(System.Web.HttpContext.Current.Request.InputStream.Length);
            byte[] b = new byte[intLen];
            System.Web.HttpContext.Current.Request.InputStream.Read(b, 0, intLen);
            return System.Text.Encoding.UTF8.GetString(b);
        }
    }

}