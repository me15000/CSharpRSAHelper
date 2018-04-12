using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Services
{

    /// <summary>
    /// ModuleService 的摘要说明
    /// </summary>
    public class ModuleService
    {
        public static bool IsEnabled(string moduleKey)
        {
            var dbh = Common.CommonService.Resolve<Common.DB.IDBHelper>();

            
            return false;
        }

    }

}