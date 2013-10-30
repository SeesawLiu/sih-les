using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using com.Sconit.Entity.ACC;

namespace com.Sconit.Utility
{
    public static class KanbanUtility
    {
        private static IDictionary<string, string> KanbanCalcCapDict = new Dictionary<string, string>();

        static KanbanUtility() {
            KanbanCalcCapDict.Add("BatchNo", "批处理号");
            KanbanCalcCapDict.Add("Qty", "数量");
            KanbanCalcCapDict.Add("Container", "包装");
            KanbanCalcCapDict.Add("LocBin", "库架位");
            KanbanCalcCapDict.Add("Shelf", "架位");
            KanbanCalcCapDict.Add("QiTiaoBian", "起跳便");
            KanbanCalcCapDict.Add("Ret", "结果代码");
            KanbanCalcCapDict.Add("Msg", "消息");
            KanbanCalcCapDict.Add("KanbanNum", "当前张数");
            KanbanCalcCapDict.Add("KanbanDeltaNum", "变化张数");
            KanbanCalcCapDict.Add("CardNo", "看板编号");
            KanbanCalcCapDict.Add("Seq", "序号");
            KanbanCalcCapDict.Add("Flow", "路线代码");
            KanbanCalcCapDict.Add("FlowDetId", "路线明细Id");
            KanbanCalcCapDict.Add("Region", "区域");
            KanbanCalcCapDict.Add("RegionName", "区域名");
            KanbanCalcCapDict.Add("LCCode", "物流中心");
            KanbanCalcCapDict.Add("Supplier", "供应商");
            KanbanCalcCapDict.Add("SupplierName", "供应商名");
            KanbanCalcCapDict.Add("Item", "物料编号");
            KanbanCalcCapDict.Add("ItemDesc", "零件描述");
            KanbanCalcCapDict.Add("PONo", "请购单号");
            KanbanCalcCapDict.Add("POLineNo", "请购单行号");
            KanbanCalcCapDict.Add("MultiSupplyGroup", "多轨组号");
            KanbanCalcCapDict.Add("EffDate", "投出日期");
            KanbanCalcCapDict.Add("FreezeDate", "冻结日期");
            KanbanCalcCapDict.Add("LastUseDate", "异动日期");
            //KanbanCalcCapDict.Add("", "");
        }

        public static string GetKanbanCalcChineseCap(string cap) {
            if (KanbanCalcCapDict.ContainsKey(cap))
            {
                return KanbanCalcCapDict[cap];
            }
            else {
                return null;
            }
        }

        public static DateTime ConvertDateTimeFromWorkDateAndShiftTime(DateTime workDate, string shiftTime, Boolean isEnd)
        {
            // 9:00
            string[] ss = shiftTime.Split(':');
            TimeSpan ts = new TimeSpan(Int32.Parse(ss[0]), Int32.Parse(ss[1]), 0);
            if (isEnd && ts.TotalSeconds == 0) {
                return workDate.AddDays(1);
            }
            return workDate.Add(ts);
        }

        //新写的计算看板时间的，涉及到跨天
        public static DateTime ConvertDateTimeFromWorkDateAndShiftTime(DateTime workDate, string shiftTime, string sysTime)
        {
            #region 小于企业选项的跨天的时间，都自动加一天,好像是没啥问题
            string[] ss = shiftTime.Split(':');
            TimeSpan ts = new TimeSpan(Int32.Parse(ss[0]), Int32.Parse(ss[1]), 0);

            string[] st = sysTime.Split(':');
            if( ts <new TimeSpan(Int32.Parse(st[0]), Int32.Parse(st[1]), 0))
            {
                workDate = workDate.AddDays(1);
            }

            return workDate.Add(ts);
            #endregion
        }

        public static int ExtractNFromQiTiaoBian(string qitiaobian)
        {
            //便
            string[] s = qitiaobian.Split('-');
            return int.Parse(s[2]);
        }

        public static int ExtractMFromQiTiaoBian(string qitiaobian)
        {
            //跳, 1-8-2
            string[] s = qitiaobian.Split('-');
            return int.Parse(s[1]);
        }

        public static string GetBatchNo(string multiregion, string region, string location, DateTime startDate, DateTime endDate, int kbCalc, User calcUser)
        {
            if (String.IsNullOrEmpty(multiregion))
            {
                return region + location + startDate.ToString("yyMMdd") + endDate.ToString("yyMMdd") + kbCalc;
            }
            else {
                return multiregion + startDate.ToString("yyMMdd") + endDate.ToString("yyMMdd") + kbCalc;
            }
        }

        public static string GetMD5(string s, int length)
        {
            MD5 md5 = MD5.Create();
            byte[] b = Encoding.UTF8.GetBytes(s);
            byte[] md5b = md5.ComputeHash(b);
            md5.Clear();
            StringBuilder sb = new StringBuilder();
            foreach (var item in md5b)
            {
                sb.Append(item.ToString("x2"));
            }
            return sb.ToString();
        }

        public static bool IsMinAndMaxDateTime(DateTime startTime, DateTime endTime)
        {
            //DateTime.Parse("1900-01-01 00:00:00"), DateTime.Parse("2999-1-1 00:00:00")
            return (startTime == DateTime.Parse("1900-01-01 00:00:00") &&
                      endTime == DateTime.Parse("2999-1-1 00:00:00"));
        }

        public static int GetSeqFromKanbanSeq(string kanbanSeq)
        {
            return Int32.Parse(kanbanSeq.Substring(1, 3));
        }
    }
}
