using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common.Helpers;
namespace Entities
{

    /// <summary>
    /// UserInfo 的摘要说明
    /// openid,pp,name,gender,date
    /// </summary>
    public class UserInfo
    {
        public int id { get; set; }
        public string openid { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }

        public string gender { get; set; }
        public string formid { get; set; }
        public int age { get; set; }
        public int pid { get; set; }
        public DateTime date { get; set; }
        public bool olduser { get; set; }

        public UserInfo(Common.DB.NVCollection nvc)
        {
            id = ConvertHelper.ToInt32(nvc["id"]);
            openid = ConvertHelper.ToString(nvc["openid"]);
            formid = ConvertHelper.ToString(nvc["formid"]);
            name = ConvertHelper.ToString(nvc["name"]);
            avatar = ConvertHelper.ToString(nvc["avatar"]);
            gender = ConvertHelper.ToString(nvc["gender"]);
            age = ConvertHelper.ToInt32(nvc["age"]);
            pid = ConvertHelper.ToInt32(nvc["pid"]);
            date = Convert.ToDateTime(nvc["date"]);
            olduser = Convert.ToBoolean(nvc["olduser"]);
        }
    }

}