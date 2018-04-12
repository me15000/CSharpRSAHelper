
using System;
using System.Web;
using System.IO;
using Newtonsoft.Json;

namespace API
{
    /// <summary>
    /// 主入口
    /// </summary>
    public partial class OpenApi : IHttpHandler
    {
        HttpRequest Request;
        HttpResponse Response;



        public void ProcessRequest(HttpContext context)
        {
            Request = context.Request;
            Response = context.Response;


            switch (Request.PathInfo)
            {
                case "/test":
                    {
                        Common.Helpers.PayToBankHelper.PayToBank(100, "test11111" + DateTime.Now.ToString("HHmmss"), "6212261102031969796", "范红朕", "1002");
                    }
                    break;

                //----------OpenApi.Sys.cs
                case "/module/data.json":
                    module_data_json();
                    break;

                case "/sys/shop/info.json":
                    sys_shop_info_json();
                    break;

                //----------OpenApi.User.cs
                case "/user/auth.json":
                    user_auth_json();
                    break;

                case "/friend/pid.do":
                    friend_pid_do();
                    break;

                case "/user/info.do":
                    user_info_do();
                    break;

                case "/user/info.json":
                    user_info_json();
                    break;


                case "/user/formid.do":
                    user_formid_do();
                    break;



                //----------OpenApi.Order.cs

                case "/order/order.do":
                    order_order_do();
                    break;
                case "/order/info.json":
                    order_info_json();
                    break;
                case "/order/list.json":
                    order_list_json();
                    break;

                case "/wx/notify.do":
                    wx_notify();
                    break;

                //----------OpenApi.Coupon.cs
                case "/coupon/list.json":
                    coupon_list_json();
                    break;
                case "/sys/coupon/list.json":
                    sys_coupon_list_json();
                    break;
                case "/sys/coupon/info.json":
                    sys_coupon_info_json();
                    break;
                case "/coupon/info.json":
                    coupon_info_json();
                    break;
                case "/coupon/get.do":
                    coupon_get_do();
                    break;
                //case "/coupon/status.do":
                //    coupon_status_do();
                //    break;

                //----------OpenApi.Cash.cs
                case "/cash/now.json":
                    cash_now_json();
                    break;

                case "/cash/list.json":
                    cash_list_json();
                    break;

                case "/user/cash/cash.do":
                    user_cash_cash_do();
                    break;


                //----------OpenApi.Bankcard.cs
                case "/user/bankcard/bind.do":
                    user_bankcard_bind_do();
                    break;

                case "/user/bankcard/bind.json":
                    user_bankcard_bind_json();
                    break;

                case "/user/bankcard/types.json":
                    user_bankcard_types_json();
                    break;


                default:
                    Response.Write(Common.Helpers.PayToBankHelper.GetPublicKey());
                    break;
            }
        }


        bool TryGetUserId(string authcode, out int userid)
        {
            userid = 0;

            if (string.IsNullOrEmpty(authcode))
            {
                //EchoFailJson("authcode is null or empty");
                return false;
            }

            dynamic ui = Services.UserService.DecodeUserAuthCode(authcode);

            if (ui == null)
            {
                //EchoFailJson("DecodeUserAuthCode fail");

                return false;
            }

            userid = ui.userid;

            if (userid <= 0)
            {
                //EchoFailJson("userid error");

                return false;
            }


            if (Services.UserService.ExistsUserId(userid))
            {
                return true;
            }
            else
            {
                //EchoFailJson("userid not exists");
                return false;
                
            }

            //return false;
        }

        void EchoFailJson(string msg = null)
        {
            var rsp = new Common.DB.NVCollection();

            rsp["code"] = -1;
            rsp["status"] = "fail";
            if (!string.IsNullOrEmpty(msg))
            {
                rsp["msg"] = msg;
            }

            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
        }

        dynamic ReadBodyData()
        {

            using (var sr = new StreamReader(Request.InputStream))
            {
                string jsonr = sr.ReadToEnd();

                if (!string.IsNullOrEmpty(jsonr))
                {
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(jsonr);

                    if (data != null)
                    {
                        return data;
                    }
                }

            }

            return null;

        }

        string GetClientIP()
        {
            var Request = HttpContext.Current.Request;

            string ip = null;

            ip = Request.Headers["Cdn-Src-Ip"] ?? string.Empty;
            if (string.IsNullOrEmpty(ip))
            {
                if (Request.ServerVariables["HTTP_VIA"] != null)
                {
                    ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (ip == null)
                    {
                        ip = Request.ServerVariables["REMOTE_ADDR"];
                    }
                }
                else
                {
                    ip = Request.ServerVariables["REMOTE_ADDR"];
                }

                if (string.Compare(ip, "unknown", true) == 0)
                {
                    return Request.UserHostAddress;
                }
            }

            return ip;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

    }


}