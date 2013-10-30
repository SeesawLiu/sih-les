using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Castle.Services.Transaction;
using com.Sconit.Entity.SAP.MD;
using com.Sconit.Service.SAP.MI_LFA1_LES;
using com.Sconit.Service.SAP.MI_MARC_OUT;
using com.Sconit.Utility;
using NHibernate;
using NHibernate.Type;

namespace com.Sconit.Service.SAP.Impl
{
    [Transactional]
    public class MasterDataMgrImpl : BaseMgr, IMasterDataMgr
    {
        private static string SAP_ITEM_BATCHNO = "SAPItemBatchNo";
        private static string SAP_SUPPLIER_BATCHNO = "SAPSupplierBatchNo";
        private static string SAP_QUOTA_BATCHNO = "SAPQuatoBatchNo";

        private static log4net.ILog log = log4net.LogManager.GetLogger("SAP_MasterData");

        #region 导入零件
        public void LoadSAPItems(string itemCode, string plantCode)
        {
            try
            {
                log.Debug("手工调用导入SAP物料。");
                MI_MARC_OUTService mmos = new MI_MARC_OUTService();
                mmos.Credentials = base.Credentials;
                mmos.Timeout = base.TimeOut;
                mmos.Url = ReplaceSAPServiceUrl(mmos.Url);

                ZTMATNR_IN zTMATNR_IN = new ZTMATNR_IN();
                zTMATNR_IN.MATNR = itemCode;
                zTMATNR_IN.WERKS = plantCode;
                zTMATNR_IN.LAEDA = DateTime.Now.AddDays(-30).ToString("yyyyMMdd");
                string smatnr = mmos.MI_MARC_OUT(zTMATNR_IN);
            }
            catch (Exception ex)
            {
                log.Error("手工调用导入SAP物料发生异常, 异常信息：" + ex.Message, ex);
                IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                errorMessageList.Add(new ErrorMessage
                {
                    Template = NVelocityTemplateRepository.TemplateEnum.ImportSapItemFail,
                    Message = "手工调用导入SAP物料发生异常, 异常信息：" + ex.Message,
                    Exception = ex
                });
                this.SendErrorMessage(errorMessageList);
            }
        }

        public void ImportSAPItem(IList<Entity.SAP.MD.Item> itemList)
        {
            try
            {
                log.Debug("接收SAP物料开始。");
                if (itemList != null && itemList.Count > 0)
                {
                    log.DebugFormat("接收SAP物料{0}条。", itemList.Count);
                    DateTime dateTimeNow = DateTime.Now;
                    int batchNo = int.Parse(numberControlMgr.GetNextSequence(SAP_ITEM_BATCHNO));
                    foreach (var item in itemList)
                    {
                        SAPItem sapItem = Mapper.Map<Item, SAPItem>(item);
                        sapItem.BatchNo = batchNo;
                        sapItem.CreateDate = dateTimeNow;
                        genericMgr.Create(sapItem);
                    }
                    genericMgr.FlushSession();

                    com.Sconit.Entity.ACC.User user = com.Sconit.Entity.SecurityContextHolder.Get();
                    genericMgr.UpdateWithNativeQuery("exec USP_IF_ProcessSAPItem ?,?,?",
                        new object[] { batchNo, user.Id, user.FullName },
                        new IType[] { NHibernateUtil.Int32, NHibernateUtil.Int32, NHibernateUtil.String });
                }
                else
                {
                    log.DebugFormat("接收SAP物料0条。");
                }

                log.Debug("接收SAP物料完成。");
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? (ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.InnerException.Message) : ex.Message;
                log.Error("接收SAP物料失败, 失败信息：" + errorMessage, ex);

                IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                errorMessageList.Add(new ErrorMessage
                {
                    Template = NVelocityTemplateRepository.TemplateEnum.ImportSapItemFail,
                    Message = "接收SAP物料失败，失败信息：" + errorMessage,
                    Exception = ex
                });
                this.SendErrorMessage(errorMessageList);
            }
        }
        #endregion

        #region 导入供应商
        public void LoadSAPSuppliers(string supplierCode)
        {
            try
            {
                log.Debug("导入SAP供应商开始。");
                MI_LFA1_LESService mmos = new MI_LFA1_LESService();
                mmos.Credentials = base.Credentials;
                mmos.Timeout = base.TimeOut;
                mmos.Url = ReplaceSAPServiceUrl(mmos.Url);

                DT_SAP dt_lesitem = new DT_SAP();
                dt_lesitem.INPUT = supplierCode;
                DT_LESITEM[] smatnr = mmos.MI_LFA1_LES(dt_lesitem);

                int batchNo = int.Parse(numberControlMgr.GetNextSequence(SAP_SUPPLIER_BATCHNO));
                DateTime dateTimeNow = DateTime.Now;

                IList<Entity.SAP.MD.SAPSupplier> supplierList = (from s in smatnr
                                                                 select new Entity.SAP.MD.SAPSupplier
                                                                 {
                                                                     Code = s.supplierCode,
                                                                     ShortCode = s.oldsupplierCode,
                                                                     Name = s.supplierName,
                                                                     CreateDate = dateTimeNow,
                                                                     BatchNo = batchNo
                                                                 }).ToList();

                foreach (var supplier in supplierList)
                {
                    this.genericMgr.Create(supplier);
                }
                this.genericMgr.FlushSession();

                com.Sconit.Entity.ACC.User user = com.Sconit.Entity.SecurityContextHolder.Get();
                this.genericMgr.UpdateWithNativeQuery("exec USP_IF_ProcessSAPSupplier ?,?,?",
                    new object[] { batchNo, user.Id, user.FullName },
                    new IType[] { NHibernateUtil.Int32, NHibernateUtil.Int32, NHibernateUtil.String });

                log.Debug("导入SAP供应商完成。");
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? (ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.InnerException.Message) : ex.Message;
                log.Error("导入SAP供应商失败, 失败信息：" + errorMessage, ex);

                IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                errorMessageList.Add(new ErrorMessage
                {
                    Template = NVelocityTemplateRepository.TemplateEnum.ImportSapSupplierFail,
                    Message = "导入SAP供应商失败，失败信息：" + errorMessage,
                    Exception = ex
                });
                this.SendErrorMessage(errorMessageList);
            }
        }
        #endregion

