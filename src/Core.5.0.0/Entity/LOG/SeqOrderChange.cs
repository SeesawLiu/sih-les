using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.LOG
{
    public partial class SeqOrderChange
    {
        [Export(ExportName = "ExportXLS", ExportSeq = 150)]
        public string TypeDescription
        {
            get {
                switch (this.Status)
                {
                    case 1:
                        return "新增";
                    case 2:
                        return "修改";
                    case 3:
                        return "删除";
                    case 4:
                        return "要货需求关闭";
                    default:
                        return "原数据";
                }
            }
        }
    }
}
