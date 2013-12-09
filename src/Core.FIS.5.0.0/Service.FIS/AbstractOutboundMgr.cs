using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.Utility;
using System.IO;
using com.Sconit.Entity.FIS;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.Exception;
using com.Sconit.Persistence;
using System.Threading;
using System.Collections;

namespace com.Sconit.Service.FIS
{
    [Transactional]
    public abstract class AbstractOutboundMgr : IOutboundMgr
    {
        #region 变量
        protected static log4net.ILog log = log4net.LogManager.GetLogger("Log.Outbound");
        //protected IList<DssObjectMapping> _dssObjectMapping;

        public IGenericMgr genericMgr { get; set; }
        #endregion

        public void ProcessOutbound(OutboundControl outboundControl)
        {

            //log.Info("Start process outbound.");
           
            string outFolder = outboundControl.OutFolder;
            //string serviceName = dssOutboundControl.ServiceName;
            string archiveFolder = outboundControl.ArchiveFolder;
            string tempFolder = outboundControl.TempFolder;
            string encoding = outboundControl.FileEncoding;
            // string encoding = Encoding.Default.WebName;
            string filePrefix = this.GetFilePrefix(outboundControl);

            #region 初始化本地目录
            //outFolder = outFolder.Replace("\\", "/");
            //if (!outFolder.EndsWith("/"))
            //{
            //    outFolder += "/";
            //}

            //if (!Directory.Exists(outFolder))
            //{
            //    Directory.CreateDirectory(outFolder);
            //}

            //archiveFolder = archiveFolder.Replace("\\", "/");
            //if (!archiveFolder.EndsWith("/"))
            //{
            //    archiveFolder += "/";
            //}

            //if (!Directory.Exists(archiveFolder))
            //{
            //    Directory.CreateDirectory(archiveFolder);
            //}

            //tempFolder = tempFolder.Replace("\\", "/");
            //if (!tempFolder.EndsWith("/"))
            //{
            //    tempFolder += "/";
            //}

            //if (Directory.Exists(tempFolder))
            //{
            //    var d = new DirectoryInfo(tempFolder);
            //    //则删除此目录、其子目录以及所有文件
            //    d.Delete(true);
            //    //Directory.Delete(tempFolder);
            //}
            //Directory.CreateDirectory(tempFolder);
            #endregion

            #region 抽取数据
            IList<FormatControl> lenArray = null;
            ExtractOutboundData(outboundControl.SystemCode, outboundControl);
            // IList<FormatControl> lenArray = this.genericMgr.FindAll<FormatControl>("from FormatControl f where f.SystemCode=? order by Sequence asc", outboundControl.SystemCode);
            //Dictionary<string, string[][]> result = ExtractOutboundData(outboundControl.SystemCode,outboundControl);
            #endregion
            //foreach (var data in result)
            //{
            //    Random rd = new Random();
            //    Thread.Sleep(10);
            //    //string fileName = DateTime.Now.ToString("yyyyMMddHHmmssff") + filePrefix + rd.Next(0, 9).ToString() + ".dat";
            //    string fileName = data.Key;
            //    StreamWriter sw = new StreamWriter(tempFolder + fileName, false, Encoding.GetEncoding(encoding));
            //    int flushCount = 0;
            //    string key = data.Value.First()[0];
            //    foreach (var item in data.Value)
            //    {
            //        item[0] = string.Empty;
            //        sw.WriteLine(ParseLine(item));

            //        if (flushCount % 2000 == 0)
            //        {
            //            sw.Flush();
            //        }
            //        flushCount++;
            //    }
            //    sw.Flush();
            //    sw.Close();

            //    #region 文件移至目录
            //    try
            //    {
            //        File.Copy(tempFolder + fileName, archiveFolder + fileName);  //备份目录
            //        log.Debug(archiveFolder + fileName+" 备份成功。 ");
            //        File.Move(tempFolder + fileName, outFolder + fileName);     //导出目录
            //        log.Debug(archiveFolder + fileName + " 导出目录成功。 ");
            //        SaveOutboundData(key, true);
            //        SaveOutboundData(key, true, fileName);
            //    }
            //    catch (Exception ex)
            //    {
            //        log.Error("Create export file error.", ex);
            //        if (File.Exists(archiveFolder + fileName))
            //        {
            //            File.Delete(archiveFolder + fileName);
            //        }

            //        if (File.Exists(outFolder + fileName))
            //        {
            //            File.Delete(outFolder + fileName);
            //        }
            //        SaveOutboundData(key, false);
            //    }
            //    #endregion
            //}
        }

        private string ParseLine(string[] data)
        {
            StringBuilder line = new StringBuilder();
            foreach (var item in data)
            {
                line.Append(item);
            }
            return line.ToString();
        }

