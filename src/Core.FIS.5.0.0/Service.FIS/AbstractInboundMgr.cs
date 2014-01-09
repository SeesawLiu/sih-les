using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.Utility;
using System.IO;
using com.Sconit.Entity;
using com.Sconit.Entity.FIS;
using com.Sconit.Entity.Exception;
using Castle.Services.Transaction;
using com.Sconit.Persistence;
using System.Collections;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SYS;
using NHibernate.Type;

namespace com.Sconit.Service.FIS
{
    public abstract class AbstractInboundMgr : IInboundMgr
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.Inbound");

        public IGenericMgr genericMgr { get; set; }

        public IOrderMgr orderMgr { get; set; }

        public IReceiptMgr receiptMgr { get; set; }

        public Service.IIpMgr ipMgr { get; set; }

        private Int32? _fISReCallTimes;
        public Int32? FISReCallTimes
        {
            get
            {
                if (!_fISReCallTimes.HasValue)
                {
                    _fISReCallTimes = int.Parse(this.genericMgr.FindById<com.Sconit.Entity.SYS.EntityPreference>
                        (com.Sconit.Entity.BusinessConstants.FIS_RECALLTIMES).Value
                    );
                }
                return _fISReCallTimes.Value;
            }
        }

        public virtual void ProcessInboundFile(InboundControl InboundControl, string[] files)
        {
            //log.Info("Start process inbound ");

            // IList<FormatControl> lenArray = this.genericMgr.FindAll<FormatControl>("from FormatControl f where f.SystemCode=? order by Sequence asc", InboundControl.SystemCode);

            foreach (var fileName in files)
            {
                try
                {
                    this.LESINbound(InboundControl, fileName);
                    this.genericMgr.FlushSession();
                    //log.Info(fileName + " successful.");
                    ArchiveFile(fileName, InboundControl.ArchiveFloder);
                }
                catch (Exception ex)
                {
                    this.genericMgr.CleanSession();
                    try
                    {
                        ArchiveFile(fileName, InboundControl.ErrorFloder);
                        //log.Info(" file success: " + fileName, ex);
                    }
                    catch (Exception)
                    {
                        log.Error(" file error: " + fileName + "存档失败。");
                    }
                }
            }
        }
        public abstract void PorcessData(string[] detail);

        public string[] ParseFields(string line, IList<FormatControl> lenArray)
        {
            if (string.IsNullOrEmpty(line)) return new string[0];
            ArrayList result = new ArrayList();
            int arrInde = 1;
            Byte[] bytes = Encoding.Unicode.GetBytes(line);

            foreach (var field in lenArray)
            {
                List<Byte> bytes2String = new List<Byte>();
                int byteCount = 0;
                for (int i = arrInde; i <= bytes.Length + 1; i = i + 2)
                {
                    if (byteCount > field.FieldLen)
                    {
                        throw new Exception("文档中的中文字符超长，无法截取");
                    }
                    if (byteCount == field.FieldLen)
                    {
                        result.Add(Encoding.Unicode.GetString(bytes2String.ToArray()));
                        arrInde = i;
                        break;
                    }
                    if (bytes[i] > 0)
                    {
                        bytes2String.Add(bytes[i - 1]);
                        bytes2String.Add(bytes[i]);
                        byteCount = byteCount + 2;
                    }
                    else
                    {
                        bytes2String.Add(bytes[i - 1]);
                        bytes2String.Add(bytes[i]);
                        byteCount = byteCount + 1;
                    }
                }
            }

            return (string[])result.ToArray(typeof(string));

        }

        public virtual FlatFileReader DataReader(string fileName, Encoding enc, string delimiter)
        {
            return new FlatFileReader(fileName, enc, delimiter); ;
        }

