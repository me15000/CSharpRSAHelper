

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Xml;

namespace Common.Helpers
{
    public class PayToBankHelper
    {


        public static bool PayToBank(int orderamt, string partner_trade_no, string banknumber, string name, string bankcode)
        {
            var config = Config.XCX;

            //try
            //{

            Dictionary<string, object> dic_params = new Dictionary<string, object>();
            dic_params["mch_id"] = config.mch_id;
            dic_params["partner_trade_no"] = partner_trade_no;
            dic_params["nonce_str"] = Guid.NewGuid().ToString("N");
            dic_params["enc_bank_no"] = Sign(banknumber);
            dic_params["enc_true_name"] = Sign(name);
            dic_params["bank_code"] = bankcode;
            dic_params["amount"] = orderamt;
            string signdata = "";
            foreach (var p in dic_params.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value))
            {
                signdata += p.Key + "=" + p.Value + "&";
            }
            signdata += "key=" + config.mch_secret;

            dic_params["sign"] = Common.Encrypt.MD5Encrypt(signdata).ToUpper();
            string resultXML = string.Empty;
            var response = CreatePostHttpResponseWithCert("https://api.mch.weixin.qq.com/mmpaysptrans/pay_bank", ToXml(dic_params), Encoding.UTF8);
            Stream respStream = response.GetResponseStream();
            StreamReader respStreamReader = new StreamReader(respStream, Encoding.UTF8);
            resultXML = respStreamReader.ReadToEnd();
            //System.Web.HttpContext.Current.Response.Write(resultXML);

            var xml = new XmlDocument();
            xml.LoadXml(resultXML);
            string return_code = xml.SelectSingleNode("/xml/return_code").InnerText;
            string result_code = xml.SelectSingleNode("/xml/return_code").InnerText;
            if (result_code == "SUCCESS" && return_code == "SUCCESS")
            {
                return true;
            }
            return false;
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //    return false;
            //}

        }


        public static string Sign(string data)
        {
            string pk = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0NZLS8G/wWUbce/OI/ZA
Xto4PQ3QYzXBdWCAWY6zqob4XExALu5KQwD/3F7M6LrFv3RhLtnKPBWe4zFlUTKm
N/53NH4RwtOgaArjRLXjsBx1YWHUq7UFNeo+n/57pLT984VWwG2GYOl7Yli5+X1y
oYP2OKFTLw9NXxtuRsDAhPGAQcvy9tAiqMZb5qhKjOQeFELtsoUt20IQv+wonhwJ
Az+u/cIm9K+bbfG/us/MGkmSt9zSfBmHWWbxeSb02tgiJXF2xCb6KRuR0ZM1Xk9c
/fa0AuT4lUzY/FnQQcead1J77d2H5qKBGQmk3kTdhWksHu59VWJQluJjivaJCDuS
xQIDAQAB
-----END PUBLIC KEY-----";

            string strPublic = pk
                                 .Split(new string[] { "-----" }, StringSplitOptions.RemoveEmptyEntries)[1]
                                 .Replace(" ", "").Replace("\r", "").Replace("\n", "");

            var rs = new RSACryptoHelper(null, strPublic);
            string result = rs.Encrypt(data);



            return result;


            //公钥加密
            //var rawData = "1234";


            //using (WebClient wc = new WebClient())
            //{



            //    var nvc = new System.Collections.Specialized.NameValueCollection();
            //    nvc["pubk"] = pk;
            //    nvc["cont"] = data;
            //    return Encoding.UTF8.GetString(wc.UploadValues("http://test.dongwujianbihua.com/rsa.php", nvc));
            //}

        }


        public static string GetPublicKey()
        {
            var config = Config.XCX;

            Dictionary<string, object> dic_params = new Dictionary<string, object>();
            dic_params["mch_id"] = config.mch_id;
            dic_params["nonce_str"] = Guid.NewGuid().ToString("N");
            dic_params["sign_type"] = "MD5";

            string signdata = "";
            foreach (var p in dic_params.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value))
            {
                signdata += p.Key + "=" + p.Value + "&";
            }
            signdata += "key=" + config.mch_secret;

            dic_params["sign"] = Common.Encrypt.MD5Encrypt(signdata).ToUpper();

            string resultXML = string.Empty;
            var response = CreatePostHttpResponseWithCert("https://fraud.mch.weixin.qq.com/risk/getpublickey", ToXml(dic_params), Encoding.UTF8);
            Stream respStream = response.GetResponseStream();
            StreamReader respStreamReader = new StreamReader(respStream, Encoding.UTF8);
            resultXML = respStreamReader.ReadToEnd();

            var xml = new XmlDocument();
            xml.LoadXml(resultXML);
            string publicKey = string.Empty;
            string return_code = xml.SelectSingleNode("/xml/return_code").InnerText;
            string result_code = xml.SelectSingleNode("/xml/return_code").InnerText;
            if (result_code == "SUCCESS" && return_code == "SUCCESS")
            {
                publicKey = xml.SelectSingleNode("/xml/pub_key").InnerText;
            }


            return publicKey;
        }


        static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受     
        }


        static HttpWebResponse CreatePostHttpResponseWithCert(string url, string datas, Encoding charset)
        {
            var config = Config.XCX;

            HttpWebRequest request = null;
            //HTTPSQ请求  
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            request = WebRequest.Create(url) as HttpWebRequest;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            //将上面的改成
            X509Certificate2 cert = new X509Certificate2(AppDomain.CurrentDomain.BaseDirectory + "cert\\apiclient_cert.p12", config.mch_id, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);//线上发布需要添加

            request.ClientCertificates.Add(cert);


            StringBuilder buffer = new StringBuilder();

            buffer.AppendFormat(datas);
            byte[] data = charset.GetBytes(buffer.ToString());
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            return request.GetResponse() as HttpWebResponse;
        }

        private static string ToXml(Dictionary<string, object> values)
        {
            if (0 == values.Count)
            {
                throw new Exception("数据为空!");
            }

            string xml = "<xml>";
            foreach (KeyValuePair<string, object> pair in values)
            {
                if (pair.Value == null)
                {
                    throw new Exception("内部含有值为null的字段!");
                }

                if (pair.Value.GetType() == typeof(int))
                {
                    xml += "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + ">";
                }
                else if (pair.Value.GetType() == typeof(string))
                {
                    xml += "<" + pair.Key + ">" + "<![CDATA[" + pair.Value + "]]></" + pair.Key + ">";
                }
                else if (pair.Value.GetType() == typeof(float))
                {
                    xml += "<" + pair.Key + ">" + "<![CDATA[" + pair.Value + "]]></" + pair.Key + ">";
                }
                else
                {
                    throw new Exception("字段数据类型错误!");
                }
            }
            xml += "</xml>";
            return xml;
        }


    }
}