        private string ParseLine(string[] data, IList<FormatControl> lenArray)
        {
            string line = string.Empty;
            for (int i = 0; i < data.Length; ++i)
            {
                string item = data[i] != null ? data[i].ToString() : string.Empty;
                string field = string.Empty;
                int byteCouunt = Encoding.Default.GetByteCount(item);
                if (byteCouunt < lenArray[i].FieldLen)
                {
                    field = item.PadRight(lenArray[i].FieldLen - (byteCouunt - item.Length));
                }
                else if (byteCouunt > lenArray[i].FieldLen)
                {
                    field = ParseOutLenField(item, lenArray[i].FieldLen);
                }
                else
                {
                    field = item;
                }
                line = line + field;
                //if (i < data.Length - 1)
                //{
                //    line = (line + field).PadRight(lenArray[i + 1].StartPos);
                //}
                //else
                //{
                //    line = line + field;
                //}
            }
            return line;
        }


        public string ParseOutLenField(string item, int length)
        {
            List<Byte> bytes2String = new List<byte>();
            Byte[] bytes = Encoding.Unicode.GetBytes(item);
            int len = 0;
            for (int i = 1; i < bytes.Length; i = i + 2)
            {
                if (bytes[i] > 0)
                {
                    if (len + 2 <= length)
                    {
                        bytes2String.Add(bytes[i - 1]);
                        bytes2String.Add(bytes[i]);
                    }
                    else
                    {
                        break;
                    }
                    len = len + 2;
                }
                else
                {
                    if (len + 1 <= length)
                    {
                        bytes2String.Add(bytes[i - 1]);
                        bytes2String.Add(bytes[i]);
                    }
                    else
                    {
                        break;
                    }
                    len = len + 1;
                }
            }

            if (bytes2String.Count > 0)
            {
                return Encoding.Unicode.GetString(bytes2String.ToArray());
            }
            else
            {
                return string.Empty;
            }
        }
      
        //#region Abstract Method
        //protected abstract Dictionary<string, string[][]> ExtractOutboundData(string systemCode);//应该带有一个参数FisMapping
        protected abstract void ExtractOutboundData(string systemCode, OutboundControl outboundControl);

        protected virtual void SaveOutboundData(string systemCode, Boolean SuccessOrError)
        {
        }

        protected virtual void SaveOutboundData(string systemCode, Boolean SuccessOrError, string fileName)
        {
        }

        private string GetFilePrefix(OutboundControl outboundControl)
        {
            string filePrefix = outboundControl.SystemCode;
            return filePrefix;
        }

        protected void BaseOutboundData(Dictionary<string, string[][]> result, OutboundControl outboundControl)
        {
            string outFolder = outboundControl.OutFolder;
            string archiveFolder = outboundControl.ArchiveFolder;
            string tempFolder = outboundControl.TempFolder;
            string encoding = outboundControl.FileEncoding;
            string filePrefix = this.GetFilePrefix(outboundControl);

            #region 初始化本地目录
            outFolder = outFolder.Replace("\\", "/");
            if (!outFolder.EndsWith("/"))
            {
                outFolder += "/";
            }

            if (!Directory.Exists(outFolder))
            {
                Directory.CreateDirectory(outFolder);
            }

            archiveFolder = archiveFolder.Replace("\\", "/");
            if (!archiveFolder.EndsWith("/"))
            {
                archiveFolder += "/";
            }

            if (!Directory.Exists(archiveFolder))
            {
                Directory.CreateDirectory(archiveFolder);
            }

            tempFolder = tempFolder.Replace("\\", "/");
            if (!tempFolder.EndsWith("/"))
            {
                tempFolder += "/";
            }

            if (Directory.Exists(tempFolder))
            {
                var d = new DirectoryInfo(tempFolder);
                //则删除此目录、其子目录以及所有文件
                d.Delete(true);
                //Directory.Delete(tempFolder);
            }
            Directory.CreateDirectory(tempFolder);
            #endregion

            foreach (var data in result)
            {
                Random rd = new Random();
                Thread.Sleep(10);
                //string fileName = DateTime.Now.ToString("yyyyMMddHHmmssff") + filePrefix + rd.Next(0, 9).ToString() + ".dat";
                string fileName = data.Key;
                StreamWriter sw = new StreamWriter(tempFolder + fileName, false, Encoding.GetEncoding(encoding));
                int flushCount = 0;
                string key = data.Value.First()[0];
                foreach (var item in data.Value)
                {
                    item[0] = string.Empty;
                    sw.WriteLine(ParseLine(item));

                    if (flushCount % 2000 == 0)
                    {
                        sw.Flush();
                    }
                    flushCount++;
                }
                sw.Flush();
                sw.Close();

                #region 文件移至目录
                try
                {
                    File.Copy(tempFolder + fileName, archiveFolder + fileName);  //备份目录
                    //log.Debug(archiveFolder + fileName + " 备份成功。 ");
                    File.Move(tempFolder + fileName, outFolder + fileName);     //导出目录
                    //log.Debug(archiveFolder + fileName + " 导出目录成功。 ");
                    SaveOutboundData(key, true);
                    SaveOutboundData(key, true, fileName);
                }
                catch (Exception ex)
                {
                    log.Error("Create export file error.", ex);
                    if (File.Exists(archiveFolder + fileName))
                    {
                        File.Delete(archiveFolder + fileName);
                    }

                    if (File.Exists(outFolder + fileName))
                    {
                        File.Delete(outFolder + fileName);
                    }
                    SaveOutboundData(key, false);
                }
                #endregion
            }
        
        }

    }
}
