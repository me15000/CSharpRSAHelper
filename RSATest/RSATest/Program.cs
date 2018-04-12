using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSATest
{
    class Program
    {
        static void Main(string[] args)
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

            string data = "test1234";

            string result = rs.Encrypt(data);

            Console.WriteLine(result);

            Console.Read();
        }
    }
}
