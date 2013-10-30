using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using com.Sconit.SmartDevice.Properties;
using com.Sconit.SmartDevice.SmartDeviceRef;
namespace com.Sconit.SmartDevice
{
    public class Utility
    {
        public static string GetBarCodeType(BarCodeType[] barCodeTypes, string barCode)
        {
            if (string.IsNullOrEmpty(barCode) || barCode.Length < 2)
            {
                return "-1";
            }
            else if (barCode.StartsWith("$"))
            {
                return barCode.Substring(1, 1);
            }
            else if (barCode.StartsWith(".."))
            {
                return CodeMaster.BarCodeType.DATE.ToString();
            }
            else if (barCode.StartsWith("W"))
            {
                return CodeMaster.BarCodeType.W.ToString();
            }
            else if (barCode.StartsWith("SP"))
            {
                return CodeMaster.BarCodeType.SP.ToString();
            }
            //else if (barCode.StartsWith("00"))
            //{
            //    return "00";
            //}
            else
            {
                foreach (var codeType in barCodeTypes)
                {
                    if (barCode.StartsWith(codeType.PreFixed))
                    {
                        return codeType.Type.ToString();
                    }
                }
                return CodeMaster.BarCodeType.HU.ToString();
            }
        }

        public static bool HasPermission(IpMaster ipMaster, User user)
        {
            return HasPermission(user.Permissions, ipMaster.OrderType, ipMaster.IsCheckPartyFromAuthority, ipMaster.IsCheckPartyToAuthority, ipMaster.PartyFrom, ipMaster.PartyTo);
        }

        public static bool HasPermission(OrderMaster orderMaster, User user)
        {
            return HasPermission(user.Permissions, orderMaster.Type, orderMaster.IsCheckPartyFromAuthority, orderMaster.IsCheckPartyToAuthority, orderMaster.PartyFrom, orderMaster.PartyTo); ;
        }

        public static bool HasPermission(FlowMaster flowMaster, User user)
        {
            return HasPermission(user.Permissions, flowMaster.Type, flowMaster.IsCheckPartyFromAuthority, flowMaster.IsCheckPartyToAuthority, flowMaster.PartyFrom, flowMaster.PartyTo);
        }

        public static bool HasPermission(PickListMaster pickListMaster, User user)
        {
            return HasPermission(user.Permissions, pickListMaster.OrderType, pickListMaster.IsCheckPartyFromAuthority, pickListMaster.IsCheckPartyToAuthority, pickListMaster.PartyFrom, pickListMaster.PartyTo);
        }

        public static bool HasPermission(Permission[] permissions, OrderType? orderType, bool isCheckPartyFromAuthority, bool isCheckPartyToAuthority, string partyFrom, string partyTo)
        {
            return true;
            bool hasPartyFromPermission = true;
            bool hasPartyToPermission = true;
            if (orderType == null || orderType == OrderType.Transfer || orderType == OrderType.Production)
            {
                if (orderType == null && partyFrom == null)
                {
                    hasPartyToPermission = true;
                }
                else
                {
                    if (isCheckPartyFromAuthority)
                    {
                        hasPartyFromPermission = permissions.Where(t => t.PermissionCategoryType == PermissionCategoryType.Region).Select(t => t.PermissionCode).Contains(partyFrom);
                    }
                    if (isCheckPartyToAuthority)
                    {
                        hasPartyToPermission = permissions.Where(t => t.PermissionCategoryType == PermissionCategoryType.Region).Select(t => t.PermissionCode).Contains(partyTo);
                    }
                }
            }
            else if (orderType == OrderType.Procurement || orderType == OrderType.CustomerGoods || orderType == OrderType.SubContract)
            {
                if (isCheckPartyFromAuthority)
                {
                    hasPartyFromPermission = permissions.Where(t => t.PermissionCategoryType == PermissionCategoryType.Supplier).Select(t => t.PermissionCode).Contains(partyFrom);
                }
                if (isCheckPartyToAuthority)
                {
                    hasPartyToPermission = permissions.Where(t => t.PermissionCategoryType == PermissionCategoryType.Region).Select(t => t.PermissionCode).Contains(partyTo);
                }
            }
            else if (orderType == OrderType.Distribution)
            {
                if (isCheckPartyFromAuthority)
                {
                    hasPartyFromPermission = permissions.Where(t => t.PermissionCategoryType == PermissionCategoryType.Region).Select(t => t.PermissionCode).Contains(partyFrom);
                }
                if (isCheckPartyToAuthority)
                {
                    hasPartyToPermission = permissions.Where(t => t.PermissionCategoryType == PermissionCategoryType.Customer).Select(t => t.PermissionCode).Contains(partyTo);
                }
            }
            return hasPartyFromPermission && hasPartyToPermission;
        }


