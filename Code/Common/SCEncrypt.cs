using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Common
{
    /// <summary>
    /// SCEncrypt 的摘要说明
    /// </summary>
    public class SCEncrypt
    {

        static string EncodeBase64(string str)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
        }

        static string DecodeBase64(string base64)
        {
            string decode = null;
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                decode = System.Text.Encoding.ASCII.GetString(bytes);
            }
            catch
            {
            }
            return decode;
        }

        private const string STR_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private static int Index_STR_CHARS(char c)
        {
            for (int i = 0; i < STR_CHARS.Length; i++)
            {
                if (STR_CHARS[i] == c)
                {
                    return i;
                }
            }
            return -1;
        }

        private static string R_Step1(string str)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                int n = Index_STR_CHARS(c);
                if (n >= 0)
                {
                    if (n == 0)
                    {
                        sb.Append(STR_CHARS[STR_CHARS.Length - 1]);
                    }
                    else
                    {
                        sb.Append(STR_CHARS[n - 1]);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static string R_Step2(string str)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = str.Length - 1; i >= 0; i--)
            {
                sb.Append(str[i]);
            }

            return sb.ToString();
        }

        private static string R_Step3(string str)
        {
            return DecodeBase64(str);
        }

        public static string Decode(string str)
        {
            string r_str = R_Step1(str);
            r_str = R_Step2(r_str);
            r_str = R_Step3(r_str);
            return r_str;
        }

        private static string Step1(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        private static string Step2(string str)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = str.Length - 1; i >= 0; i--)
            {
                sb.Append(str[i]);
            }

            return sb.ToString();
        }

        private static string Step3(string str)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                int n = Index_STR_CHARS(c);

                if (n >= 0)
                {
                    if (n == STR_CHARS.Length - 1)
                    {
                        sb.Append(STR_CHARS[0]);
                    }
                    else
                    {
                        sb.Append(STR_CHARS[n + 1]);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static string Encode(string str)
        {
            string r_str = Step1(str);
            r_str = Step2(r_str);
            r_str = Step3(r_str);
            return r_str;
        }

        public static string EncodeNVC(NameValueCollection nvc)
        {
            string code = string.Empty;
            bool first = true;
            foreach (string key in nvc.Keys)
            {
                if (!first)
                {
                    code += "&";
                }

                code += HttpUtility.UrlEncode(key) + "=" + HttpUtility.UrlEncode(nvc[key]);

                first = false;
            }


            return Encode(code);
        }

        public static NameValueCollection DecodeNVC(string sc)
        {
            if (!string.IsNullOrEmpty(sc))
            {
                string decode = Decode(sc);

                if (!string.IsNullOrEmpty(decode))
                {
                    string[] array = decode.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

                    NameValueCollection nvc = new NameValueCollection();
                    for (int i = 0; i < array.Length; i++)
                    {
                        string item = array[i];

                        int sinx = item.IndexOf('=');
                        if (sinx > 0)
                        {
                            string key = item.Substring(0, sinx);
                            if (sinx + 1 >= item.Length)
                            {
                                continue;
                            }

                            string value = item.Substring(sinx + 1);

                            nvc[HttpUtility.UrlDecode(key)] = HttpUtility.UrlDecode(value);
                        }
                    }

                    return nvc;
                }
            }

            return null;
        }
    }
}