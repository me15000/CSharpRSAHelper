using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Web;

namespace Common.Helpers
{
    /// <summary>
    /// PayHelper 的摘要说明
    /// </summary>
    public class PayHelper
    {

        public class CallbackInfo
        {
            public string Status { get; set; }
            public Dictionary<string, string> Data { get; set; }
        }




        public static bool CheckCallbackInfo(CallbackInfo info)
        {

            var keys = info.Data.Keys;
            var dict = new Dictionary<string, string>();

            foreach (var key in keys)
            {
                if (key.Equals("sign"))
                {
                    continue;
                }
                dict[key] = info.Data[key];
            }

            //string[] keys = new string[] { "appid", "bank_type", "cash_fee", "fee_type", "is_subscribe", "mch_id", "nonce_str", "openid", "out_trade_no", "result_code", "return_code", "time_end", "total_fee", "trade_type", "transaction_id" };
            //foreach (var key in keys)
            //{
            //    dict[key] = info.Data[key];
            //}

            var config = Config.XCX;

            string sign = GetSignString(dict, config.mch_secret);

            if (sign.Equals(info.Data["sign"], StringComparison.OrdinalIgnoreCase))
            {

                return true;
            }

            return false;
        }

        public static CallbackInfo ParseCallbackInfo(string xmlString)
        {
            var xml = new XmlDocument();
            xml.LoadXml(xmlString);

            if (xml == null)
            {
                return null;
            }

            var callbackInfo = new CallbackInfo();

            string return_code = xml.SelectSingleNode("/xml/return_code").InnerText;

            if (return_code == "SUCCESS")
            {
                callbackInfo.Status = "SUCC";

                var dict = new Dictionary<string, string>();

                var nodes = xml.DocumentElement.ChildNodes;
                for (int i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    dict[node.Name] = node.InnerText;
                }
                callbackInfo.Data = dict;

            }
            else if (return_code == "FAIL")
            {

                callbackInfo.Status = "FAIL";
                callbackInfo.Data = null;
            }

            return callbackInfo;
        }
        /*                            
         * Response.Write("<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>");
         */
        public static string SuccessCallbackInfo
        {
            get
            {
                var dic = new Dictionary<string, string>
                {
                    {"return_code", "SUCCESS"},
                    {"return_msg","OK"}
                };

                var sb = new StringBuilder();
                sb.Append("<xml>");
                foreach (var d in dic)
                {
                    sb.Append("<" + d.Key + ">" + d.Value + "</" + d.Key + ">");
                }
                sb.Append("</xml>");

                return sb.ToString();
            }
        }

        public static string FailCallbackInfo
        {
            get
            {
                var dic = new Dictionary<string, string>
                {
                    {"return_code", "FAIL"},
                    {"return_msg","FAIL"}
                };

                var sb = new StringBuilder();
                sb.Append("<xml>");
                foreach (var d in dic)
                {
                    sb.Append("<" + d.Key + ">" + d.Value + "</" + d.Key + ">");
                }
                sb.Append("</xml>");

                return sb.ToString();
            }
        }



        public PayHelper()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        static string GetRandomString(int CodeCount)
        {
            string allChar = "1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,i,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            string[] allCharArray = allChar.Split(',');
            string RandomCode = "";
            int temp = -1;
            Random rand = new Random();
            for (int i = 0; i < CodeCount; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(temp * i * ((int)DateTime.Now.Ticks));
                }
                int t = rand.Next(allCharArray.Length - 1);
                while (temp == t)
                {
                    t = rand.Next(allCharArray.Length - 1);
                }
                temp = t;
                RandomCode += allCharArray[t];
            }

            return RandomCode;
        }

        public class OrderInfo
        {
            /// <summary>
            /// 用户OpenID
            /// </summary>
            public string OpenID { get; set; }

            /// <summary>
            /// 订单号
            /// </summary>
            public string OrderNo { get; set; }

            /// <summary>
            /// 总金额
            /// </summary>
            public int TotalFee { get; set; }

