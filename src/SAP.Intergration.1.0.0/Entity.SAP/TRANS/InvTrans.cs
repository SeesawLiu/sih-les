using System;
using System.Collections.Generic;

//TODO: Add other using statements here

namespace com.Sconit.Entity.SAP.TRANS
{
    public partial class InvTrans
    {
        #region Non O/R Mapping Properties
        public Int64 LocTransId { get; set; }
        public string CheckList { get; set; }
        public string StatusName { get; set; }
        public IList<InvTrans> InvTransList { get; set; }

        public enum ErrorIdEnum
        {
            E000 = 0,     //没有错误。
            E100 = 100,   //系统出错。
            E101 = 101,   //库存事务{0}的PO单号/计划协议号为空。
            E102 = 102,   //库存事务{0}的PO单号行号/计划协议行号为空。
            E103 = 103,   //交货计划行收货时库存事务{0}的计划协议行号不包含分隔符{1}。
            E104 = 104,   //移库/移库退货结算数量小于0。
            E105 = 105,   //移库/移库退货寄售数量小于0。
            E106 = 106,   //移库/移库退货冲销结算数量小于0。
            E107 = 107,   //移库/移库退货冲销寄售数量小于0。
            E108 = 108,   //发生未知的结算。
            E199 = 199,   //生成移动类型时发生错误。
            E201 = 201,   //调用SAP移动类型接口返回结果不成功。
            E202 = 202    //调用SAP移动类型接口返回Exception信息。
        }
        #endregion
    }
}