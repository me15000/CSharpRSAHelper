using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class Config
{
    public static XCXConfig XCX = new XCXConfig()
    {
        AppID = "xx",	//小程序appid
        AppSecret = "xx",//小程序密钥
        mch_id = "xxx",//支付商户号
        mch_secret = "xxx"//支付商户秘钥

    };
}

public class XCXConfig
{
    public string AppID { get; set; }
    public string AppSecret { get; set; }
    public string mch_id { get; set; }
    public string mch_secret { get; set; }

}