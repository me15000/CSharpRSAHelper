using System;
using System.Collections.Generic;
using System.Web;

namespace API
{
    /// <summary>
    /// OpenApi 银行卡相关操作
    /// </summary>
    public partial class OpenApi
    {
        /// <summary>
        /// 用户绑定银行卡,用户更改银行卡
        /// [POST] /open/user/bankcard/bind.do
        /// @authcode
        /// @name
        /// @bank
        /// @code
        /// </summary>
        public void user_bankcard_bind_do()
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


            string name = postdata.name;
            string bank = postdata.bank;
            string code = postdata.code;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(bank) || string.IsNullOrEmpty(code))
            {
                EchoFailJson();
                return;
            }

            int n = dbh.ExecuteNoneQuery("update [user.bankcard] set  bank=@0,number=@1,name=@2,date=@3,statusdate=@4 where userid=@5", bank, code, name, DateTime.Now, DateTime.Now, userid);

            if (n < 1)
            {
                dbh.ExecuteNoneQuery("insert into [user.bankcard] (userid,bank,number,name,date,statusdate,status) values (@0,@1,@2,@3,@4,@5,@6)", userid, bank, code, name, DateTime.Now, DateTime.Now, 1);
            }

            var rsp = new Common.DB.NVCollection();

            rsp["code"] = 0;
            rsp["status"] = "succ";
            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
        }

        /// <summary>
        /// 查询用户当前绑定的银行卡
        /// [GET] /open/user/bankcard/bind.json
        /// @authcode
        /// </summary>
        public void user_bankcard_bind_json()
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

  
            var banks = new Dictionary<string, string>();
            banks["1002"] = "工商银行";
            banks["1005"] = "农业银行";
            banks["1026"] = "中国银行";
            banks["1003"] = "建设银行";
            banks["1001"] = "招商银行";
            banks["1066"] = "邮储银行";
            banks["1020"] = "交通银行";
            banks["1004"] = "浦发银行";
            banks["1006"] = "民生银行";
            banks["1009"] = "兴业银行";
            banks["1010"] = "平安银行";
            banks["1021"] = "中信银行";
            banks["1025"] = "华夏银行";
            banks["1027"] = "广发银行";
            banks["1022"] = "光大银行";
            banks["1032"] = "北京银行";
            banks["1056"] = "宁波银行";

            var bank = dbh.GetData("select top 1 number,bank from [user.bankcard] where userid=@0", userid);
            if (bank == null)
            {
                EchoFailJson("bankcard is null");
                return;
            }

            var data = new Common.DB.NVCollection();
            var type = new Common.DB.NVCollection();
            type["key"] = Convert.ToString(bank["bank"]);
            type["name"] = banks[type["key"].ToString()];

            data["type"] = type;
            data["code"] = bank["number"];

            rsp["code"] = 0;
            rsp["status"] = "succ";
            rsp["data"] = data;
            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
        }

        /// <summary>
        /// 银行卡类型
        /// [GET] /open/user/bankcard/types.json
        /// @authcode
        /// </summary>
        public void user_bankcard_types_json()
        {

            var rsp = new Common.DB.NVCollection();
            string authcode = Request.QueryString["authcode"];
            int userid = 0;

            if (!TryGetUserId(authcode, out userid))
            {
                EchoFailJson("!TryGetUserId");
                return;
            }

           

            Response.Write(System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("/bankcode.json")));
        }
    }

}