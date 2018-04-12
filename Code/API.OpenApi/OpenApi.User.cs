using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace API
{
    /// <summary>
    /// OpenApi 用户信息等
    /// </summary>
    public partial class OpenApi
    {
        
   
        
   



        /// <summary>
        /// 获得用户安全码
        /// [GET] /open/user/auth.json
        /// @code
        /// </summary>
        public void user_auth_json()
        {

            var rsp = new Common.DB.NVCollection();

            string code = Request.QueryString["code"] ?? string.Empty;
            if (string.IsNullOrEmpty(code))
            {
                EchoFailJson("code is null");
                return;
            }
            else
            {
                var nvc = Services.UserService.GetOpenIDByCode(code);
                if (nvc == null)
                {
                    EchoFailJson("GetOpenIDByCode is null");

                    return;
                }

                var obj = new
                {
                    openid = nvc["openid"] as string ?? string.Empty,
                    sessionKey = nvc["sessionKey"] as string ?? string.Empty,
                    unionid = nvc["unionid"] as string ?? string.Empty
                };
                int userid = 0;


                if (Services.UserService.SaveUserOpenId(obj.openid, out userid, InitUserGiveCoupon))
                {
                    string authcode = Services.UserService.GenUserAuthCode(obj.openid, userid);
                    rsp["code"] = 0;
                    rsp["status"] = "succ";
                    rsp["authcode"] = authcode;
                    Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
                    return;
                }
            }

        }

        /// <summary>
        /// 用户父关系建立
        /// [POST] /open/friend/pid.do	
        /// @authcode
        /// @authcodefriend
        /// </summary>
        public void friend_pid_do()
        {
            var postdata = ReadBodyData();

            string authcode = postdata.authcode ?? string.Empty;
            string authcodefriend = postdata.authcodefriend ?? string.Empty;

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

            if (userid == pid)
            {
                EchoFailJson("pid == userid");
                return;
            }

            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();


            dbh.ExecuteNoneQuery("update [user] set pid=@0 where id=@1", pid, userid);

            SetFirstCouponPid(userid, pid);

            var rsp = new Common.DB.NVCollection();
            rsp["code"] = 0;
            rsp["status"] = "succ";

            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
            return;
        }

        /// <summary>
        /// 更新用户基本信息
        /// [POST] /open/user/info.do
        /// @authcode
        /// @name
        /// @avatar
        /// @gender
        /// @age
        /// </summary>
        public void user_info_do()
        {
            var postdata = ReadBodyData();

            string authcode = postdata.authcode ?? string.Empty;
            int userid = 0;
            if (!TryGetUserId(authcode, out userid))
            {
                EchoFailJson("!TryGetUserId");
                return;
            }



            var rsp = new Common.DB.NVCollection();



            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();



            string name = postdata.name ?? string.Empty;
            string avatar = postdata.avatar ?? string.Empty;
            string gender = postdata.gender ?? string.Empty;
            string age = postdata.age ?? string.Empty;

            dbh.ExecuteNoneQuery("update [user] set name=@0,avatar=@1,gender=@2,age=@3 where id=@4", name, avatar, gender, age, userid);
            rsp["code"] = 0;
            rsp["status"] = "succ";

            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
            return;
        }


        /// <summary>
        /// 获得用户基本信息
        /// [GET] /open/user/info.json
        /// @authcode
        /// </summary>
        public void user_info_json()
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

            var user = dbh.GetData("select name,avatar,gender,age from [user] where id=@0", userid);
            if (userid <= 0)
            {
                EchoFailJson("user not exists");
                return;
            }
            else
            {
                rsp["code"] = 0;
                rsp["status"] = "succ";
                rsp["data"] = user;
                Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
                return;
            }

        }

        /// <summary>
        /// 更新Formid-用于后期推送消息给用户
        /// [POST] /open/user/formid.do
        /// @authcode
        /// @formid
        /// </summary>
        public void user_formid_do()
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


            string formid = postdata.formid ?? string.Empty;
            if (string.IsNullOrEmpty(formid))
            {
                EchoFailJson("formid is null or empty");
                return;
            }

            dbh.ExecuteNoneQuery("update [user] set formid=@0,formiddate=@1 where id=@2", formid, DateTime.Now, userid);

            var rsp = new Common.DB.NVCollection();
            rsp["code"] = 0;
            rsp["status"] = "succ";
            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
        }


    }

}