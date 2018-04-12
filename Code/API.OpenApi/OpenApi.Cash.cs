using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
namespace API
{
    /// <summary>
    /// OpenApi 用户提现相关操作
    /// </summary>
    public partial class OpenApi
    {
        /// <summary>
        /// 获得用户当前账户收益信息
        /// [GET] /open/cash/now.json
        /// @authcode
        /// @id
        /// </summary>
        public void cash_now_json()
        {
            var rsp = new Common.DB.NVCollection();
            string authcode = Request.QueryString["authcode"];
           

            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();

            int userid = 0;
            if (!TryGetUserId(authcode, out userid))
            {
                EchoFailJson("!TryGetUserId");
                return;
            }


            int id = Convert.ToInt32(Request.QueryString["id"]);
            if (id == 0)
            {
                var data = dbh.GetData("select top 1 amount,date,datets,amountNow,amountPrev,amountTotal,[type],info from [user.cash] where userid=@0 order by datets desc", userid);
                if (data == null)
                {
                    //error
                    rsp["code"] = 0;
                    rsp["status"] = "succ";
                    rsp["data"] = null;
                    Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
                    return;
                }
                data["date"] = Convert.ToDateTime(data["date"]).ToString("yyyy-MM-dd");
                rsp["code"] = 0;
                rsp["status"] = "succ";
                rsp["data"] = data;
                Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
                return;
            }
            else
            {
                var data = dbh.GetData("select top 1 amount,date,datets,amountNow,amountPrev,amountTotal,[type],info from [user.cash] where id=@0", id);
                if (data == null)
                {
                    //error
                    rsp["code"] = 0;
                    rsp["status"] = "succ";
                    rsp["data"] = null;
                    Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
                    return;
                }
                data["date"] = Convert.ToDateTime(data["date"]).ToString("yyyy-MM-dd");
                rsp["code"] = 0;
                rsp["status"] = "succ";
                rsp["data"] = data;
                Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
                return;
            }
        }

        /// <summary>
        /// 获得用户收益流水记录
        /// [GET] /open/cash/list.json
        /// @authcode
        /// @ts
        /// </summary>
        public void cash_list_json()
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


            long ts = Convert.ToInt64(Request.QueryString["ts"]);
            int pagesize = 30;
            var data = dbh.GetDataList("select top " + pagesize + " amount,date,datets,amountNow,amountPrev,amountTotal,[type],info from [user.cash] where userid=@0" + (ts > 0 ? " and datets<@1 order by datets desc" : ""), userid, ts);
            foreach (var d in data)
            {
                d["date"] = Convert.ToDateTime(d["date"]).ToString("yyyy-MM-dd");
            }
            rsp["code"] = 0;
            rsp["status"] = "succ";
            rsp["data"] = data;

            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
            return;
        }

        /// <summary>
        /// 发起提现
        /// [GET] /open/user/cash/cash.do
        /// @authcode        
        /// </summary>
        public void user_cash_cash_do()
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



            var bank = dbh.GetData("select * from [user.bankcard] where userid=@0", userid);
            if (bank == null)
            {
                EchoFailJson("bankcard not exists");
                return;
            }

            var usercashnow = dbh.GetData("select top 1 * from [user.cashnow] where userid=@0", userid);
            int amount = Convert.ToInt32(usercashnow["amountNow"]);

            var fee = (amount * 0.001);
            if (fee < 100)
            {
                fee = 100;
            }

            if (usercashnow == null && (int)(amount - fee) <= 0)
            {
                EchoFailJson("cashnow not exists");
                return;
            }

            string partner_trade_no = Guid.NewGuid().ToString("N");

            //error - 改为事务
            #region
            var cnn = dbh.GetConnection();
            cnn.Open();
            DbTransaction trans = cnn.BeginTransaction();

            DbCommand sqlcmd_insert_usercash = dbh.CreateCommand();
            sqlcmd_insert_usercash.Connection = cnn;
            sqlcmd_insert_usercash.Transaction = trans;

            DbCommand sqlcmd_update_cashnow = dbh.CreateCommand();
            sqlcmd_update_cashnow.Connection = cnn;
            sqlcmd_update_cashnow.Transaction = trans;


            DbCommand sqlcmd_insert_bankcash = dbh.CreateCommand();
            sqlcmd_insert_bankcash.Connection = cnn;
            sqlcmd_insert_bankcash.Transaction = trans;


            DbCommand sqlcmd_update_bankcash = dbh.CreateCommand();
            sqlcmd_update_bankcash.Connection = cnn;
            sqlcmd_update_bankcash.Transaction = trans;

