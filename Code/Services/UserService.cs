using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Services
{


    /// <summary>
    /// UserService 的摘要说明
    /// </summary>
    public class UserService
    {
        public UserService()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        public static void SetUserOld(int userid)
        {
            string sql = "update [user] set olduser=1 where id=@0;";

            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();

            dbh.ExecuteNoneQuery(sql, userid);


        }


        public static bool ExistsUserId(int userid)
        {
            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();

            var obj = dbh.ExecuteScalar<object>("select  id  from [user] where id=@0", userid);

            if (obj != null && obj != DBNull.Value)
            {
                return Convert.ToInt32(obj) > 0;
            }

            return false;
        }


        public static string GenUserAuthCode(string openid, int userid)
        {
            string str = "openid=" + HttpUtility.UrlEncode(openid) + "&userid=" + userid + "&t=" + DateTime.Now.Ticks;

            return Common.SCEncrypt.Encode(str);
        }



        public static dynamic DecodeUserAuthCode(string code)
        {
            var nvc = Common.SCEncrypt.DecodeNVC(code);

            if (nvc == null)
            {
                return null;
            }

            string openid = nvc["openid"];
            if (string.IsNullOrEmpty(openid))
            {
                return null;
            }

            int userid = int.Parse(nvc["userid"] ?? "0");

            if (userid <= 0)
            {
                return null;
            }

            dynamic obj = new System.Dynamic.ExpandoObject();


            obj.openid = openid;
            obj.userid = userid;


            return obj;


        }

        public static Common.DB.NVCollection GetOpenIDByCode(string code)
        {


            var config = Config.XCX;


            string appid = config.AppID;
            string appSecret = config.AppSecret;

            var client = new RestSharp.RestClient("https://api.weixin.qq.com");
            var request = new RestSharp.RestRequest("sns/jscode2session", RestSharp.Method.GET);
            request.AddParameter("appid", appid);
            request.AddParameter("secret", appSecret);
            request.AddParameter("js_code", code);
            request.AddParameter("grant_type", "authorization_code");


            var response = client.Execute(request);
            response.ContentEncoding = "utf-8";

            string content = response.Content;
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            dynamic data = JsonConvert.DeserializeObject<dynamic>(content);

            if (data == null)
            {
                return null;
            }

            if (data.errcode != null)
            {
                int errcode = data.errcode;
                if (errcode > 0)
                {
                    return null;
                }
            }

            string openid = data.openid ?? string.Empty;
            string session_key = data.session_key ?? string.Empty;
            string unionid = data.unionid ?? string.Empty;

            var nvc = new Common.DB.NVCollection();
            nvc["openid"] = openid;
            nvc["sessionKey"] = session_key;
            nvc["unionid"] = unionid;

            return nvc;
        }





        public static bool SaveUserOpenId(string openid, out int userid, Action<int> userinit = null)
        {
            userid = 0;

            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();

            var nvc = new Common.DB.NVCollection();
            nvc["openid"] = openid;
            nvc["date"] = DateTime.Now;

            object exo = dbh.ExecuteScalar<object>("select top 1 1 from [user] where openid=@0", openid);


            if (exo == null || exo == DBNull.Value)
            {
                userid = dbh.ExecuteScalar<int>("insert into [user](openid,date) values(@openid,@date);select @@IDENTITY", nvc);

                if (userinit != null)
                {
                    userinit(userid);
                }
            }
            else
            {
                userid = dbh.ExecuteScalar<int>("update [user] set date=@date where openid=@openid;select id from [user] where openid=@openid;", nvc);
            }


            if (userid > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public static Entities.UserInfo GetUserInfo(int userid)
        {
            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();
            var user = dbh.GetData("select top 1 [id],[openid],[name],[avatar],[gender],[age],[pid],[date],[formid],[formiddate],[olduser] from [user] where id=@0", userid);
            if (user != null)
            {
                return new Entities.UserInfo(user);
            }
            return null;
        }

    }

}