using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Constant 的摘要说明
/// </summary>
public class Constant
{
    public enum CouponType
    {
        Order = 1,
        Friend = 2,
        System = 3
    }
    public enum CouponStatus
    {
        Enabled = 1,
        Disabled = 2,
        Giving = 3,
    }
    public enum OrderStatus
    {
        Start = 1,
        Finish = 2,
        Fail = 3,
    }
}