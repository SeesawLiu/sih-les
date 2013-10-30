using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace com.Sconit.Entity.SAP
{
    public enum StatusEnum
    {
        [Description("执行中")]
        Pending = 0,
        [Description("成功")]
        Success = 1,
        [Description("失败")]
        Fail = 2,
        [Description("异常")]
        Exception = 3,
    }

    public interface ITraceable
    {
        StatusEnum Status { get; set; }
        DateTime CreateDate { get; set; }
        DateTime LastModifyDate { get; set; }
        Int32 ErrorCount { get; set; }
    }
}
