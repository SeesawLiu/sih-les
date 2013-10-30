using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using com.Sconit.Service.SAP.MI_LES;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.SAP.TRANS;
using AutoMapper;
using com.Sconit.Entity.ORD;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Utility;
using com.Sconit.Entity.BIL;
using com.Sconit.Entity.CUST;
using com.Sconit.Entity.Exception;
using System.IO;

namespace com.Sconit.Service.SAP.Impl
{
    [Transactional]
    public class LocationLotDetailMgrImpl : BaseMgr, ILocationLotDetailMgr
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SAP_LocationLotDetail");

        #region 库存报表

        private static object ReportLocationLotDetailLock = new object();

        public void ReportLocationLotDetail(string ftpServer, int ftpPort, string ftpUser, string ftpPass,
                                            string ftpFolder, string localFolder, string localTempFolder)
        {
            IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();

            lock (ReportLocationLotDetailLock)
            {
                Process(ftpServer, ftpPort, ftpUser, ftpPass, ftpFolder,
                                localFolder, localTempFolder, errorMessageList);

                this.SendErrorMessage(errorMessageList);
            }
        }

        private void Process(string ftpServer, int ftpPort, string ftpUser, string ftpPass, string ftpFolder,
                               string localFolder, string localTempFolder, IList<ErrorMessage> errorMessageList)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append(
                    " select r.Plant,l.SAPLocation,det.Item,");
                sql.Append(
                    " CASE WHEN det.IsCS = 1 THEN 'K' ELSE '' END AS IsCS, ");
                sql.Append("det.CSSupplier, ");
                sql.Append("SUM(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty,");
                sql.Append("SUM(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty,");
                sql.Append("SUM(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, ");
                sql.Append("i.Uom ");
                sql.Append(" from VIEW_LocationLotDet det,MD_Location l,MD_Region r,MD_Item i ");
                sql.Append(" where det.Location = l.Code and l.Region = r.Code and det.Item=i.Code ");
                sql.Append(" group by r.Plant,l.SAPLocation,det.Item,i.Uom,det.CSSupplier,det.IsCS ");
                var locLotDetList = this.genericMgr.FindAllWithNativeSql<object[]>(sql.ToString());
                if (locLotDetList == null || locLotDetList.Count == 0)
                {
                    log.Warn("没有库存数据");
                    return;
                }
                string ymd = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                IWorkbook workbook = new HSSFWorkbook();

                ISheet sheet = workbook.CreateSheet(ymd);
                sheet.ForceFormulaRecalculation = true;

                ICellStyle headStyle = workbook.CreateCellStyle();
                IFont font = workbook.CreateFont();
                font.Boldweight = (short)FontBoldWeight.BOLD;
                font.FontHeightInPoints = (short)10;
                headStyle.SetFont(font);
                int rownum = 0;
                int colnum = 0;

                #region 列头

                XlsHelper.SetRowCell(sheet, rownum, colnum++, "工厂代码", headStyle);
                XlsHelper.SetRowCell(sheet, rownum, colnum++, "库存地点", headStyle);
                XlsHelper.SetRowCell(sheet, rownum, colnum++, "物料代码", headStyle);
                XlsHelper.SetRowCell(sheet, rownum, colnum++, "特殊标志", headStyle);
                XlsHelper.SetRowCell(sheet, rownum, colnum++, "供应厂商", headStyle);
                XlsHelper.SetRowCell(sheet, rownum, colnum++, "正常库存", headStyle);
                XlsHelper.SetRowCell(sheet, rownum, colnum++, "冻结库存", headStyle);
                XlsHelper.SetRowCell(sheet, rownum, colnum++, "质检库存", headStyle);
                XlsHelper.SetRowCell(sheet, rownum, colnum, "库存单位", headStyle);

                rownum++;

                #endregion

                #region 列表

