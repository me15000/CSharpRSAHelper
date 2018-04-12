using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
namespace Common
{


    public static class CommonService
    {

        static IContainer __container;
        static CommonService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Common.DB.SQLServer.DBHelper>()
                .As<Common.DB.IDBHelper>()
                .WithParameter("connectionString", ConfigurationManager.ConnectionStrings["default"].ConnectionString);

            __container = builder.Build();
        }


        public static T Resolve<T>()
        {
            return __container.Resolve<T>();
        }

        

    }

}