        public virtual void ArchiveFile(IList<string> fileFullPaths, string archiveFloder)
        {
            if (fileFullPaths != null && fileFullPaths.Count > 0)
            {
                foreach (var fileFullPath in fileFullPaths)
                {
                    this.ArchiveFile(fileFullPath, archiveFloder);
                }
            }
        }
        public virtual void ArchiveFile(string fileFullPath, string archiveFloder)
        {
            string fomatedFileFullPath = fileFullPath.Replace("\\", "/");
            string fileName = fomatedFileFullPath.Substring(fomatedFileFullPath.LastIndexOf("/") + 1);

            //log.Info("Archive file : " + archiveFloder + fileName);
            archiveFloder = archiveFloder.Replace("\\", "/");
            if (!archiveFloder.EndsWith("/"))
            {
                archiveFloder += "/";
            }

            if (!Directory.Exists(archiveFloder))
            {
                Directory.CreateDirectory(archiveFloder);
            }

            if (File.Exists(archiveFloder + fileName))
            {
                File.Delete(archiveFloder + fileName);
            }
            //log.Info("文件读完存档: " + archiveFloder + fileName);
            File.Move(fileFullPath, archiveFloder + fileName);
            string fileFullPathCTL = fileFullPath.ToUpper().Replace(".DAT", ".ctl");
            if (File.Exists(fileFullPathCTL))
            {
                File.Delete(fileFullPathCTL);
            }
        }