                foreach (var locLotDet in locLotDetList)
                {
                    //工厂代码	库存地点	物料代码	特殊标志	供应厂商	正常库存	冻结库存	质检库存	库存单位
                    try
                    {
                        colnum = 0;
                        if (locLotDet[0] != null)
                            XlsHelper.SetRowCell(sheet, rownum, colnum, locLotDet[0].ToString());
                        colnum++;
                        if (locLotDet[1] != null)
                            XlsHelper.SetRowCell(sheet, rownum, colnum, locLotDet[1].ToString());
                        colnum++;
                        if (locLotDet[2] != null)
                            XlsHelper.SetRowCell(sheet, rownum, colnum, locLotDet[2].ToString());
                        colnum++;
                        if (locLotDet[3] != null)
                            XlsHelper.SetRowCell(sheet, rownum, colnum, locLotDet[3].ToString());
                        colnum++;
                        if (locLotDet[4] != null)
                            XlsHelper.SetRowCell(sheet, rownum, colnum, locLotDet[4].ToString());
                        colnum++;
                        if (locLotDet[5] != null)
                            XlsHelper.SetRowCell(sheet, rownum, colnum, decimal.Parse(locLotDet[5].ToString()).ToString("0.########"));
                        colnum++;
                        if (locLotDet[6] != null)
                            XlsHelper.SetRowCell(sheet, rownum, colnum, decimal.Parse(locLotDet[6].ToString()).ToString("0.########"));
                        colnum++;
                        if (locLotDet[7] != null)
                            XlsHelper.SetRowCell(sheet, rownum, colnum, decimal.Parse(locLotDet[7].ToString()).ToString("0.########"));
                        colnum++;
                        if (locLotDet[8] != null)
                            XlsHelper.SetRowCell(sheet, rownum, colnum, locLotDet[8].ToString());
                        rownum++;
                    }
                    catch (Exception ex)
                    {

                        log.Error(NVelocityTemplateRepository.TemplateEnum.GenerateInvRepFail, ex);
                        string errorMessage = locLotDet[0].ToString() + " " + locLotDet[1].ToString() + locLotDet[2].ToString() + " " +
                                              locLotDet[3].ToString() + " " + decimal.Parse(locLotDet[4].ToString()).ToString("0.########") + " " +
                                              decimal.Parse(locLotDet[5].ToString()).ToString("0.########") + " " +
                                              decimal.Parse(locLotDet[7].ToString()).ToString("0.########") + " " + locLotDet[7].ToString() +
                                              locLotDet[8].ToString() + " 生成库存报表失败。";
                        errorMessageList.Add(new ErrorMessage
                            {
                                Template = NVelocityTemplateRepository.TemplateEnum.GenerateInvRepFail,
                                Message = errorMessage,
                                Exception = ex
                            });
                    }
                }

                #endregion

                XlsHelper.WriteToFile(localFolder, "LESMB52" + ymd + ".xls", workbook);

                if (!Directory.Exists(localTempFolder))
                {
                    Directory.CreateDirectory(localTempFolder);
                }

                string localTempFolderFileName = localTempFolder + "/" + "LESMB52" + ymd + ".xls";
                string localFolderFileName = localFolder + "/" + "LESMB52" + ymd + ".xls";
                File.Copy(localFolderFileName, localTempFolderFileName, true); //备份目录

                log.Info("文件生成成功，记录数：" + locLotDetList.Count());

                #region 上传

                try
                {
                    FtpHelper ftp = new FtpHelper(ftpServer, ftpPort, ftpFolder, ftpUser, ftpPass);
                    ftp.Upload(localTempFolderFileName);
                    File.Delete(localTempFolderFileName);

                    log.Info(localTempFolderFileName + "上传成功");
                }
                catch (Exception ex)
                {
                    log.Error("Upload file:" + localTempFolderFileName, ex);

                    log.Error(NVelocityTemplateRepository.TemplateEnum.GenerateInvRepFail, ex);
                    string errorMessage = "生成库存报表上传失败。";
                    errorMessageList.Add(new ErrorMessage
                    {
                        Template = NVelocityTemplateRepository.TemplateEnum.GenerateInvRepFail,
                        Message = errorMessage,
                        Exception = ex
                    });
                }

                #endregion
            }
            catch (Exception ex)
            {
                log.Error(NVelocityTemplateRepository.TemplateEnum.GenerateInvRepFail, ex);
                string errorMessage = "生成库存报表失败。";
                errorMessageList.Add(new ErrorMessage
                    {
                        Template = NVelocityTemplateRepository.TemplateEnum.GenerateInvRepFail,
                        Message = errorMessage,
                        Exception = ex
                    });
            }
        }

        #endregion
    }
}
