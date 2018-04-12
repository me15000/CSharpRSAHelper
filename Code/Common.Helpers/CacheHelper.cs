using System;
using System.Collections.Generic;
using System.Web;

namespace Common.Helpers
{


    /// <summary>
    /// CacheHelper 的摘要说明
    /// </summary>
    public class CacheHelper
    {
        public delegate object CacheCallback();

        public static T GetCacheObject<T>(string key, int seconds, CacheCallback call, bool refresh = false)
        {
            var cache = HttpRuntime.Cache;

            object cacheObject = cache.Get(key);

            if (cacheObject == null || refresh)
            {
                cacheObject = call();

                if (cacheObject == null)
                {
                    return default(T);
                }

                cache.Insert(key, cacheObject, null, DateTime.Now.AddSeconds(seconds), TimeSpan.Zero);

            }

            if (cacheObject != null)
            {
                return (T)Convert.ChangeType(cacheObject, typeof(T));
            }
            else
            {
                return default(T);
            }

        }

    }

}