            public string Type { get; set; }


        }


        public static string GetOrderNo(int userid)
        {
            return userid.ToString() + "X" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + (new Random()).Next(999).ToString();
        }


        public class OrderResult
        {
            public string Status { get; set; }

            public Dictionary<string, string> Data { get; set; }
        }



        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }




        public OrderResult Order(string orderno, int orderamt, string notifyurl, string ordername, string openid, string ip, string attach)
        {
            //参考代码
            //http://blog.csdn.net/suixufeng/article/details/71215461


            var config = Config.XCX;


            string notifyUrl = notifyurl;


            string appid = config.AppID;
            string mch_id = config.mch_id;

            string body = ordername; //商品描述


            string notify_url = notifyUrl; //通知地址

            string spbill_create_ip = ip;//终端IP

            string total_fee = orderamt.ToString();//总金额，单位分


            //订单号
            string out_trade_no = orderno;


            var dic = new Dictionary<string, string>
            {
                {"appid", appid},
                {"mch_id", mch_id},
                {"nonce_str", GetRandomString(20)/*Random.Next().ToString()*/},
                {"body",body},
                {"out_trade_no",out_trade_no},//商户自己的订单号码  
                {"total_fee",total_fee},
                {"spbill_create_ip",spbill_create_ip},//服务器的IP地址  
                {"notify_url",notify_url},//异步通知的地址，不能带参数  
                {"trade_type","JSAPI" },
                {"attach", attach },
                {"openid",openid}
            };
            dic.Add("sign", GetSignString(dic, config.mch_secret));

            var sb = new StringBuilder();
            sb.Append("<xml>");
            foreach (var d in dic)
            {
                sb.Append("<" + d.Key + "><![CDATA[" + d.Value + "]]></" + d.Key + ">");
            }
            sb.Append("</xml>");


            string postXML = sb.ToString();

          
         
            var enc = Encoding.GetEncoding("UTF-8");

            string resultXML = null;

            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            using (var wc = new WebClient())
            {
                byte[] data = enc.GetBytes(postXML);

                byte[] result = wc.UploadData(url, data);

                if (result != null)
                {
                    resultXML = enc.GetString(result);
            
                }
            }

            if (string.IsNullOrEmpty(resultXML))
            {
                return null;
            }


            var xml = new XmlDocument();

            xml.LoadXml(resultXML);

            if (xml == null)
            {
                return null;
            }

            var orderResult = new OrderResult();

            string return_code = xml.SelectSingleNode("/xml/return_code").InnerText;

            if (return_code == "SUCCESS")
            {


                var dict = new Dictionary<string, string>();

                var nodes = xml.DocumentElement.ChildNodes;
                for (int i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    dict[node.Name] = node.InnerText;
                }


                if (dict["result_code"] == "SUCCESS")
                {
                    orderResult.Status = "SUCC";

                    var res = new Dictionary<string, string>
                    {
                        {"appId", config.AppID },
                        {"timeStamp", GetTimeStamp() },
                        {"nonceStr", dict["nonce_str"] },
                        {"package",  "prepay_id=" + dict["prepay_id"] },
                        {"signType", "MD5"}
                    };

                    res.Add("paySign", GetSignString(res, config.mch_secret));

                    //在服务器上签名                      
                    orderResult.Data = res;
                }
            }

            return orderResult;
        }



        bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受     
        }

        public static string GetSignString(Dictionary<string, string> dic, string key)
        {
            //string key = System.Web.Configuration.WebConfigurationManager.AppSettings["key"].ToString();//商户平台 API安全里面设置的KEY  32位长度  
            //排序  
            dic = dic.OrderBy(d => d.Key).ToDictionary(d => d.Key, d => d.Value);
            //连接字段  
            var sign = dic.Aggregate("", (current, d) => current + (d.Key + "=" + d.Value + "&"));
            sign += "key=" + key;

            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            sign = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(sign))).Replace("-", null);
            return sign;
        }



    }


}