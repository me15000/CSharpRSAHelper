using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace API
{
    /// <summary>
    /// OpenApi 系统配置等
    /// </summary>
    public partial class OpenApi
    {
        /// <summary>
        /// 获取模块配置
        /// [GET] /open/module/data.json
        /// @authcode
        /// </summary>
        public void module_data_json()
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


            var modules = dbh.GetDataList("select * from [sys.module]");

            var module = new Common.DB.NVCollection();

            foreach (var m in modules)
            {
                string key = m["key"].ToString();
                m.Remove("key");
                module[key] = m;
            }
            module["code"] = 0;
            module["status"] = "succ";
            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(module));
        }

        /// <summary>
        /// 获得门店基本信息
        /// [GET] /open/sys/shop/info.json
        /// @authcode
        /// </summary>
        public void sys_shop_info_json()
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

           

            var config = dbh.GetData("select top 1 name,px,py,address,contact,pics,content,logo,qrcode from [sys.config] where enabled=1");
            if (config == null)
            {
                rsp["code"] = -1;
                rsp["status"] = "fail";
                Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
                return;
            }
            config["pics"] = Convert.ToString(config["pics"]).Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            rsp["code"] = 0;
            rsp["status"] = "succ";
            rsp["data"] = config;
            Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(rsp));
            return;
        }
    }

}