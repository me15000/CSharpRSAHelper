using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Common.Helpers
{
    /// <summary>
    /// ConvertHelper 的摘要说明
    /// </summary>
    public class ConvertHelper
    {
        public static int ToInt32(object o)
        {
            if (o != null && o != DBNull.Value)
            {
                return Convert.ToInt32(o);
            }
            return 0;
        }

        public static string ToString(object o)
        {
            if (o != null && o != DBNull.Value)
            {
                return Convert.ToString(o);
            }
            return null;
        }
    }

}