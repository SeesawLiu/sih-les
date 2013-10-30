using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace com.Sconit.Utility
{
    public static class SystemHelper
    {
        public static bool IsInterate(decimal dl)
        {
            return Math.Ceiling(dl) == dl && Math.Floor(dl) == dl;
        }

        public static bool IsInterate(double db)
        {
            return Math.Ceiling(db) == db && Math.Floor(db) == db;
        }

        static object lockGetSystemUniqueCode = new object();
        public static string GetSystemUniqueCode()
        {
            lock (lockGetSystemUniqueCode)
            {
                Thread.Sleep(100);
                return DateTime.Now.ToString("yyyyMMddHHmmssff");
            }
        }


        private static string[] cstr = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
        private static string[] wstr = { "", "", "拾", "佰", "仟", "萬", "拾", "佰", "仟", "億", "拾", "佰", "仟" };
        //数字必须在12位整数以内的字符串
        //调用方式如：Label1.Text=ConvertInt("数字字符串");

        public static string ConvertIntToZH(int tobeConvert)
        {
            string str = tobeConvert.ToString();
            int len = str.Length;
            int i;
            string tmpstr, rstr;
            rstr = "";
            for (i = 1; i <= len; i++)
            {
                tmpstr = str.Substring(len - i, 1);
                rstr = string.Concat(cstr[Int32.Parse(tmpstr)] + wstr[i], rstr);
            }
            rstr = rstr.Replace("拾零", "拾");
            rstr = rstr.Replace("零拾", "零");
            rstr = rstr.Replace("零佰", "零");
            rstr = rstr.Replace("零仟", "零");
            rstr = rstr.Replace("零萬", "萬");
            for (i = 1; i <= 6; i++)
                rstr = rstr.Replace("零零", "零");
            rstr = rstr.Replace("零萬", "零");
            rstr = rstr.Replace("零億", "億");
            rstr = rstr.Replace("零零", "零");
            return rstr;
        }
    }
}