        private void LESINbound(InboundControl InboundControl, string fileName)
        {
            bool noError = true;
            try
            {
                string[] datStrs = System.IO.File.ReadAllLines(fileName);
                if (datStrs.Length == 0)
                {
                    throw new BusinessException("文件为空。");
                }
                fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                string[] datStr = new string[datStrs.Length - 1];
                for (int i = 0; i < datStrs.Length; i++)
                {
                    if (i > 0)
                    {
                        datStr[i - 1] = datStrs[i];
                    }
                }

                if (fileName.Substring(0, 4) == "MIGO")
                {
                    //log.Info("开始读文件" + fileName);
                    int rowCount = 0;
                    #region 供应商采购
                    foreach (var datLine in datStr) //循环每一行
                    {
                        rowCount++;
                        var line = datLine.Split(new char[] { '\t' }).ToArray();
                        LesINLog lesInLog = new LesINLog();
                        try
                        {
                            try
                            {
                                #region Log
                                IList<LesINLog> existsLesInLogList = this.genericMgr.FindAll<LesINLog>("select l from LesINLog as l where l.WMSNo=? and HandResult='S'", (string)line[20]);
                                if (existsLesInLogList != null && existsLesInLogList.Count > 0)
                                {
                                    #region LES中已经存在相同的WMS号 并且成功处理
                                    existsLesInLogList.First().IsCreateDat = false;
                                    this.genericMgr.Update(existsLesInLogList.First());
                                    continue;
                                    #endregion
                                }
                                lesInLog.Type = "MIGO";
                                lesInLog.MoveType = (string)line[0] + (line[10] == string.Empty ? string.Empty : line[10].ToUpper());
                                lesInLog.Sequense = "";
                                lesInLog.PO = (string)line[3];//
                                lesInLog.POLine = (string)line[4];//
                                lesInLog.WMSNo = (string)line[20];
                                lesInLog.WMSLine = (string)line[21];
                                lesInLog.Item = (string)line[11];
                                lesInLog.HandResult = "S";
                                lesInLog.FileName = fileName;
                                lesInLog.HandTime = System.DateTime.Now;// System.DateTime.Now.ToString("yyMMddHHmmss");
                                lesInLog.IsCreateDat = false;
                                lesInLog.ASNNo = line[17];
                                lesInLog.ExtNo = (string)line[3];
                                #endregion
                                if (line[11] == null || line[11] == string.Empty)
                                {
                                    throw new BusinessException("Item is Empty!");
                                }
                                if (line[17] == null || line[17] == string.Empty)
                                {
                                    throw new BusinessException("IpNo is Empty!");
                                }
                                if (line[0] == "102")
                                {
                                    #region 冲销
                                    IList<ReceiptDetail> receiptDetailList = this.genericMgr.FindAll<ReceiptDetail>("select r from ReceiptDetail as r where r.IpNo=? and r.Item=? and r.ExternalOrderNo=? and r.ExternalSequence=" + Convert.ToInt32(line[4]) + "", new object[] { line[17], line[11], line[3] });
                                    ReceiptMaster receiptMstr = this.genericMgr.FindById<ReceiptMaster>(receiptDetailList.First().ReceiptNo);
                                    //更新收货单的wms号为冲销记录的行号
                                    receiptMstr.WMSNo = (string)line[20];
                                    receiptMgr.CancelReceipt(receiptMstr, System.DateTime.Now);
                                    lesInLog.PO = receiptMstr.ReceiptNo;
                                    #endregion
                                }
                                else
                                {
                                    #region 正常收货
                                    IList<IpDetail> datIpdetailList = new List<IpDetail>();
                                    if (line[12] == null || line[12] == string.Empty)
                                    {
                                        throw new BusinessException("Qty is Empty!");
                                    }

                                    IList<IpDetail> ipDetails = this.genericMgr.FindAll<IpDetail>("select i from IpDetail as i where i.IpNo=? and i.Item=? and i.ExternalOrderNo=? and i.ExternalSequence=" + Convert.ToInt32(line[4]) + "", new object[] { line[17], line[11], line[3] });
                                    if (ipDetails == null || ipDetails.Count == 0)
                                    {
                                        throw new BusinessException("此ASN{0}物料{1},PO号{2},PO行号{3}找不到对应明细", line[17], line[11], line[3], line[4]);
                                    }
                                    decimal remianReceivedQty = Convert.ToDecimal(line[12].ToString());//数量
                                    //单位换算
                                    if (line[13].ToUpper() != ipDetails.First().Uom.ToUpper())
                                    {
                                        remianReceivedQty = remianReceivedQty / ipDetails.First().UnitQty;
                                    }
                                    ipDetails.First().BWART = line[0] + (line[10] == string.Empty ? string.Empty : line[10].ToUpper());//移动类型


                                    if (ipDetails.First().IpDetailInputs == null)
                                    {
                                        ipDetails.First().IpDetailInputs = new List<Entity.ORD.IpDetailInput>();
                                        ipDetails.First().AddIpDetailInput(new Entity.ORD.IpDetailInput());
                                        ipDetails.First().IpDetailInputs.Single().WMSRecNo = (string)line[20];
                                    }


                                    ipDetails.First().IpDetailInputs.Single().ReceiveQty += remianReceivedQty;
                                    datIpdetailList.Add(ipDetails.First());
                                    if (datIpdetailList != null && datIpdetailList.Count > 0)
                                    {
                                        ReceiptMaster recMaster = orderMgr.ReceiveIp(datIpdetailList);
                                        lesInLog.PO = recMaster.ReceiptNo;
                                    }
                                    #endregion
                                }
                            }
                            catch (BusinessException ex)
                            {
                                this.genericMgr.CleanSession();
                                lesInLog.HandResult = "F";
                                lesInLog.ErrorCause = ex.GetMessages()[0].GetMessageString();
                                noError = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            this.genericMgr.CleanSession();
                            lesInLog.HandResult = "F";
                            lesInLog.ErrorCause = ex.Message;
                            noError = false;
                        }
                        #region 更新或插入Log
                        IList<LesINLog> lesLogList = new List<LesINLog>();
                        lesLogList = this.genericMgr.FindAll<LesINLog>("select l from LesINLog as l where l.WMSNo=?", lesInLog.WMSNo);
                        if (lesLogList != null && lesLogList.Count > 0)
                        {
                            #region 如果已存在记录的状态为失败时根据当前执行情况更新此记录
                            if (lesLogList.First().HandResult == "F")
                            {
                                lesLogList.First().HandResult = lesInLog.HandResult;
                                lesLogList.First().IsCreateDat = false;
                                lesLogList.First().HandTime = lesInLog.HandTime;
                                lesLogList.First().FileName = lesInLog.FileName;
                                lesLogList.First().ErrorCause = lesInLog.ErrorCause;
                                lesLogList.First().Item = lesInLog.Item;
                                lesLogList.First().ExtNo = lesInLog.ExtNo;
                                lesLogList.First().PO = lesInLog.PO;
                                this.genericMgr.Update(lesLogList.First());
                            }
                            #endregion

                            #region 如果已存在记录为成功则不处理，但需要重发反馈信息给安吉，可以避免安吉不断重发
                            else
                            {
                                lesLogList.First().IsCreateDat = false;
                            }
                            #endregion
                        }
                        else
                        {
                            this.genericMgr.Create(lesInLog);
                        }
                        #endregion
                    }
                    // }
                    #endregion
                    if (rowCount < datStr.Length)
                    {
                        throw new BusinessException("文件读取行数不对，处理失败。");
                    }
                    //log.Info("文件" + fileName + "读完成功。");
                }
                else if (fileName.Substring(0, 4) == "MB1B")
                {
                    //log.Info("开始读文件" + fileName);
                    int rowCount = 0;
                    #region 安吉移库
                    var Query = from dat in datStr
                                let x = dat.Split(new char[] { '\t' }).ToArray()
                                select x;
                    foreach (var line in Query)
                    {
                        rowCount++;
                        WMSDatFile newMSDatFile = new WMSDatFile();
                        try
                        {
                            #region
                            IList<WMSDatFile> ExitxswmsDatFile = this.genericMgr.FindAll<WMSDatFile>("select w from WMSDatFile as w where w.WMSId=?", line[20]);
                            if (ExitxswmsDatFile != null && ExitxswmsDatFile.Count > 0)
                            {
                                #region 通过 WMSId 如果存在中间表中 过滤掉

                                continue;

                                //if (ExitxswmsDatFile.First().IsHand)
                                //{
                                //    #region 已经处理
                                //    IList<LesINLog> exitxLesLog = this.genericMgr.FindAll<LesINLog>("select l from LesINLog as l where l.WMSNo=?", ExitxswmsDatFile.First().WMSId);
                                //    if (exitxLesLog.First().HandResult == "S")
                                //    {
                                //        #region 已经成功处理的重新发送Log
                                //        if (exitxLesLog.First().IsCreateDat)//已经创建logDAT
                                //        {
                                //            exitxLesLog.First().IsCreateDat = false;
                                //            this.genericMgr.Update(exitxLesLog.First());
                                //            continue;
                                //        }
                                //        continue;
                                //        #endregion
                                //    }
                                //    else
                                //    {
                                //        #region 处理失败的更新中间表数据
                                //        newMSDatFile = this.GetWMSDatFile(line, fileName);
                                //        newMSDatFile.Id = ExitxswmsDatFile.First().Id;
                                //        newMSDatFile.IsHand = false;
                                //        this.genericMgr.Update(newMSDatFile);
                                //        continue;
                                //        #endregion
                                //    }
                                //    #endregion
                                //}
                                //else
                                //{
                                //    #region 没有处理过的 更新中间表的数据
                                //    newMSDatFile = this.GetWMSDatFile(line, fileName);
                                //    newMSDatFile.Id = ExitxswmsDatFile.First().Id;
                                //    this.genericMgr.Update(newMSDatFile);
                                //    continue;
                                //    #endregion
                                //}
                                #endregion
                            }
                            else
                            {
                                try
                                {
                                    #region 不存在，将数据插入到中间表
                                    newMSDatFile = this.GetWMSDatFile(line, fileName);
                                    this.genericMgr.Create(newMSDatFile);

                                    //如果是退货单明细，则直接收货
                                    int orderDetailId = 0;
                                    try
                                    {
                                        orderDetailId = Convert.ToInt32(newMSDatFile.WmsLine);
                                    }
                                    catch
                                    {
                                        continue;
                                    }
                                    OrderDetail orderDetail = this.genericMgr.FindById<OrderDetail>(orderDetailId);
                                    if (orderDetail != null)
                                    {
                                        OrderMaster orderMstr = this.genericMgr.FindById<OrderMaster>(orderDetail.OrderNo);
                                        if (orderMstr != null)
                                        {
                                            if (orderMstr.SubType == CodeMaster.OrderSubType.Return)
                                                orderMgr.ReceiveWMSIpMaster(newMSDatFile);
                                        }
                                    }
                                    continue;
                                    #endregion
                                }
                                catch (Exception e)
                                {
                                    throw new BusinessException(e.Message);
                                }
                            }
                            #endregion
                        }
                        catch (BusinessException ex)
                        {
                            #region Log
                            LesINLog lesInLog = new LesINLog();
                            lesInLog.Type = "MB1B";
                            lesInLog.MoveType = (string)line[0] + (line[10] == string.Empty ? string.Empty : line[10].ToUpper());
                            lesInLog.Sequense = "";
                            // lesInLog.PO = (string)line[3];//
                            //lesInLog.POLine = (string)line[4];//
                            lesInLog.WMSNo = (string)line[20];
                            lesInLog.WMSLine = (string)line[21];
                            lesInLog.Item = (string)line[11];
                            lesInLog.HandResult = "F";
                            lesInLog.FileName = fileName;
                            lesInLog.HandTime = System.DateTime.Now;
                            lesInLog.IsCreateDat = false;
                            lesInLog.ASNNo = line[17];
                            lesInLog.ErrorCause = ex.GetMessages()[0].GetMessageString();
                            this.genericMgr.Create(lesInLog);
                            noError = false;
                            continue;
                            #endregion
                        }
                    }

                    #endregion

                    if (rowCount < datStr.Length)
                    {
                        throw new BusinessException("文件读取行数不对，处理失败。");
                    }
                    //log.Info("文件" + fileName + "读完成功。");
                }
                else
                {
                    log.Info("文件" + fileName + "类型不对。");
                    throw new BusinessException("文件类型不对。");
                }
                if (!noError)
                {
                    throw new BusinessException("ERROR");
                }
            }
            catch (BusinessException ex)
            {
                throw new BusinessException(ex.GetMessages()[0].GetMessageString());
            }
        }

        private string GetMoveType(string moveType, IList<CodeDetail> codeDetailList)
        {
            // string type = "MinusMoveType";
            foreach (CodeDetail codeDetail in codeDetailList)
            {
                if (codeDetail.Value == moveType)
                {
                    return "PlusMoveType";
                }
            }
            return "MinusMoveType";
        }

        private WMSDatFile GetWMSDatFile(string[] line, string fileName)
        {
            #region 检查字段
            if (line[11] == null || line[11] == string.Empty)
            {
                throw new BusinessException("Item is Empty!");
            }
            if (line[12] == null || line[12] == string.Empty)
            {
                throw new BusinessException("Qty is Empty!");
            }
            if (line[17] == null || line[17] == string.Empty)
            {
                throw new BusinessException("IpNo is Empty!");
            }
            if (line[21] == null || line[21] == string.Empty)
            {
                throw new BusinessException("OrderId is Empty!");
            }
            if (line[20] == null || line[20] == string.Empty)
            {
                throw new BusinessException("WMSNo is Empty!");
            }
            #endregion

            #region 获取数据插中间表
            WMSDatFile wmsDatFile = new WMSDatFile();
            wmsDatFile.MoveType = line[0];
            wmsDatFile.BLDAT = line[1];
            wmsDatFile.BUDAT = line[2];
            wmsDatFile.PO = line[3];
            wmsDatFile.POLine = line[4];
            wmsDatFile.VBELN = line[5];
            wmsDatFile.POSNR = line[6];
            wmsDatFile.LIFNR = line[7];
            wmsDatFile.WERKS = line[8];
            wmsDatFile.LGORT = line[9];
            wmsDatFile.SOBKZ = line[10] == string.Empty ? string.Empty : line[10].ToUpper();
            wmsDatFile.Item = line[11];
            wmsDatFile.Qty = Convert.ToDecimal(line[12]);
            wmsDatFile.Uom = line[13];
            wmsDatFile.UMLGO = line[14];
            wmsDatFile.GRUND = line[15];
            wmsDatFile.KOSTL = line[16];
            wmsDatFile.WmsNo = line[17];
            wmsDatFile.RSNUM = line[18];
            wmsDatFile.RSPOS = line[19];
            wmsDatFile.WMSId = line[20];
            wmsDatFile.WmsLine = line[21];
            wmsDatFile.OLD = line[22];
            wmsDatFile.INSMK = line[23];
            wmsDatFile.XABLN = line[24];
            wmsDatFile.AUFNR = line[25];
            wmsDatFile.UMMAT = line[26];
            wmsDatFile.UMWRK = line[27];
            wmsDatFile.WBS = line[28];
            wmsDatFile.HuId = line[29];
            wmsDatFile.CreateDate = System.DateTime.Now;
            wmsDatFile.IsHand = false;
            wmsDatFile.FileName = fileName;

            #endregion

            return wmsDatFile;
        }
    }
}