            int usercashid = 0;
            try
            {
                sqlcmd_insert_usercash.CommandText = "insert into [user.cash] ([userid],[orderno],[amount],[date],[datets],[amountNow],[amountPrev],[amountTotal],[type],[info]) values (@0,@1,@2,@3,@4,@5,@6,@7,@8,@9) select @@identity;";
                sqlcmd_insert_usercash.Parameters.Add(dbh.CreateParameter("0", userid));
                sqlcmd_insert_usercash.Parameters.Add(dbh.CreateParameter("1", partner_trade_no));
                sqlcmd_insert_usercash.Parameters.Add(dbh.CreateParameter("2", amount * -1));
                sqlcmd_insert_usercash.Parameters.Add(dbh.CreateParameter("3", DateTime.Now));
                sqlcmd_insert_usercash.Parameters.Add(dbh.CreateParameter("4", Common.Helpers.TimeHelper.GetTimeStamp(DateTime.Now, 10)));
                sqlcmd_insert_usercash.Parameters.Add(dbh.CreateParameter("5", 0));
                sqlcmd_insert_usercash.Parameters.Add(dbh.CreateParameter("6", amount));
                sqlcmd_insert_usercash.Parameters.Add(dbh.CreateParameter("7", usercashnow["amountTotal"]));
                sqlcmd_insert_usercash.Parameters.Add(dbh.CreateParameter("8", "withdraw"));
                sqlcmd_insert_usercash.Parameters.Add(dbh.CreateParameter("9", "提现到银行卡"));
                usercashid =  Convert.ToInt32(sqlcmd_insert_usercash.ExecuteScalar());
                if (usercashid == 0)
                {
                    trans.Rollback();
                    EchoFailJson("usercashid == 0");
                    return;
                }


                sqlcmd_update_cashnow.CommandText = "update [user.cashnow] set amountNow=@0,amountPrev=@1,amountTotal=@2 where userid=@3";
                sqlcmd_update_cashnow.Parameters.Add(dbh.CreateParameter("0", 0));
                sqlcmd_update_cashnow.Parameters.Add(dbh.CreateParameter("1", amount));
                sqlcmd_update_cashnow.Parameters.Add(dbh.CreateParameter("2", usercashnow["amountTotal"]));
                sqlcmd_update_cashnow.Parameters.Add(dbh.CreateParameter("3", userid));
                var n = sqlcmd_update_cashnow.ExecuteNonQuery();
                if (n == 0)
                {
                    trans.Rollback();
                    EchoFailJson("update user.cashnow fail");
                    return;
                }

                sqlcmd_insert_bankcash.CommandText = "insert into [user.bankcard.cash] ([cashno],[userid],[bankcode],[banknumber],[name],[amount],[status],[statusdate],[date]) values (@0,@1,@2,@3,@4,@5,@6,@7,@8)";
                sqlcmd_insert_bankcash.Parameters.Add(dbh.CreateParameter("0", partner_trade_no));
                sqlcmd_insert_bankcash.Parameters.Add(dbh.CreateParameter("1", userid));
                sqlcmd_insert_bankcash.Parameters.Add(dbh.CreateParameter("2", bank["bank"]));
                sqlcmd_insert_bankcash.Parameters.Add(dbh.CreateParameter("3", bank["number"]));
                sqlcmd_insert_bankcash.Parameters.Add(dbh.CreateParameter("4", bank["name"]));
                sqlcmd_insert_bankcash.Parameters.Add(dbh.CreateParameter("5", amount));
                sqlcmd_insert_bankcash.Parameters.Add(dbh.CreateParameter("6", "start"));
                sqlcmd_insert_bankcash.Parameters.Add(dbh.CreateParameter("7", DateTime.Now));
                sqlcmd_insert_bankcash.Parameters.Add(dbh.CreateParameter("8", DateTime.Now));
                n = sqlcmd_insert_bankcash.ExecuteNonQuery();
                if (n == 0)
                {
                    trans.Rollback();
                    EchoFailJson("insert user.bankcard.cash fail");
                    return;
                }

                bool r = Common.Helpers.PayToBankHelper.PayToBank((int)(amount - fee), partner_trade_no, Convert.ToString(bank["number"]), Convert.ToString(bank["name"]), Convert.ToString(bank["bank"]));

                if (r) {
                    sqlcmd_update_bankcash.CommandText = "update [user.bankcard.cash] set [status]='succ' where [cashno]=@0";
                    sqlcmd_update_bankcash.Parameters.Add(dbh.CreateParameter("0", partner_trade_no));
                    n= sqlcmd_update_bankcash.ExecuteNonQuery();
                    if (n == 0)
                    {
                        trans.Rollback();
                        EchoFailJson("update user.bankcard.cash fail");
                        return;
                    }
                }
                else
                {
                    trans.Rollback();
                    EchoFailJson("paytobank fail");
                    return;
                }


                trans.Commit();
                rsp["code"] = 0;
                rsp["status"] = "succ";
                Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
            }
            catch(Exception e)
            {
                trans.Rollback();
                throw e;
            }
            finally
            {
                if (trans != null)
                {
                    trans.Dispose();
                }

                if (cnn != null)
                {
                    if (cnn.State == System.Data.ConnectionState.Open)
                    {
                        cnn.Close();
                    }

                    cnn.Dispose();
                }
            }
            #endregion
        }
    }

}