        #region 导入配额
        public void GetSAPQuota(string itemCode, string plantCode)
        {
            try
            {
                log.Debug("导入SAP配额开始。");
                com.Sconit.Service.SAP.MI_QUOTA_OUT.MI_QUOTA_OUTService mqos = new com.Sconit.Service.SAP.MI_QUOTA_OUT.MI_QUOTA_OUTService();
                mqos.Credentials = base.Credentials;
                mqos.Timeout = base.TimeOut;
                mqos.Url = ReplaceSAPServiceUrl(mqos.Url);

                com.Sconit.Service.SAP.MI_QUOTA_OUT.ZTMATNR_IN zTMATNR_IN = new com.Sconit.Service.SAP.MI_QUOTA_OUT.ZTMATNR_IN();
                zTMATNR_IN.MATNR = itemCode;
                zTMATNR_IN.WERKS = plantCode;
                com.Sconit.Service.SAP.MI_QUOTA_OUT.ZSEQUPK[] zSEQUPKArray = mqos.MI_QUOTA_OUT(zTMATNR_IN);

                if (zSEQUPKArray != null && zSEQUPKArray.Length > 0)
                {
                    log.DebugFormat("接收SAP配额{0}条。", zSEQUPKArray.Length);
                    DateTime dateTimeNow = DateTime.Now;
                    int batchNo = int.Parse(numberControlMgr.GetNextSequence(SAP_QUOTA_BATCHNO));
                    foreach (var zSEQUPK in zSEQUPKArray)
                    {
                        SAPQuota sapQuota = new SAPQuota();
                        sapQuota.QUNUM = zSEQUPK.QUNUM;
                        sapQuota.QUPOS = zSEQUPK.QUPOS;
                        sapQuota.LIFNR = zSEQUPK.LIFNR;
                        sapQuota.WERKS = zSEQUPK.WERKS;
                        sapQuota.BEWRK = zSEQUPK.BEWRK;
                        sapQuota.MATNR = zSEQUPK.MATNR;
                        sapQuota.VDATU = zSEQUPK.VDATU;
                        sapQuota.BDATU = zSEQUPK.BDATU;
                        sapQuota.BESKZ = zSEQUPK.BESKZ;
                        sapQuota.SOBES = zSEQUPK.SOBES;
                        sapQuota.QUOTE = zSEQUPK.QUOTE;
                        sapQuota.BatchNo = batchNo;
                        sapQuota.CreateDate = dateTimeNow;
                        genericMgr.Create(sapQuota);
                    }
                    genericMgr.FlushSession();

                    com.Sconit.Entity.ACC.User user = com.Sconit.Entity.SecurityContextHolder.Get();
                    IList<string> msgList = genericMgr.FindAllWithNativeSql<string>("exec USP_IF_ProcessSAPQuota ?,?,?",
                        new object[] { batchNo, user.Id, user.FullName },
                        new IType[] { NHibernateUtil.Int32, NHibernateUtil.Int32, NHibernateUtil.String });

                    if (msgList != null && msgList.Count > 0)
                    {
                        IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                        foreach (string msg in msgList)
                        {
                            errorMessageList.Add(new ErrorMessage
                            {
                                Template = NVelocityTemplateRepository.TemplateEnum.ImportSapQuotaFail,
                                Message = "导入SAP配额发生错误, 错误信息：" + msg,
                            });
                        }
                        this.SendErrorMessage(errorMessageList);
                    }
                }
                else
                {
                    log.DebugFormat("接收SAP配额0条。");
                }

                log.Debug("导入SAP配额完成。");
            }
            catch (Exception ex)
            {
                log.Error("导入SAP配额发生异常, 异常信息：" + ex.Message, ex);
                IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                errorMessageList.Add(new ErrorMessage
                {
                    Template = NVelocityTemplateRepository.TemplateEnum.ImportSapQuotaFail,
                    Message = "导入SAP配额发生异常, 异常信息：" + ex.Message,
                    Exception = ex
                });
                this.SendErrorMessage(errorMessageList);
            }
        }
        #endregion
    }
}

