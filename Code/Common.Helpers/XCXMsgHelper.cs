using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
namespace Common.Helpers
{
    /// <summary>
    /// XCXMsgHelper 的摘要说明
    /// </summary>
    public class XCXMsgHelper
    {
        public string GetAccessToken()
        {
            var config = Config.XCX;
            string url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + config.AppID + "&secret=" + config.AppSecret;

            string tokenString = Common.Helpers.CacheHelper.GetCacheObject<string>("cache_access_token_skn", 3600, () =>
            {
                using (var wc = new WebClient())
                {
                    byte[] data = wc.DownloadData(url);

                    string json = Encoding.GetEncoding("utf-8").GetString(data);

                    if (!string.IsNullOrEmpty(json))
                    {
                        dynamic dydata = JsonConvert.DeserializeObject<dynamic>(json);

                        if (dydata != null)
                        {
                            string token = dydata.access_token;

                            return token;
                        }
                    }
                    wc.Dispose();
                }

                return null;

            });

            return tokenString;
        }

        public string SendMessage(string touser, string templateId, string page, string formId, string amount, string totalAmt)
        {
            string token = GetAccessToken();

            string url = "https://api.weixin.qq.com/cgi-bin/message/wxopen/template/send?access_token=" + token;

            string json = @"{
        ""touser"": """ + touser + @""",
        ""template_id"": """ + templateId + @""",
        ""page"": """ + page + @""",
        ""form_id"": """ + formId + @""",
        ""data"": {
            ""keyword1"": {
                ""value"": ""恭喜获得收益"",
   ""color"": ""#ff0000""
            },
            ""keyword2"": {
                ""value"": """ + DateTime.Now.ToString("yyyy/MM/dd HH:mm") + @""",
   ""color"": ""#000000""
            },
            ""keyword3"": {
                ""value"": ""您邀请好友到店消费获得收益" + amount + "￥，当前可提现余额" + totalAmt + @"￥"",
   ""color"": ""#000000""
            },
            ""keyword4"": {
                ""value"": """ + amount + @"元"",
   ""color"": ""#000000""
            }
        },
        ""emphasis_keyword"": ""keyword1.DATA""
        }";
            //System.Web.HttpContext.Current.Response.Write(json);
            var enc = Encoding.GetEncoding("utf-8");

            using (var wc = new WebClient())
            {
                byte[] result = wc.UploadData(url, enc.GetBytes(json));

                string resultStirng = enc.GetString(result);

                return resultStirng;
            }
        }
    }

}