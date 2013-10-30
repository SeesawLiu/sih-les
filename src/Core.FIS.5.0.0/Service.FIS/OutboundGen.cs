using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using com.Sconit.Entity;
using com.Sconit.Entity.FIS;
using com.Sconit.Utility;
using Castle.Windsor;
using System.Text;

namespace com.Sconit.Service.FIS
{
    public class OutboundGen : IOutboundGen
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.Outbound");

        public IGenericMgr genericMgr { get; set; }

        public void ExportData(string systemCode, IWindsorContainer container)
        {
            IList<OutboundControl> outboundControlList = this.genericMgr.FindAll<OutboundControl>("from OutboundControl where SystemCode=?", systemCode).OrderBy(o => o.Sequence).ToList();
            //dssOutboundControlMgr.GetDssOutboundControl();

            if (outboundControlList != null && outboundControlList.Count > 0)
            {
                foreach (OutboundControl outboundControl in outboundControlList)
                {
                    string serviceName = outboundControl.ServiceName;
                    string fileEncoding = outboundControl.FileEncoding;

                    //ConstructorInfo ct = Type.GetType(serviceName).GetConstructor(new Type[0]);
                    //IOutboundMgr processor = (IOutboundMgr)ct.Invoke(new Object[0]);
                    IOutboundMgr processor = container.Resolve<IOutboundMgr>(serviceName);
                    processor.ProcessOutbound(outboundControl);
                }
            }
        }