        #region
        public static void ShowMessageBox(string message)
        {
            message = FormatExMessage(message);
            Sound sound = new Sound(Resources.Error);
            sound.Play();
            DialogResult dr = MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2);
        }

        public static void ShowMessageBox(BusinessException ex)
        {
            string message = FormatExMessage(ex.Message);
            if (ex.MessageParams != null)
            {
                message = string.Format(message, ex.MessageParams);
            }
            ShowMessageBox(message);
        }

        public static void ShowMessageBox(Exception ex)
        {
            string message = FormatExMessage(ex.Message);
            ShowMessageBox(message);
        }

        public static void ShowMessageBox(SoapException ex)
        {
            string message = FormatExMessage(ex.Message);
            ShowMessageBox(message);
        }

        public static string FormatExMessage(string message)
        {
            try
            {
                if (message.StartsWith("System.Web.Services.Protocols.SoapException"))
                {
                    message = message.Remove(0, 44);
                    int index = message.IndexOf("\n");
                    if (index > 0)
                    {
                        message = message.Remove(index, message.Length - index);
                    }
                    index = message.IndexOf("\r\n");
                    if (index > 0)
                    {
                        message = message.Remove(index, message.Length - index);
                    }
                }
                message = message.Replace("\\n", "\r\n");
                return message;
            }
            catch (Exception ex)
            {
                return message;
            }
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

        public static void ValidatorDecimal(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == "\b" || e.KeyChar.ToString() == "." || e.KeyChar.ToString() == "-")
            {
                string str = string.Empty;
                if (e.KeyChar.ToString() == "\b")
                {
                    e.Handled = false;
                    return;
                }
                else
                {
                    str = ((TextBox)sender).Text.Trim() + e.KeyChar.ToString();
                }

                if (Utility.IsDecimal(str) || str == "-")
                {
                    e.Handled = false;
                    return;
                }
            }

            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        public static bool IsDecimal(string str)
        {
            try
            {
                Convert.ToDecimal(str);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region LotNo
        public static string GenerateLotNo()
        {
            return GenerateLotNo(DateTime.Now);
        }

        public static string GenerateLotNo(DateTime dateTime)
        {
            /*
              	年份	代码	年份	代码	年份	代码
	            2004	4	    2016	G	    2028	W
	            2005	5	    2017	H	    2029	X
	            2006	6	    2018	J	    2030	Y
	            2007	7	    2019	K	    2031	1
	            2008	8	    2010	L	    2032	2
	            2009	9	    2021	M	    2033	3
	            2010	A	    2022	N	    2034	4
	            2011	B	    2023	P	    2035	5
	            2012	C	    2024	Q	    2036	6
	            2013	D	    2025	S	    2037	7
	            2014	E	    2026	T	    2038	8
	            2015	F	    2027	V	    2039	9
                超过2039年再从A开始
            */
            int year = dateTime.Year;
            int yearMod = (year - 2000) % 30;
            string yearStr = string.Empty;
            switch (yearMod)
            {
                case 1:
                    yearStr = "1";
                    break;
                case 2:
                    yearStr = "2";
                    break;
                case 3:
                    yearStr = "3";
                    break;
                case 4:
                    yearStr = "4";
                    break;
                case 5:
                    yearStr = "5";
                    break;
                case 6:
                    yearStr = "6";
                    break;
                case 7:
                    yearStr = "7";
                    break;
                case 8:
                    yearStr = "8";
                    break;
                case 9:
                    yearStr = "9";
                    break;
                case 10:
                    yearStr = "A";
                    break;
                case 11:
                    yearStr = "B";
                    break;
                case 12:
                    yearStr = "C";
                    break;
                case 13:
                    yearStr = "D";
                    break;
                case 14:
                    yearStr = "E";
                    break;
                case 15:
                    yearStr = "F";
                    break;
                case 16:
                    yearStr = "G";
                    break;
                case 17:
                    yearStr = "H";
                    break;
                case 18:
                    yearStr = "J";
                    break;
                case 19:
                    yearStr = "K";
                    break;
                case 20:
                    yearStr = "L";
                    break;
                case 21:
                    yearStr = "M";
                    break;
                case 22:
                    yearStr = "N";
                    break;
                case 23:
                    yearStr = "P";
                    break;
                case 24:
                    yearStr = "Q";
                    break;
                case 25:
                    yearStr = "S";
                    break;
                case 26:
                    yearStr = "T";
                    break;
                case 27:
                    yearStr = "V";
                    break;
                case 28:
                    yearStr = "W";
                    break;
                case 29:
                    yearStr = "X";
                    break;
                case 30:
                    yearStr = "Y";
                    break;
            }

            /*
             月份	代码
                1	1
                2	2
                3	3
                4	4
                5	5
                6	6
                7	7
                8	8
                9	9
                10	A
                11	B
                12	C
            */
            int month = dateTime.Month;
            string monthStr = String.Format("{0:X}", month);

            int day = dateTime.Day;
            string dayStr = day.ToString().PadLeft(2, '0');

            return yearStr + monthStr + dayStr;
        }

        public static bool IsValidateLotNo(string lotNo)
        {
            bool isValidateLotNo = false;
            try
            {
                isValidateLotNo = Utility.validateLotNo(lotNo);
            }
            catch (Exception)
            {
                isValidateLotNo = false;
            }
            return isValidateLotNo;
        }

        public static bool validateLotNo(string lotNo)
        {

            if (lotNo.Length != 4)
            {
                throw new BusinessException("批号长度不合法。");
            }

            Regex ry = new Regex("[A-Y1-9]");
            Regex my = new Regex("[A-C1-9]");

            string year = lotNo.Substring(0, 1);
            string month = lotNo.Substring(1, 1);
            string day = lotNo.Substring(2, 2);

            if (!ry.IsMatch(year))
            {
                throw new BusinessException("批号年份不合法。");
            }

            if (!my.IsMatch(month))
            {
                throw new BusinessException("批号月份不合法。");
            }

            try
            {
                int d = int.Parse(day);
                if (d > 32)
                {
                    throw new BusinessException("批号日期不合法。");
                }
            }
            catch (FormatException)
            {
                throw new BusinessException("批号日期不合法。");
            }

            return true;
        }

        public static DateTime ResolveLotNo(string lotNo)
        {
            validateLotNo(lotNo);

            char[] ch = lotNo.ToCharArray();
            char year = ch[0];
            char month = ch[1];
            string day = lotNo.Substring(2, 2);

            int yearDiff = 0;
            int yearInt = 0;

            switch (year)
            {
                case '1':
                    yearDiff = 1;
                    break;
                case '2':
                    yearDiff = 2;
                    break;
                case '3':
                    yearDiff = 3;
                    break;
                case '4':
                    yearDiff = 4;
                    break;
                case '5':
                    yearDiff = 5;
                    break;
                case '6':
                    yearDiff = 6;
                    break;
                case '7':
                    yearDiff = 7;
                    break;
                case '8':
                    yearDiff = 8;
                    break;
                case '9':
                    yearDiff = 9;
                    break;
                case 'A':
                    yearDiff = 10;
                    break;
                case 'B':
                    yearDiff = 11;
                    break;
                case 'C':
                    yearDiff = 12;
                    break;
                case 'D':
                    yearDiff = 13;
                    break;
                case 'E':
                    yearDiff = 14;
                    break;
                case 'F':
                    yearDiff = 15;
                    break;
                case 'G':
                    yearDiff = 16;
                    break;
                case 'H':
                    yearDiff = 17;
                    break;
                case 'J':
                    yearDiff = 18;
                    break;
                case 'K':
                    yearDiff = 19;
                    break;
                case 'L':
                    yearDiff = 20;
                    break;
                case 'M':
                    yearDiff = 21;
                    break;
                case 'N':
                    yearDiff = 22;
                    break;
                case 'P':
                    yearDiff = 23;
                    break;
                case 'Q':
                    yearDiff = 24;
                    break;
                case 'S':
                    yearDiff = 25;
                    break;
                case 'T':
                    yearDiff = 26;
                    break;
                case 'V':
                    yearDiff = 27;
                    break;
                case 'W':
                    yearDiff = 28;
                    break;
                case 'X':
                    yearDiff = 29;
                    break;
                case 'Y':
                    yearDiff = 30;
                    break;
                default:
                    throw new BusinessException("批号年份不合法。");
            }

            int nowYear = DateTime.Now.Year;
            int baseYear = ((nowYear - 2000) / 30) + 2000;

            if (baseYear + yearDiff > nowYear)
            {
                yearInt = baseYear - yearDiff;
            }
            else
            {
                yearInt = baseYear + yearDiff;
            }


            int monthInt = Int32.Parse(month.ToString(), System.Globalization.NumberStyles.HexNumber);
            int dayInt = Int32.Parse(day.TrimStart('0'));

            return new DateTime(yearInt, monthInt, dayInt);
        }
        #endregion
    }

    public class Sound
    {
        private byte[] m_soundBytes;

        private string m_fileName;

        private enum Flags
        {
            SND_SYNC = 0x0000,  /* play synchronously (default) */
            SND_ASYNC = 0x0001,  /* play asynchronously */
            SND_NODEFAULT = 0x0002,  /* silence (!default) if sound not found */
            SND_MEMORY = 0x0004,  /* pszSound points to a memory file */
            SND_LOOP = 0x0008,  /* loop the sound until next sndPlaySound */
            SND_NOSTOP = 0x0010,  /* don't stop any currently playing sound */
            SND_NOWAIT = 0x00002000, /* don't wait if the driver is busy */
            SND_ALIAS = 0x00010000, /* name is a registry alias */
            SND_ALIAS_ID = 0x00110000, /* alias is a predefined ID */
            SND_FILENAME = 0x00020000, /* name is file name */
            SND_RESOURCE = 0x00040004  /* name is resource name or atom */
        }

        [DllImport("CoreDll.DLL", EntryPoint = "PlaySound", SetLastError = true)]
        private extern static int WCE_PlaySound(string szSound, IntPtr hMod, int flags);

        [DllImport("CoreDll.DLL", EntryPoint = "PlaySound", SetLastError = true)]
        private extern static int WCE_PlaySoundBytes(byte[] szSound, IntPtr hMod, int flags);

        /// <summary>
        /// Construct the Sound object to play sound data from the specified file.
        /// </summary>
        public Sound(string fileName)
        {
            m_fileName = fileName;
        }

        /// <summary>
        /// Construct the Sound object to play sound data from the specified stream.
        /// </summary>
        public Sound(Stream stream)
        {
            // read the data from the stream
            m_soundBytes = new byte[stream.Length];
            stream.Read(m_soundBytes, 0, (int)stream.Length);
        }

        /// <summary>
        /// Construct the Sound object to play sound data from the specified byte.
        /// </summary>
        public Sound(byte[] m_soundBytes)
        {
            this.m_soundBytes = m_soundBytes;
        }

        /// <summary>
        /// Play the sound
        /// </summary>
        public void Play()
        {
            // if a file name has been registered, call WCE_PlaySound,
            //  otherwise call WCE_PlaySoundBytes
            try
            {
                if (m_fileName != null)
                {
                    WCE_PlaySound(m_fileName, IntPtr.Zero, (int)(Flags.SND_ASYNC | Flags.SND_FILENAME));
                }
                else
                {
                    WCE_PlaySoundBytes(m_soundBytes, IntPtr.Zero, (int)(Flags.SND_ASYNC | Flags.SND_MEMORY));
                }

            }
            catch (Exception)
            {
                //throw;
            }
        }


    }
}
