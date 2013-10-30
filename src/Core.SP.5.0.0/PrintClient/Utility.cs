using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows.Forms;
using PrintClient.Properties;
using System.Management;
using System.Reflection;
using System.ComponentModel;

namespace PrintClient
{
    public class Utility
    {
        public static string GetBarcodeCode128BByStr(string str)
        {
            int total = 104;
            int a = 0;
            int endAsc = 0;
            char endChar = new char();
            for (int i = 0; i < str.Length; i++)
            {
                //转换ASCII数值
                a = Convert.ToInt32(Convert.ToChar(str.Substring(i, 1)));

                //Code 128 SET B 字符集
                if (a >= 32)
                {
                    total += (a - 32) * (i + 1);
                }
                else
                {
                    total += (a + 64) * (i + 1);
                }
            }
            endAsc = total % 103;
            //字符集大于95直接赋值，其它转换后获得
            if (endAsc >= 95)
            {
                switch (endAsc)
                {
                    case 95:
                        endChar = Convert.ToChar("Ã");
                        break;
                    case 96:
                        endChar = Convert.ToChar("Ä");
                        break;
                    case 97:
                        endChar = Convert.ToChar("Å");
                        break;
                    case 98:
                        endChar = Convert.ToChar("Æ");
                        break;
                    case 99:
                        endChar = Convert.ToChar("Ç");
                        break;
                    case 100:
                        endChar = Convert.ToChar("È");
                        break;
                    case 101:
                        endChar = Convert.ToChar("É");
                        break;
                    case 102:
                        endChar = Convert.ToChar("Ê");
                        break;
                    default:
                        endChar = Convert.ToChar("");
                        break;
                }
            }
            else
            {
                endAsc += 32;
                endChar = Convert.ToChar(endAsc);
            }
            //生成Code 128B条码字符串
            string result = "Ì" + str + endChar.ToString() + "Î";
            return result;
        }

        public static string Md5(string originalPassword)
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(originalPassword);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            return s.ToString();
        }

        public static bool IsDecimal(string str)
        {
            try
            {
                if (str.Contains("+"))
                {
                    string[] chars = str.Split('+');
                    for (int i = 0; i < chars.Length; i++)
                    {
                        if (i == chars.Length - 1 && chars[i].ToString() == string.Empty)
                            return true;
                        Convert.ToDecimal(chars[i]);
                    }
                }
                else
                {
                    Convert.ToDecimal(str);

                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string FormatExMessage(string message)
        {
            try
            {
                if (message.StartsWith("System.Web.Services.Protocols.SoapException"))
                {
                    message = message.Remove(0, 44);
                    message = message.Remove(message.IndexOf("\n"), message.Length - message.IndexOf("\n"));
                }
                message = message.Replace("\\n", "\n\n");
            }
            catch (Exception ex)
            {
                return message;
            }
            return message;
        }

        public static void PrintOrder(string fileUrl, string printerName, IWin32Window win32Window)
        {
            KillProcess("EXCEL");
            Microsoft.Office.Interop.Excel.Application myExcel = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook myBook = null;
            Microsoft.Office.Interop.Excel.Worksheet mySheet1 = null;

            Object missing = System.Reflection.Missing.Value;
            Object defaultPrint = missing;

            string print = printerName;
            if (print != null && print != string.Empty)
            {
                defaultPrint = print;
            }

            try
            {
                myBook = myExcel.Workbooks.Open(fileUrl, missing, missing, missing, missing,
                    missing, missing, missing, missing, missing, missing, missing, missing, missing, missing);
                //handle sheets
                mySheet1 = (Microsoft.Office.Interop.Excel.Worksheet)myBook.Sheets[1];
                mySheet1.PrintOut(missing, missing, missing, missing, defaultPrint, missing, missing, missing);
            }
            catch (Exception e)
            {
                string errorMsg = "打印失败,重打请按CTRL+P!错误信息:" + e.Message;
                MessageBox.Show(win32Window, errorMsg, "打印失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (myBook != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(myBook);
                }
                if (mySheet1 != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mySheet1);
                }
                myBook = null;
                mySheet1 = null;
                myExcel.Quit();
                GC.Collect();
            }
        }

        private static void KillProcess(string processName)
        {
            //获得进程对象，以用来操作   
            System.Diagnostics.Process myproc = new System.Diagnostics.Process();
            //得到所有打开的进程    
            try
            {
                //获得需要杀死的进程名   
                foreach (Process thisproc in Process.GetProcessesByName(processName))
                {
                    //立即杀死进程   
                    thisproc.Kill();
                }
            }
            catch (Exception Exc)
            {
                //throw new Exception("", Exc);
            }
        }

        public static void Log(string logstr)
        {
            FileStream fs = new FileStream("C:\\SconitTemp\\Sconit_CSLog.txt", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter m_streamWriter = new StreamWriter(fs);
            m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
            m_streamWriter.WriteLine(logstr + " " + DateTime.Now.ToString() + "\n");
            m_streamWriter.Flush();
            m_streamWriter.Close();
            fs.Close();
        }

        public static string[] WMIGetMACString()
        {
            //获取网卡Mac地址   
            string mac = "";
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] == true)
                {
                    mac += mo["MacAddress"].ToString() + "|";
                }
            }
            moc = null;
            mc = null;
            return mac.Split('|');
        }

        public static SqlConnection DBConnection()
        {
            //OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + @"E:\CS4KPAC\yksjk.mdb");
            //return conn;
            //SqlConnection conn = new SqlConnection(@"Data Source=JIENYUAN\SQLEXPRESS;Initial Catalog=YKSJK;Persist Security Info=True;User ID=sa;Password=999999");
            SqlConnection conn = new SqlConnection(@"Data Source=10.20.70.250;Initial Catalog=YKSJK;Persist Security Info=True;User ID=sconittest;Password=sconittest");
            return conn;
        }

        public static string GetEnumDescription(System.Type value, string name)
        {
            FieldInfo fi = value.GetField(name);
            DescriptionAttribute[] attributes =
              (DescriptionAttribute[])fi.GetCustomAttributes(
              typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : name;
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes =(DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }

        public static string GetEnumValue(System.Type type,string desc)
        {
            string enumValue="";
            FieldInfo[] fis = type.GetFields();
            foreach (var fi in fis)
            {
                DescriptionAttribute[] attributes =(DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes.Length > 0 && attributes[0].Description == desc)
                {
                    enumValue = fi.Name;
                }
            }
            return enumValue;
            
        }
    }
}