        public void UploadFile()
        {
            log.Info("Start upload file to ftp according to FtpControl table.");
            IList<FtpControl> ftpControlList = this.genericMgr.FindAll<FtpControl>("from FtpControl fc where IOType=? ", BusinessConstants.IO_TYPE_OUT);

            if (ftpControlList != null && ftpControlList.Count > 0)
            {
                foreach (FtpControl ftpControl in ftpControlList)
                {
                    string ftpServer = string.Empty;
                    int ftpPort = 21;
                    string ftpTempFolder = string.Empty;
                    string ftpFolder = string.Empty;
                    string ftpUser = string.Empty;
                    string ftpPass = string.Empty;
                    string filePattern = string.Empty;
                    string localFolder = string.Empty;
                    string localTempFolder = string.Empty;
                    try
                    {
                        #region 获取参数
                        ftpServer = ftpControl.FtpServer;
                        ftpPort = ftpControl.FtpPort.HasValue ? ftpControl.FtpPort.Value : 21;
                        ftpTempFolder = ftpControl.FtpTempFolder;
                        ftpFolder = ftpControl.FtpFolder;
                        ftpUser = ftpControl.FtpUser;
                        ftpPass = ftpControl.FtpPassword;
                        filePattern = ftpControl.FilePattern;
                        localFolder = ftpControl.LocalFolder;
                        localTempFolder = ftpControl.LocalTempFolder;
                        #endregion

                        #region 初始化远程目录
                        FtpHelper ftp = new FtpHelper(ftpServer, ftpPort, ftpFolder, ftpUser, ftpPass);
                        
                        //ftpTempFolder = ftpTempFolder.Replace("\\", "/");
                        //if (!ftpTempFolder.EndsWith("/"))
                        //{
                        //    ftpTempFolder += "/";
                        //}

                        //try
                        //{
                        //    //清空Temp目录
                        //    foreach (string fileName in ftp.GetFileList(filePattern))
                        //    {

                        //        ftp.Delete(fileName);

                        //    }
                        //}
                        //catch (Exception)
                        //{
                        //}
                        //if (!ftp.DirectoryExist(ftpTempFolder))
                        //{
                        //    ftp.MakeDir(ftpTempFolder);
                        //}

                        ftpFolder = ftpFolder.Replace("\\", "/");
                        if (!ftpFolder.EndsWith("/"))
                        {
                            ftpFolder += "/";
                        }
                        //if (!ftp.DirectoryExist(ftpFolder))
                        //{
                        //    ftp.MakeDir(ftpFolder);
                        //}
                        #endregion

                        #region 获取本地上传文件列表
                        string[] files = null;
                        if (filePattern != null)
                        {
                            files = Directory.GetFiles(localFolder, filePattern);
                        }
                        else
                        {
                            files = Directory.GetFiles(localFolder);
                        }
                        #endregion

                        #region 上传文件
                        if (files != null && files.Length > 0)
                        {
                            foreach (string fileFullPath in files)
                            {
                                string fomatedFileFullPath = fileFullPath.Replace("\\", "/");
                                string fileName = fomatedFileFullPath.Substring(fomatedFileFullPath.LastIndexOf("/") + 1);
                                try
                                {
                                    ftp.Upload(fomatedFileFullPath); //上传
                                    if (ftp.FileExist(fileName))//检查是否已经存在FTP
                                    {
                                        if (ftpControl.VaildFilePattern != "BACKUP")//上传成功 写入ctl文件 表示完成 
                                        {
                                            string CTLName = fomatedFileFullPath.Substring(0, fomatedFileFullPath.LastIndexOf("/") + 1) + fileName.Replace(".DAT", ".CTL");
                                            this.CreateCTL(CTLName);
                                            ftp.Upload(CTLName);
                                            File.Delete(CTLName);
                                        }
                                        //else
                                        //{
                                        //    localTempFolder = localTempFolder.Replace("\\", "/");
                                        //    File.Copy(fomatedFileFullPath, localTempFolder+"/" + fileName);  //备份目录
                                        //}
                                        log.Info("Delete file: " + fomatedFileFullPath);
                                        File.Delete(fomatedFileFullPath);
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }
                                   
                                }
                                catch (Exception ex)
                                {
                                    if (fileName.Substring(0, 5) == "ASNLE")
                                    {
                                        this.genericMgr.Update("update CreateIpDAT set IsCreateDat=?,FileName=? where FileName=?", new object[] { false, ex.Message.Length>50?ex.Message.Substring(0,49):ex.Message, fileName });
                                    }
                                    else if (fileName.Substring(0, 6) == "SEQ1LE")
                                    {
                                        this.genericMgr.Update("update CreateOrderDAT set IsCreateDat=?,FileName=? where FileName=?", new object[] { false, ex.Message.Length > 50 ? ex.Message.Substring(0, 49) : ex.Message, fileName });
                                    }
                                    else if (fileName.Substring(0, 4) == "SHIP")
                                    {
                                        this.genericMgr.Update("update CreateProcurementOrderDAT set IsCreateDat=?,FileName=? where FileName=?", new object[] { false, ex.Message.Length > 50 ? ex.Message.Substring(0, 49) : ex.Message, fileName });
                                    }
                                    else if (fileName.Substring(0, 5) == "LOGLE")
                                    {
                                        this.genericMgr.Update("update LesINLog set IsCreateDat=0 where HandTime>?", new object[] { System.DateTime.Now.AddHours(-1) });
                                    }
                                    else if (fileName.Substring(0, 4) == "CANC")
                                    {
                                        this.genericMgr.Update("update CancelReceiptMasterDAT set IsCreateDat =0, CreateDATDate=?,DATFileName=? where DATFileName=?", new object[] { System.DateTime.Now, ex.Message.Length > 50 ? ex.Message.Substring(0, 49) : ex.Message, fileName });
                                    }
                                    else if (fileName.Substring(0, 4) == "HUID")
                                    {
                                        this.genericMgr.Update("update CreateBarCode set DATFileName=?,CreateDATDate=?,IsCreateDat=0 where DATFileName=?", new object[] { ex.Message.Length > 50 ? ex.Message.Substring(0, 49) : ex.Message, System.DateTime.Now, fileName });
                                    }
                                    else if (fileName.Substring(0, 3) == "SEQ" && fileName.Substring(0, 6) != "SEQ1LE")
                                    {
                                        this.genericMgr.Update("update CreateSeqOrderDAT set IsCreateDat=?,FileName=? where FileName=? ", new object[] { false, ex.Message.Length > 50 ? ex.Message.Substring(0, 49) : ex.Message, fileName });
                                    }
                                    else if (fileName.Substring(0, 4) == "ITEM")
                                    {
                                        this.genericMgr.Update("update ItemStandardPackDAT set IsCreateDat=0, DATFileName=?,CreateDATDate=? where DATFileName=?", new object[] { ex.Message.Length > 50 ? ex.Message.Substring(0, 49) : ex.Message, System.DateTime.Now, fileName });
                                    }
                                    else if (fileName.Substring(0, 4) == "WARE")
                                    {
                                    }
                                    else if (fileName.Substring(0, 4) == "FLOW")
                                    {
                                    }
                                    else if (fileName.Substring(0, 4) == "RETU")
                                    {
                                        this.genericMgr.Update("update YieldReturn set IsCreateDat=0, DATFileName=?,CreateDATDate=? where DATFileName=?", new object[] { ex.Message.Length > 50 ? ex.Message.Substring(0, 49) : ex.Message, System.DateTime.Now, fileName });
                                    }
                                    log.Error("Upload file:" + fileFullPath, ex);
                                }
                            }
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        log.Error("Upload files from ftpServer:" + ftpServer, ex);
                    }
                }
            }
            else
            {
                log.Info("No record found in FtpControl table.");
            }

            log.Info("End upload file to ftp according to FtpControl table.");
        }

        private void CreateCTL(string Url)
        {
            StreamWriter sw = new StreamWriter(Url, false, Encoding.GetEncoding("UTF-8"));
            sw.WriteLine(string.Empty);
            sw.Flush();
            sw.Close();
        }
    }
}
