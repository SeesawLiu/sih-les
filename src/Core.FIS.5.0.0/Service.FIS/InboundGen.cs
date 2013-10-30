using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.Entity.FIS;
using com.Sconit.Entity;
using System.IO;
using com.Sconit.Utility;
using System.Reflection;
using Castle.Windsor;

namespace com.Sconit.Service.FIS
{
    public class InboundGen : IInboundGen
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.Inbound");

        public IGenericMgr genericMgr { get; set; }

        public void DownloadFile()
        {
            log.Info("Start download file from ftp according to FtpControl table.");
            IList<FtpControl> ftpControlList = this.genericMgr.FindAll<FtpControl>("from FtpControl fc where IOType=?", BusinessConstants.IO_TYPE_IN);
            //dssFtpControlMgr.GetDssFtpControl(BusinessConstants.IO_TYPE_IN);

            if (ftpControlList != null && ftpControlList.Count > 0)
            {
                foreach (FtpControl ftpControl in ftpControlList)
                {
                    string ftpServer = string.Empty;
                    int ftpPort = 21;
                    string[] ftpInboundFolders;
                    string ftpUser = string.Empty;
                    string ftpPass = string.Empty;
                    string filePattern = string.Empty;
                    string localTempFolder = string.Empty;
                    string localFolder = string.Empty;
                    string ftpBackupFolder = string.Empty;
                    string vaildFilePattern = string.Empty;
                    try
                    {
                        #region 获取ftp参数
                        ftpServer = ftpControl.FtpServer;
                        ftpPort = ftpControl.FtpPort.HasValue ? ftpControl.FtpPort.Value : 21;
                        ftpInboundFolders = ftpControl.FtpFolder.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        ftpBackupFolder = ftpControl.FtpTempFolder;
                        ftpUser = ftpControl.FtpUser;
                        ftpPass = ftpControl.FtpPassword;
                        filePattern = ftpControl.FilePattern;
                        vaildFilePattern = ftpControl.VaildFilePattern;
                        #endregion

                        #region 初始化本地目录
                        localTempFolder = ftpControl.LocalTempFolder;
                        localTempFolder = localTempFolder.Replace("\\", "/");
                        if (!localTempFolder.EndsWith("/"))
                        {
                            localTempFolder += "/";
                        }
                        if (!Directory.Exists(localTempFolder))
                        {
                            Directory.CreateDirectory(localTempFolder);
                        }

                        localFolder = ftpControl.LocalFolder;
                        localFolder = localFolder.Replace("\\", "/");
                        if (!localFolder.EndsWith("/"))
                        {
                            localFolder += "/";
                        }
                        if (!Directory.Exists(localFolder))
                        {
                            Directory.CreateDirectory(localFolder);
                        }
                        #endregion

                        IList<InboundControl> inboundCtrlList = this.genericMgr.FindAll<InboundControl>();

                        #region 下载文件
                        foreach (var ftpInboundFolder in ftpInboundFolders)
                        {
                            InboundControl inboundCtrl = this.genericMgr.FindAll<InboundControl>("from InboundControl ic where ic.FtpFolder=?", ftpInboundFolder).SingleOrDefault();
                            FtpHelper ftp = new FtpHelper(ftpServer, ftpPort, ftpInboundFolder, ftpUser, ftpPass);
                            foreach (string vaildFileName in ftp.GetFileList(vaildFilePattern))
                            {
                                string fileName = filePattern.Replace("*", vaildFileName.Substring(0, vaildFileName.IndexOf(".")));
                                string inFolder = inboundCtrl.InFloder;
                                inFolder = inFolder.Replace("\\", "/");
                                if (!inFolder.EndsWith("/"))
                                {
                                    inFolder += "/";
                                }
                                if (!Directory.Exists(inFolder))
                                {
                                    Directory.CreateDirectory(inFolder);
                                }
                                try
                                {
                                    ftp.Download(inFolder, vaildFileName);//CTL
                                    ftp.Download(inFolder, fileName);//DAT
                                    
                                    log.Info("Move file from folder: " + localTempFolder + vaildFileName + " to folder: " + localFolder + vaildFileName);
                                    File.Copy(inFolder + fileName, localTempFolder + fileName);
                                    //if (ftpBackupFolder != null && ftpBackupFolder.Length > 0)
                                    //{
                                    //    ftp.MovieFile(vaildFileName, ftpBackupFolder);
                                    //    ftp.MovieFile(fileName, ftpBackupFolder);
                                    //    //ftp.MovieFile(fileName, ftpControl.FtpBackUp);
                                    //    //ftp.MovieFile(fileName.Substring(0, fileName.IndexOf("."))+".CTL", ftpControl.FtpBackUp);
                                    //}
                                    //else
                                    //{
                                        ftp.Delete(vaildFileName);
                                        ftp.Delete(fileName);
                                    //}
                                    
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Download file:" + vaildFileName, ex);
                                    //ftp.MovieFile(fileName, ftpControl.FtpErrorFolder);
                                    //ftp.MovieFile(fileName.Substring(0, fileName.IndexOf(".")) + ".CTL", ftpControl.FtpErrorFolder);
                                }
                            }
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        log.Error("Download files from ftpServer:" + ftpServer, ex);
                    }
                }
            }
            else
            {
                log.Info("No record found in FtpControl table.");
            }

            log.Info("End download file from ftp according to FtpControl table.");
        }

        public void ImportData(IWindsorContainer container)
        {
            log.Info("Start import data file according to DssInboundControl table.");
            IList<InboundControl> inboundControlList = this.genericMgr.FindAll<InboundControl>().OrderBy(o => o.Sequence).ToList();
            //this.dssInboundControlMgr.GetDssInboundControl();

            if (inboundControlList != null && inboundControlList.Count > 0)
            {
                foreach (InboundControl inboundControl in inboundControlList)
                {
                    string inFloder = inboundControl.InFloder;
                    string filePattern = inboundControl.FilePattern;
                    string serviceName = inboundControl.ServiceName;
                    string archiveFloder = inboundControl.ArchiveFloder;
                    string errorFloder = inboundControl.ErrorFloder;
                    string fileEncoding = inboundControl.FileEncoding;

                    log.Info("Start import data, floder: " + inFloder + ", filePattern: " + filePattern + ", serviceName: " + serviceName);

                    string[] files = null;

                    if (Directory.Exists(inFloder))
                    {
                        if (filePattern != null)
                        {
                            files = Directory.GetFiles(inFloder, filePattern);
                        }
                        else
                        {
                            files = Directory.GetFiles(inFloder);
                        }
                    }

                    if (files != null && files.Length > 0)
                    {
                        try
                        {
                            IInboundMgr processor = container.Resolve<IInboundMgr>(serviceName);
                            processor.ProcessInboundFile(inboundControl, files);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Process inbound error: ", ex);
                        }
                    }
                    else
                    {
                        log.Info("No files found to process.");
                    }
                }
            }
            else
            {
                log.Info("No record found in DssInboundControl table.");
            }

            log.Info("End import data file according to DssInboundControl table.");
        }
    }
}
