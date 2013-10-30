using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.CUST;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.SAP;
using com.Sconit.Entity.SAP.ORD;
using com.Sconit.Service.SAP.MI_PO_CFR_LES;
using com.Sconit.Service.SAP.MI_PO_LES;
using com.Sconit.Service.SAP.MI_POCANCLE_LES;
using com.Sconit.Utility;
using NHibernate.Type;
using NHibernate;

namespace com.Sconit.Service.SAP.Impl
{
    public class ProductionMgrImpl : BaseMgr, IProductionMgr
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SAP_Productoin");

        public IOrderMgr orderMgr { get; set; }
        public IItemMgr itemMgr { get; set; }
        public IWorkingCalendarMgr workingCalendarMgr { get; set; }
        public IReportProdOrderOperationMgr reportProdOrderOperationMgr { get; set; }

        #region 获取整车生产单
        private static object AutoCreateVanOrderLock = new object();
        public void AutoCreateVanOrder(string prodLine)
        {
            lock (AutoCreateVanOrderLock)
            {
                string guid = Guid.NewGuid().ToString();
                try
                {
                    log.Info("-----------------------------------无敌的分割线----------------------------------------");
                    log.DebugFormat("GUID {1}，开始获取生产线{0}的整车生产单。", prodLine, guid);

                    //查找生产线Mapping表
                    ProductLineMap productLineMap = this.genericMgr.FindEntityWithNativeSql<ProductLineMap>("select * from CUST_ProductLineMap where SAPProdLine = ?", prodLine).SingleOrDefault();
                    if (productLineMap == null)
                    {
                        throw new BusinessException("GUID {1}，没有找到生产线{0}的映射关系。", prodLine, guid);
                    }
                    if (string.IsNullOrWhiteSpace(productLineMap.CabProdLine))
                    {
                        throw new BusinessException("GUID {1}，生产线{0}的映射关系没有维护驾驶室生产线。", prodLine, guid);
                    }
                    if (string.IsNullOrWhiteSpace(productLineMap.ChassisProdLine))
                    {
                        throw new BusinessException("GUID {1}，生产线{0}的映射关系没有维护底盘生产线。", prodLine, guid);
                    }
                    if (string.IsNullOrWhiteSpace(productLineMap.AssemblyProdLine))
                    {
                        throw new BusinessException("GUID {1}，生产线{0}的映射关系没有维护总装生产线。", prodLine, guid);
                    }
                    if (string.IsNullOrWhiteSpace(productLineMap.SpecialProdLine))
                    {
                        throw new BusinessException("GUID {1}，生产线{0}的映射关系没有维护特装生产线。", prodLine, guid);
                    }
                    if (string.IsNullOrWhiteSpace(productLineMap.CheckProdLine))
                    {
                        throw new BusinessException("GUID {1}，生产线{0}的映射关系没有维护检测生产线。", prodLine, guid);
                    }
                    if (string.IsNullOrWhiteSpace(productLineMap.Plant))
                    {
                        throw new BusinessException("GUID {1}，生产线{0}的映射关系没有维护工厂。", prodLine, guid);
                    }

                    #region 判断是否处于休息时间，休息时间不自动获取生产单
                    DateTime dateTimeFrom = DateTime.Now;
                    DateTime dateTimeTo = dateTimeFrom.AddMinutes(1);

                    IList<object[]> obj = this.genericMgr.FindAllWithNativeSql<object[]>("exec USP_Busi_GetWorkingCalendarView ?,?,?,?",
                        new object[] { productLineMap.ChassisProdLine, null, dateTimeFrom, dateTimeTo },
                         new IType[] { NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.DateTime, NHibernateUtil.DateTime });
                    #endregion

                    if (obj != null && obj.Count > 0)
                    {
                        //获取待上线底盘生产单数量
                        object submittedOrderCount = this.genericMgr.FindAllWithNativeSql<object>("select count(1) as counter from ORD_OrderMstr_4 WITH(NOLOCK) where Flow = ? and Status = ? and PauseStatus <> ?", new object[] { productLineMap.ChassisProdLine, CodeMaster.OrderStatus.Submit, CodeMaster.PauseStatus.Paused }).SingleOrDefault();

                        //底盘最大上线数大于当前上线数
                        if (submittedOrderCount != null && productLineMap.MaxOrderCount > int.Parse(submittedOrderCount.ToString()))
                        {
                            bool isNextOrder = true;
                            IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                            string sapOrderNo = this.genericMgr.FindAllWithNativeSql<string>("select top 1 mstr.ExtOrderNo from ORD_OrderMstr_4 as mstr WITH(NOLOCK) inner join ORD_OrderSeq as seq WITH(NOLOCK) on mstr.OrderNo = seq.OrderNo where mstr.Flow = ? order by seq.SapSeq desc",
                                productLineMap.ChassisProdLine).SingleOrDefault();

                            //没有找到生产单，取生产线映射上的初始生产订单号
                            if (sapOrderNo == null)
                            {
                                if (string.IsNullOrWhiteSpace(productLineMap.Plant))
                                {
                                    throw new BusinessException("GUID {1}，生产线{0}的映射关系没有维护初始生产订单号。", prodLine, guid);
                                }
                                sapOrderNo = productLineMap.InitialVanOrder;
                                isNextOrder = false;
                            }

                            int batchNo = 0;
                            if (isNextOrder)
                            {
                                //获取下一张生产单
                                batchNo = this.GetNextVanOrder(productLineMap.Plant, sapOrderNo, prodLine);
                            }
                            else
                            {
                                batchNo = this.GetNextVanOrder(productLineMap.Plant, sapOrderNo, prodLine);
                                //获取当前生产单
                                //this.GetCurrentVanOrder(productLineMap.Plant, sapOrderNo, prodLine);
                            }

                            IList<string> msgList = this.genericMgr.FindAllWithNativeSql<string>("select Msg from LOG_GenVanProdOrder where BatchNo = ?", batchNo);

                            if (msgList != null && msgList.Count > 0)
                            {
                                foreach (string msg in msgList)
                                {
                                    log.WarnFormat("获取生产线{0}的整车生产单发生问题：{1}。", prodLine, msg);
                                    errorMessageList.Add(new ErrorMessage
                                    {
                                        Template = NVelocityTemplateRepository.TemplateEnum.ImportVanPordOrderFail,
                                        Message = msg
                                    });
                                }
                                this.SendErrorMessage(errorMessageList);
                            }
                            log.DebugFormat("GUID {1}，获取生产线{0}的整车生产单成功。", prodLine, guid);
                        }
                        else
                        {
                            log.DebugFormat("GUID {3}，生产线{0}待上线整车生产单数量{1}大于等于最大上线数量{2}，不导入整车生产单。", prodLine, (submittedOrderCount != null ? int.Parse(submittedOrderCount.ToString()) : -1), productLineMap.MaxOrderCount, guid);
                        }
                    }
                    else
                    {
                        log.DebugFormat("GUID {1}，生产线{0}当前为休息时间，不导入整车生产单。", prodLine, guid);
                    }
                }
                catch (BusinessException ex)
                {
                    log.ErrorFormat("GUID {1}，自动获取生产线{0}的整车生产单失败。", prodLine, guid);
                    log.Error(ex);

                    IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                    foreach (Message msg in ex.GetMessages())
                    {
                        errorMessageList.Add(new ErrorMessage
                        {
                            Template = NVelocityTemplateRepository.TemplateEnum.ImportVanPordOrderFail,
                            Message = msg.GetMessageString()
                        });
                    }
                    this.SendErrorMessage(errorMessageList);
                }
                catch (Exception ex)
                {
                    string exMessage = ex.InnerException != null ? (ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.InnerException.Message) : ex.Message;
                    string errorMsg = string.Format("GUID {2}，自动获取生产线{0}的整车生产单出现异常，异常信息：{1}。", prodLine, exMessage, guid);
                    log.Error(errorMsg, ex);

                    IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                    errorMessageList.Add(new ErrorMessage
                    {
                        Template = NVelocityTemplateRepository.TemplateEnum.ImportVanPordOrderFail,
                        Message = errorMsg,
                        Exception = ex
                    });
                    this.SendErrorMessage(errorMessageList);
                }
            }
        }

        private static object GetVanOrderLock = new object();
        private int GetNextVanOrder(string plant, string sapOrderNo, string prodlLine)
        {
            lock (GetVanOrderLock)
            {
                int batchNo = 0;
                string AUFNR = null;
                try
                {
                    batchNo = this.GetVanOrder(plant, sapOrderNo, prodlLine, true);
                    AUFNR = this.genericMgr.FindAllWithNativeSql<string>("select AUFNR from SAP_ProdOrder where BatchNo = ?", batchNo).First();

                    log.DebugFormat("开始校验整车生产订单, 整车生产订单号{0}，BatchNo {1}", AUFNR, batchNo);
                    IList<string> errorMessageList = this.genericMgr.FindAllWithNativeSql<string>("exec USP_Busi_GenVanProdOrderVerify ?,?", new object[] { batchNo, false });
                    log.DebugFormat("结束校验整车生产订单, 整车生产订单号{0}，BatchNo {1}", AUFNR, batchNo);

                    if (errorMessageList != null && errorMessageList.Count > 0)
                    {
                        //log.ErrorFormat("生成整车生产单校验失败, 整车生产订单号{0}，BatchNo {1}，", AUFNR, batchNo);

                        BusinessException ex = new BusinessException();
                        foreach (string errorMessage in errorMessageList)
                        {
                            ex.AddMessage(errorMessage);
                        }

                        throw ex;
                    }
                    else
                    {
                        log.DebugFormat("开始生成整车生产订单, 整车生产订单号{0}，BatchNo {1}", AUFNR, batchNo);
                        User user = SecurityContextHolder.Get();
                        this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_GenVanProdOrder ?,?,?", new object[] { batchNo, user.Id, user.FullName });
                        log.DebugFormat("结束生成整车生产订单, 整车生产订单号{0}，BatchNo {1}", AUFNR, batchNo);
                    }

                    return batchNo;
                }
                catch (BusinessException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.InnerException != null ? (ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.InnerException.Message) : ex.Message;
                    throw new BusinessException("生成整车生产单{0}发生异常，BatchNo {1}，异常信息：{2}。", AUFNR, batchNo.ToString(), errorMessage);
                }
            }
        }

        public void GetCurrentVanOrder(string plant, string sapOrderNo, string prodlLine)
        {
            lock (GetVanOrderLock)
            {
                int batchNo = 0;
                string AUFNR = null;
                try
                {
                    batchNo = this.GetVanOrder(plant, sapOrderNo, prodlLine, false);
                    AUFNR = this.genericMgr.FindAllWithNativeSql<string>("select AUFNR from SAP_ProdOrder where BatchNo = ?", batchNo).First();

                    log.DebugFormat("开始生成整车生产订单, 整车生产订单号{0}，BatchNo {1}", AUFNR, batchNo);
                    IList<string> errorMessageList = this.genericMgr.FindAllWithNativeSql<string>("exec USP_Busi_GenVanProdOrderVerify ?,?", new object[] { batchNo, false });

                    if (errorMessageList != null && errorMessageList.Count > 0)
                    {
                        //log.ErrorFormat("生成整车生产单校验失败, 整车生产订单号{0}，BatchNo {1}，", AUFNR, batchNo);

                        BusinessException ex = new BusinessException();
                        foreach (string errorMessage in errorMessageList)
                        {
                            ex.AddMessage(errorMessage);
                        }

                        throw ex;
                    }
                    else
                    {
                        User user = SecurityContextHolder.Get();
                        this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_UpdateVanProdOrder ?,?,?", new object[] { batchNo, user.Id, user.FullName });
                        log.DebugFormat("结束生成整车生产订单, 整车生产订单号{0}，BatchNo {1}", AUFNR, batchNo);
                    }
                }
                catch (BusinessException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.InnerException != null ? (ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.InnerException.Message) : ex.Message;
                    throw new BusinessException("生成整车生产单{0}发生异常，BatchNo {1}，异常信息：{2}。", AUFNR, batchNo.ToString(), errorMessage);
                }
            }
        }

        public IList<string> UpdateVanOrder(string plant, string sapOrderNo, string prodlLine)
        {
            lock (GetVanOrderLock)
            {
                try
                {
                    int batchNo = GetVanOrder(plant, sapOrderNo, prodlLine, false);

                    log.DebugFormat("开始更新整车生产订单, 整车生产订单号{0}，BatchNo {1}", sapOrderNo, batchNo);
                    IList<string> errorMessageList = this.genericMgr.FindAllWithNativeSql<string>("exec USP_Busi_GenVanProdOrderVerify ?,?", new object[] { batchNo, true });

                    if (errorMessageList != null && errorMessageList.Count > 0)
                    {
                        log.ErrorFormat("更新整车生产订单校验失败, 整车生产订单号{0}，BatchNo {1}，", sapOrderNo, batchNo);

                        return errorMessageList;
                    }
                    else
                    {
                        User user = SecurityContextHolder.Get();
                        this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_UpdateVanProdOrder ?,?,?", new object[] { batchNo, user.Id, user.FullName });
                        log.DebugFormat("结束更新整车生产订单, 整车生产订单号{0}，BatchNo {1}", sapOrderNo, batchNo);

                        #region 驾驶室物料号发生变更
                        CodeMaster.CabType? cabType = null;

                        string cabItem = this.genericMgr.FindAllWithNativeSql<string>("select MATERIAL from SAP_ProdBomDet where BatchNo = ? and DISPO = 'IAF' and BESKZ = 'E' and ISNULL(SOBSL, '') = ''", batchNo).SingleOrDefault();
                        if (!string.IsNullOrWhiteSpace(cabItem))
                        {
                            cabType = CodeMaster.CabType.SelfMade;
                        }
                        else
                        {
                            cabItem = this.genericMgr.FindAllWithNativeSql<string>("select MATERIAL from SAP_ProdBomDet where BatchNo = ? and DISPO = 'L13'", batchNo).SingleOrDefault();
                            if (!string.IsNullOrWhiteSpace(cabItem))
                            {
                                cabType = CodeMaster.CabType.Purchase;
                            }
                        }

                        if (cabType.HasValue)
                        {
                            CabOut cabOut = this.genericMgr.FindEntityWithNativeSql<CabOut>("select * from CUST_CabOut where OrderNo in (select OrderNo from ORD_OrderMstr_4 WITH(NOLOCK) where ExtOrderNo = ? and ProdLineType = ?)", new object[] { sapOrderNo, CodeMaster.ProdLineType.Cab }).Single();

                            if (cabOut.CabItem != cabItem || cabOut.CabType != cabType)
                            {
                                log.InfoFormat("驾驶室生产单{0}的油漆后驾驶室物料号发生更新，原驾驶室物料号：{1}，变更后驾驶室物料号：{2}。", cabOut.OrderNo, cabOut.CabItem, cabItem);
                                cabOut.CabItem = cabItem;
                                cabOut.CabType = cabType.Value;

                                this.genericMgr.Update(cabOut);

                                //todo
                                //发送短消息  油漆后驾驶室物料号发生变化
                            }
                        }
                        #endregion
                    }
                }
                catch (BusinessException ex)
                {
                    log.Error("更新整车生产单失败。", ex);

                    IList<string> errorMessageList = new List<string>();
                    foreach (Message message in ex.GetMessages())
                    {
                        errorMessageList.Add(message.GetMessageString());
                    }
                    return errorMessageList;
                }
                catch (Exception ex)
                {
                    log.Error("更新整车生产单出现异常。", ex);

                    IList<string> errorMessageList = new List<string>();
                    if (ex.InnerException != null)
                    {
                        if (ex.InnerException.InnerException != null)
                        {
                            errorMessageList.Add(ex.InnerException.InnerException.Message);
                        }
                        else
                        {
                            errorMessageList.Add(ex.InnerException.Message);
                        }
                    }
                    else
                    {
                        errorMessageList.Add(ex.Message);
                    }

                    return errorMessageList;
                }

                return null;
            }
        }

        private int GetVanOrder(string plant, string sapOrderNo, string prodlLine, bool isNextOrder)
        {
            log.DebugFormat("开始获取整车生产订单，工厂{0}，生产订单号{1}，生产线{2}，重复获取标识{3}", plant, sapOrderNo, prodlLine, isNextOrder);
            ZHEAD[] orderHeadAry = null;
            ZITEM_LX[] orderOpAry = null;
            ZITEM_ZJ[] orderBomAry = null;
            try
            {
                MI_PO_LESService soService = new MI_PO_LESService();
                soService.Credentials = base.Credentials;
                soService.Timeout = base.TimeOut;
                soService.Url = ReplaceSAPServiceUrl(soService.Url);

                ZRANGE_AUFNR[] AUFNR = new ZRANGE_AUFNR[1];
                AUFNR[0] = new ZRANGE_AUFNR();
                AUFNR[0].SIGN = "I";
                AUFNR[0].OPTION = "EQ";
                AUFNR[0].LOW = sapOrderNo;

                ZRANGE_DAUAT[] DAUAT = new ZRANGE_DAUAT[1];
                DAUAT[0] = new ZRANGE_DAUAT();
                DAUAT[0].SIGN = "I";
                DAUAT[0].OPTION = "EQ";
                DAUAT[0].LOW = "Z901";
                ZRANGE_DISPO[] DISPO = new ZRANGE_DISPO[0];
                ZRANGE_GSTRS[] GSTRS = new ZRANGE_GSTRS[0];

                string returnMessage = null;
                log.DebugFormat("连接WebService获取整车订单，工厂代码{0}，生产订单号{1}，生产线{2}，重复获取标识{3}", plant, sapOrderNo, prodlLine, isNextOrder);
                orderHeadAry = soService.MI_PO_LES(AUFNR, DAUAT, DISPO, GSTRS, plant, isNextOrder ? "" : "X", prodlLine, out orderOpAry, out orderBomAry, out returnMessage);

                if (!string.IsNullOrWhiteSpace(returnMessage))
                {
                    throw new BusinessException("连接SAP获取整车生产单失败，失败信息：{0}。", returnMessage);
                }
            }
            catch (BusinessException ex)
            {
                log.ErrorFormat("连接SAP获取整车生产单失败，工厂代码{0}，生产订单号{1}，生产线{2}，重复获取标识{3}。", plant, sapOrderNo, prodlLine, isNextOrder);
                log.Error(ex);

                throw ex;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("连接SAP获取整车生产单出现异常，工厂代码{0}，生产订单号{1}，生产线{2}，重复获取标识{3}。", plant, sapOrderNo, prodlLine, isNextOrder);
                log.Error(ex);

                throw new BusinessException("连接SAP获取整车生产单出现异常，异常信息：{0}。", ex.Message);
            }

            if (orderHeadAry == null || orderHeadAry.Length == 0)
            {
                log.ErrorFormat("没有获取到整车生产单，工厂代码{0}，生产订单号{1}，生产线{2}，重复获取标识{3}。", plant, sapOrderNo, prodlLine, isNextOrder);

                throw new BusinessException("没有获取到整车生产单。");
            }
            else if (orderHeadAry.Length > 1)
            {
                //log.ErrorFormat("获取到多个SAP整车生产单，工厂代码{0}，生产订单号{1}，生产线{2}，获取下一台{3}。", plant, sapOrderNo, prodlLine, isNextOrder);

                ////todo
                ////发送短消息

                //throw new BusinessException("获取到多个SAP整车生产单。");
            }
            else
            {
                log.DebugFormat("成功获取整车生产订单{0}", orderHeadAry[0].AUFNR);
            }

            int batchNo = InsertTmpTable(orderHeadAry, orderOpAry, orderBomAry);
            this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_OffsetNegativeBom ?", batchNo);

            return batchNo;
        }

        private int InsertTmpTable(ZHEAD[] orderHeadAry, ZITEM_LX[] orderOpAry, ZITEM_ZJ[] orderBomAry)
        {
            try
            {
                int batchNo = int.Parse(numberControlMgr.GetNextSequence("SAPProdOrderBatchNo"));
                DateTime createDate = DateTime.Now;
                StringBuilder insertSql = new StringBuilder();
                int recordInsertBatchSize = 1000;
                int recordaccumulateCount = 0;
                log.DebugFormat("开始插入生产订单数据至中间表, 生产订单号{0}，BatchNo {1}", orderHeadAry[0].AUFNR, batchNo);

                #region 插入生产订单头信息
                if (orderHeadAry != null && orderHeadAry.Length > 0)
                {
                    PrepareInsertProdOrderSql(insertSql);
                    for (int i = 0; i < orderHeadAry.Length; i++)
                    {
                        ZHEAD head = orderHeadAry[i];
                        insertSql.Append("(");
                        insertSql.Append(batchNo.ToString() + ",");
                        //insertSql.Append("'" + createDate.ToString("yyyy/MM/dd HH:mm:ss") + "',");
                        insertSql.Append("'" + (head.AUFNR != null ? head.AUFNR : string.Empty) + "',");
                        insertSql.Append("'" + (head.WERKS != null ? head.WERKS : string.Empty) + "',");
                        insertSql.Append("'" + (head.DAUAT != null ? head.DAUAT : string.Empty) + "',");
                        insertSql.Append("'" + (head.MATNR != null ? head.MATNR : string.Empty) + "',");
                        insertSql.Append("'" + (head.MAKTX != null ? head.MAKTX : string.Empty) + "',");
                        insertSql.Append("'" + (head.DISPO != null ? head.DISPO : string.Empty) + "',");
                        insertSql.Append("'" + (head.CHARG != null ? head.CHARG : string.Empty) + "',");
                        insertSql.Append("'" + (head.GSTRS != null ? head.GSTRS : string.Empty) + "',");
                        insertSql.Append("'" + (head.CY_SEQNR != null ? head.CY_SEQNR : string.Empty) + "',");
                        insertSql.Append("'" + (head.GMEIN != null ? head.GMEIN : string.Empty) + "',");
                        insertSql.Append(head.GAMNG.ToString() + ",");
                        insertSql.Append("'" + (head.LGORT != null ? head.LGORT : string.Empty) + "',");
                        insertSql.Append("'" + (head.LTEXT != null ? head.LTEXT : string.Empty) + "',");
                        insertSql.Append("'" + (head.ZLINE != null ? head.ZLINE : string.Empty) + "',");
                        insertSql.Append("'" + (head.RSNUM != null ? head.RSNUM : string.Empty) + "',");
                        insertSql.Append("'" + (head.AUFPL != null ? head.AUFPL : string.Empty) + "',");
                        insertSql.Append("'" + (head.VERSION != null ? head.VERSION : string.Empty) + "'");
                        insertSql.Append(")");
                        recordaccumulateCount++;
                        if (recordaccumulateCount == recordInsertBatchSize)
                        {
                            this.genericMgr.UpdateWithNativeQuery(insertSql.ToString());
                            insertSql = new StringBuilder();
                            recordaccumulateCount = 0;
                            if (i + 1 != orderHeadAry.Length)
                            {
                                PrepareInsertProdOrderSql(insertSql);
                            }
                        }
                        else
                        {
                            if (i + 1 != orderHeadAry.Length)
                            {
                                insertSql.Append(",");
                            }
                        }
                    }
                }
                #endregion

                #region 插入生产订单组件信息
                if (orderBomAry != null && orderBomAry.Length > 0)
                {
                    PrepareInsertProdBomDetSql(insertSql);
                    for (int i = 0; i < orderBomAry.Length; i++)
                    {

                        ZITEM_ZJ bom = orderBomAry[i];
                        insertSql.Append("(");
                        insertSql.Append(batchNo.ToString() + ",");
                        //insertSql.Append("'" + createDate.ToString("yyyy/MM/dd HH:mm:ss") + "',");
                        insertSql.Append("'" + (bom.AUFNR != null ? bom.AUFNR : string.Empty) + "',");
                        insertSql.Append("'" + (bom.RSNUM != null ? bom.RSNUM : string.Empty) + "',");
                        insertSql.Append("'" + (bom.RSPOS != null ? bom.RSPOS : string.Empty) + "',");
                        insertSql.Append("'" + (bom.WERKS != null ? bom.WERKS : string.Empty) + "',");
                        insertSql.Append("'" + (bom.MATERIAL != null ? bom.MATERIAL : string.Empty) + "',");
                        insertSql.Append("'" + (bom.BISMT != null ? bom.BISMT : string.Empty) + "',");
                        insertSql.Append("'" + (bom.MAKTX != null ? bom.MAKTX : string.Empty) + "',");
                        insertSql.Append("'" + (bom.DISPO != null ? bom.DISPO : string.Empty) + "',");
                        insertSql.Append("'" + (bom.BESKZ != null ? bom.BESKZ : string.Empty) + "',");
                        insertSql.Append("'" + (bom.SOBSL != null ? bom.SOBSL : string.Empty) + "',");
                        insertSql.Append("'" + (bom.MEINS != null ? bom.MEINS : string.Empty) + "',");
                        insertSql.Append(bom.MDMNG.ToString() + ",");
                        insertSql.Append(bom.MDMNG.ToString() + ",");
                        insertSql.Append("'" + (bom.LGORT != null ? bom.LGORT : string.Empty) + "',");
                        insertSql.Append("'" + (bom.BWART != null ? bom.BWART : string.Empty) + "',");
                        insertSql.Append("'" + (bom.AUFPL != null ? bom.AUFPL : string.Empty) + "',");
                        insertSql.Append("'" + (bom.PLNFL != null ? bom.PLNFL : string.Empty) + "',");
                        insertSql.Append("'" + (bom.VORNR != null ? bom.VORNR : string.Empty) + "',");
                        insertSql.Append("'" + (bom.GW != null ? bom.GW : string.Empty) + "',");
                        insertSql.Append("'" + (bom.WZ != null ? bom.WZ : string.Empty) + "',");
                        insertSql.Append("'" + (bom.ZOPID != null ? bom.ZOPID : string.Empty) + "',");
                        insertSql.Append("'" + (bom.ZOPDS != null ? bom.ZOPDS : string.Empty) + "',");
                        insertSql.Append("'" + (bom.LIFNR != null ? bom.LIFNR : string.Empty) + "',");
                        insertSql.Append("'" + (bom.BATCH != null ? bom.BATCH : string.Empty) + "',");
                        insertSql.Append("'" + (bom.RGEKZ != null ? bom.RGEKZ : string.Empty) + "'");
                        insertSql.Append(")");
                        recordaccumulateCount++;
                        if (recordaccumulateCount == recordInsertBatchSize)
                        {
                            this.genericMgr.UpdateWithNativeQuery(insertSql.ToString());
                            insertSql = new StringBuilder();
                            recordaccumulateCount = 0;
                            if (i + 1 != orderBomAry.Length)
                            {
                                PrepareInsertProdBomDetSql(insertSql);
                            }
                        }
                        else
                        {
                            if (i + 1 != orderBomAry.Length)
                            {
                                insertSql.Append(",");
                            }
                        }
                    }
                }
                #endregion

                #region 插入生产订单工艺路线信息
                if (orderOpAry != null && orderOpAry.Length > 0)
                {
                    PrepareInsertProdRoutingDetSql(insertSql);
                    for (int i = 0; i < orderOpAry.Length; i++)
                    {
                        ZITEM_LX op = orderOpAry[i];
                        insertSql.Append("(");
                        insertSql.Append(batchNo.ToString() + ",");
                        //insertSql.Append("'" + createDate.ToString("yyyy/MM/dd HH:mm:ss") + "',");
                        insertSql.Append("'" + (op.AUFNR != null ? op.AUFNR : string.Empty) + "',");
                        insertSql.Append("'" + (op.WERKS != null ? op.WERKS : string.Empty) + "',");
                        insertSql.Append("'" + (op.AUFPL != null ? op.AUFPL : string.Empty) + "',");
                        insertSql.Append("'" + (op.APLZL != null ? op.APLZL : string.Empty) + "',");
                        insertSql.Append("'" + (op.PLNTY != null ? op.PLNTY : string.Empty) + "',");
                        insertSql.Append("'" + (op.PLNNR != null ? op.PLNNR : string.Empty) + "',");
                        insertSql.Append("'" + (op.PLNAL != null ? op.PLNAL : string.Empty) + "',");
                        insertSql.Append("'" + (op.PLNFL != null ? op.PLNFL : string.Empty) + "',");
                        insertSql.Append("'" + (op.VORNR != null ? op.VORNR : string.Empty) + "',");
                        insertSql.Append("'" + (op.ARBPL != null ? op.ARBPL : string.Empty) + "',");
                        insertSql.Append("'" + (op.RUEK != null ? op.RUEK : string.Empty) + "',");
                        insertSql.Append("'" + (op.AUTWE != null ? op.AUTWE : string.Empty) + "'");
                        insertSql.Append(")");
                        recordaccumulateCount++;
                        if (recordaccumulateCount == recordInsertBatchSize)
                        {
                            this.genericMgr.UpdateWithNativeQuery(insertSql.ToString());
                            insertSql = new StringBuilder();
                            recordaccumulateCount = 0;
                            if (i + 1 != orderOpAry.Length)
                            {
                                PrepareInsertProdRoutingDetSql(insertSql);
                            }
                        }
                        else
                        {
                            if (i + 1 != orderOpAry.Length)
                            {
                                insertSql.Append(",");
                            }
                        }
                    }
                }
                #endregion

                if (insertSql.Length > 0)
                {
                    this.genericMgr.UpdateWithNativeQuery(insertSql.ToString());
                }
                log.DebugFormat("完成插入生产订单数据至中间表, 生产订单号{0}，BatchNo {1}", orderHeadAry[0].AUFNR, batchNo);

                return batchNo;
            }
            catch (Exception ex)
            {
                log.Error("插入生产订单临时表失败。");
                log.Error(ex);

                throw new BusinessException("插入生产订单临时表失败。");
            }
        }

        private void PrepareInsertProdOrderSql(StringBuilder insertSql)
        {
            //insertSql.Append("INSERT INTO SAP_ProdOrder(BatchNo,CreateDate,AUFNR,WERKS,DAUAT,MATNR,MAKTX,DISPO,CHARG,GSTRS,CY_SEQNR,GMEIN,GAMNG,LGORT,LTEXT,ZLINE,RSNUM,AUFPL)VALUES(");
            insertSql.Append("INSERT INTO SAP_ProdOrder(BatchNo,AUFNR,WERKS,DAUAT,MATNR,MAKTX,DISPO,CHARG,GSTRS,CY_SEQNR,GMEIN,GAMNG,LGORT,LTEXT,ZLINE,RSNUM,AUFPL,[VERSION])VALUES");
        }

        private void PrepareInsertProdBomDetSql(StringBuilder insertSql)
        {
            insertSql.Append("INSERT INTO SAP_ProdBomDet(BatchNo,AUFNR,RSNUM,RSPOS,WERKS,MATERIAL,BISMT,MAKTX,DISPO,BESKZ,SOBSL,MEINS,MDMNG,MDMNG_ORG,LGORT,BWART,AUFPL,PLNFL,VORNR,GW,WZ,ZOPID,ZOPDS,LIFNR,ICHARG,RGEKZ) VALUES ");
        }

        private void PrepareInsertProdRoutingDetSql(StringBuilder insertSql)
        {
            //insertSql.Append("INSERT INTO SAP_ProdRoutingDet(BatchNo,CreateDate,AUFNR,WERKS,AUFPL,APLZL,PLNTY,PLNNR,PLNAL,PLNFL,VORNR,ARBPL,RUEK,AUTWE) VALUES");
            insertSql.Append("INSERT INTO SAP_ProdRoutingDet(BatchNo,AUFNR,WERKS,AUFPL,APLZL,PLNTY,PLNNR,PLNAL,PLNFL,VORNR,ARBPL,RUEK,AUTWE) VALUES");
        }
        #endregion

        #region 获取非整车生产单
        private static object GetProductOrderLock = new object();
        public IList<string> GetProductOrder(string plant, IList<string> sapOrderNoList)
        {
            lock (GetProductOrderLock)
            {
                try
                {
                    IList<int> batchNoList = GetProductOrder(plant, sapOrderNoList, null, DateOption.EQ, null, null, null);

                    foreach (int batchNo in batchNoList)
                    {
                        log.DebugFormat("开始生成非整车生产订单, BatchNo {0}", batchNo);
                        User user = SecurityContextHolder.Get();
                        this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_GenProductOrder ?,?,?", new object[] { batchNo, user.Id, user.FullName });
                        log.DebugFormat("结束生成非整车生产订单, BatchNo {0}", batchNo);
                    }

                    string sql = string.Empty;
                    IList<object> parms = new List<object>();
                    foreach (int batchNo in batchNoList)
                    {
                        if (sql == string.Empty)
                        {
                            sql = "select Msg from LOG_GenProductOrder where BatchNo in (?";
                        }
                        else
                        {
                            sql += ",?";
                        }
                        parms.Add(batchNo);
                    }
                    sql += ")";

                    return this.genericMgr.FindAllWithNativeSql<string>(sql, parms.ToArray());
                }
                catch (BusinessException ex)
                {
                    log.Error("生成非整车生产单失败。", ex);

                    throw ex;
                }
                catch (Exception ex)
                {
                    string exMessage = ex.InnerException != null ? (ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.InnerException.Message) : ex.Message;
                    log.ErrorFormat("生成非整车生产单出现异常，异常信息{0}。", exMessage);
                    log.Error(ex);

                    throw new BusinessException("生成非整车生产单出现异常，异常信息{0}。", exMessage);
                }
            }
        }

        public IList<string> GetProductOrder(string plant, IList<string> sapOrderTypeList, DateOption dateOption, DateTime? dateFrom, DateTime? dateTo, IList<string> mrpCtrlList)
        {
            lock (GetProductOrderLock)
            {
                try
                {
                    IList<int> batchNoList = GetProductOrder(plant, null, sapOrderTypeList, dateOption, dateFrom, dateTo, mrpCtrlList);

                    foreach (int batchNo in batchNoList)
                    {
                        log.DebugFormat("开始生成非整车生产订单, BatchNo {0}", batchNo);
                        User user = SecurityContextHolder.Get();
                        this.genericMgr.UpdateWithNativeQuery("exec USP_Busi_GenProductOrder ?,?,?", new object[] { batchNo, user.Id, user.FullName });
                        log.DebugFormat("结束生成非整车生产订单, BatchNo {0}", batchNo);
                    }

                    string sql = string.Empty;
                    IList<object> parms = new List<object>();
                    foreach (int batchNo in batchNoList)
                    {
                        if (sql == string.Empty)
                        {
                            sql = "select Msg from LOG_GenProductOrder where BatchNo in (?";
                        }
                        else
                        {
                            sql += ",?";
                        }
                        parms.Add(batchNo);
                    }
                    sql += ")";

                    return this.genericMgr.FindAllWithNativeSql<string>(sql, parms.ToArray());
                }
                catch (BusinessException ex)
                {
                    log.Error("生成非整车生产单失败。", ex);

                    throw ex;
                }
                catch (Exception ex)
                {
                    string exMessage = ex.InnerException != null ? (ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.InnerException.Message) : ex.Message;
                    log.ErrorFormat("生成非整车生产单出现异常，异常信息{0}。", exMessage);
                    log.Error(ex);

                    throw new BusinessException("生成非整车生产单出现异常，异常信息{0}。", exMessage);
                }
            }
        }

        private IList<int> GetProductOrder(string plant, IList<string> sapOrderNoList, IList<string> sapOrderTypeList, DateOption dateOption, DateTime? dateFrom, DateTime? dateTo, IList<string> mrpCtrlList)
        {
            log.DebugFormat("开始获取非整车生产订单，工厂{0}", plant);
            ZHEAD[] orderHeadAry = null;
            ZITEM_LX[] orderOpAry = null;
            ZITEM_ZJ[] orderBomAry = null;
            try
            {
                MI_PO_LESService soService = new MI_PO_LESService();
                soService.Credentials = base.Credentials;
                soService.Timeout = base.TimeOut;
                soService.Url = ReplaceSAPServiceUrl(soService.Url);

                #region 生产订单号
                ZRANGE_AUFNR[] AUFNR = new ZRANGE_AUFNR[sapOrderNoList != null ? sapOrderNoList.Count() : 0];
                if (sapOrderNoList != null && sapOrderNoList.Count() > 0)
                {
                    for (int i = 0; i < sapOrderNoList.Count(); i++)
                    {
                        string sapOrderNo = sapOrderNoList[i];
                        AUFNR[i] = new ZRANGE_AUFNR();
                        AUFNR[i].SIGN = "I";
                        AUFNR[i].OPTION = "EQ";
                        AUFNR[i].LOW = sapOrderNo;
                    }
                }
                #endregion

                #region 订单类型
                ZRANGE_DAUAT[] DAUAT = new ZRANGE_DAUAT[sapOrderTypeList != null ? sapOrderTypeList.Count() : 0];
                if (sapOrderTypeList != null && sapOrderTypeList.Count() > 0)
                {
                    for (int i = 0; i < sapOrderTypeList.Count(); i++)
                    {
                        string sapOrderType = sapOrderTypeList[i];
                        DAUAT[i] = new ZRANGE_DAUAT();
                        DAUAT[i].SIGN = "I";
                        DAUAT[i].OPTION = "EQ";
                        DAUAT[i].LOW = sapOrderType;
                    }
                }
                #endregion

                #region 计划开始日期
                ZRANGE_GSTRS[] GSTRS = new ZRANGE_GSTRS[dateFrom.HasValue || dateTo.HasValue ? 1 : 0];

                if (dateFrom.HasValue || dateTo.HasValue)
                {
                    GSTRS[0] = new ZRANGE_GSTRS();
                    GSTRS[0].SIGN = "I";
                    switch (dateOption)
                    {
                        case DateOption.EQ:
                            GSTRS[0].OPTION = "EQ";
                            break;
                        case DateOption.GT:
                            GSTRS[0].OPTION = "GT";
                            break;
                        case DateOption.GE:
                            GSTRS[0].OPTION = "GE";
                            break;
                        case DateOption.LT:
                            GSTRS[0].OPTION = "LT";
                            break;
                        case DateOption.LE:
                            GSTRS[0].OPTION = "LE";
                            break;
                        case DateOption.BT:
                            GSTRS[0].OPTION = "BT";
                            break;
                    }
                    if (dateFrom.HasValue)
                    {
                        GSTRS[0].LOW = dateFrom.Value.ToString("yyyyMMdd");
                    }
                    if (dateTo.HasValue)
                    {
                        GSTRS[0].HIGH = dateTo.Value.ToString("yyyyMMdd");
                    }
                }
                #endregion

                #region MRP控制者
                ZRANGE_DISPO[] DISPO = new ZRANGE_DISPO[mrpCtrlList != null ? mrpCtrlList.Count() : 0];
                if (mrpCtrlList != null && mrpCtrlList.Count() > 0)
                {
                    for (int i = 0; i < mrpCtrlList.Count(); i++)
                    {
                        string mrpCtrl = mrpCtrlList[i];
                        DISPO[i] = new ZRANGE_DISPO();
                        DISPO[i].SIGN = "I";
                        DISPO[i].OPTION = "EQ";
                        DISPO[i].LOW = mrpCtrl;
                    }
                }
                #endregion

                string returnMessage = null;
                log.DebugFormat("连接WebService获取非整车订单，工厂{0}。", plant);
                orderHeadAry = soService.MI_PO_LES(AUFNR, DAUAT, DISPO, GSTRS, plant, "", "", out orderOpAry, out orderBomAry, out returnMessage);

                if (!string.IsNullOrWhiteSpace(returnMessage))
                {
                    log.ErrorFormat("获取非整车生产单失败，失败信息：{0}。", returnMessage);

                    throw new BusinessException(returnMessage);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? (ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.InnerException.Message) : ex.Message;
                log.ErrorFormat("获取非整车生产单出现异常，异常信息：{0}。", errorMessage);
                log.Error(ex);

                throw new BusinessException("获取非整车生产单出现异常，异常信息：{0}。", errorMessage);
            }

            var aufnrCollection = orderHeadAry.Select(oh => oh.AUFNR);
            IList<int> batchNoList = new List<int>();
            foreach (string AUFNR in aufnrCollection)
            {
                int batchNo = 0;
                //如果是试制订单要用batchno来比较，2013-10-14
                ZHEAD[] toInsertedAry = orderHeadAry.Where(oh => oh.AUFNR == AUFNR).ToArray();

                if (toInsertedAry[0].DAUAT == "ZP01" || toInsertedAry[0].DAUAT == "ZP02")
                    batchNo = InsertTmpTable(toInsertedAry, orderOpAry.Where(oo => oo.AUFNR == AUFNR).ToArray(), orderBomAry.Where(ob => ob.BATCH == AUFNR.Substring(2, AUFNR.Length - 2)).ToArray());
                else
                    batchNo = InsertTmpTable(toInsertedAry, orderOpAry.Where(oo => oo.AUFNR == AUFNR).ToArray(), orderBomAry.Where(ob => ob.AUFNR == AUFNR).ToArray());
                batchNoList.Add(batchNo);
            }

            return batchNoList;
        }

        #endregion

        #region 生产单报工
        private static object ReportProdOrderOperationLock = new object();
        public void ReportProdOrderOperation()
        {
            lock (ReportProdOrderOperationLock)
            {
                log.Debug("开始传输生产报工信息至SAP。");
                IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                try
                {
                    int maxFailCount = int.Parse(this.systemMgr.GetEntityPreferenceValue(Entity.SYS.EntityPreference.CodeEnum.SAPDataExchangeMaxFailCount));

                    IList<ProdOpReport> prodOpReportList = this.genericMgr.FindAll<ProdOpReport>(
                        "from ProdOpReport where Status = ? or (Status = ? and ErrorCount < ?) order by CreateDate",
                        new object[] { StatusEnum.Pending, StatusEnum.Fail, maxFailCount });

                    #region 报工
                    if (prodOpReportList != null && prodOpReportList.Count > 0)
                    {
                        foreach (ProdOpReport prodOpReport in prodOpReportList)
                        {
                            reportProdOrderOperationMgr.ReportProdOrderOperation(prodOpReport, errorMessageList, maxFailCount);
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    string logMessage = string.Format("传输生产报工信息至SAP出现异常, 异常信息：{0}", ex.Message);
                    log.Error(logMessage, ex);
                    errorMessageList.Add(new ErrorMessage
                    {
                        Template = NVelocityTemplateRepository.TemplateEnum.ReportProdOrderOpFail,
                        Exception = ex,
                        Message = logMessage
                    });
                }

                this.SendErrorMessage(errorMessageList);
            }

            log.Debug("传输生产报工信息至SAP结束。");
        }
        #endregion

        #region 手工重发报工信息
        public void ReportProdOrderOperation(IList<ProdOpReport> prodOpReportList)
        {
            lock (ReportProdOrderOperationLock)
            {
                log.Debug("开始传输生产报工信息至SAP。");
                IList<ErrorMessage> errorMessageList = new List<ErrorMessage>();
                try
                {
                    #region 报工
                    if (prodOpReportList != null && prodOpReportList.Count > 0)
                    {
                        foreach (ProdOpReport prodOpReport in prodOpReportList)
                        {
                            reportProdOrderOperationMgr.ReportProdOrderOperation(prodOpReport, errorMessageList, int.MaxValue);
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    string logMessage = string.Format("传输生产报工信息至SAP出现异常, 异常信息：{0}", ex.Message);
                    log.Error(logMessage, ex);
                    errorMessageList.Add(new ErrorMessage
                    {
                        Template = NVelocityTemplateRepository.TemplateEnum.ReportProdOrderOpFail,
                        Exception = ex,
                        Message = logMessage
                    });
                }

                this.SendErrorMessage(errorMessageList);
            }

            log.Debug("传输生产报工信息至SAP结束。");
        }
        #endregion
    }

    [Transactional]
    public class ReportProdOrderOperationMgrImpl : BaseMgr, IReportProdOrderOperationMgr
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SAP_Productoin");

        [Transaction(TransactionMode.Requires)]
        public void ReportProdOrderOperation(ProdOpReport prodOpReport, IList<ErrorMessage> errorMessageList, int maxFailCount)
        {
            #region 生产单报工
            try
            {
                log.DebugFormat("开始连接Web服务进行生产报工, 生产单号{0}, 工作中心{1}，数量{2}，废品数{3}", prodOpReport.AUFNR, prodOpReport.WORKCENTER, prodOpReport.GAMNG, prodOpReport.SCRAP);

                prodOpReport.Status = Entity.SAP.StatusEnum.Success;
                this.UpdateSiSap(prodOpReport);
                this.genericMgr.FlushSession();

                MI_PO_CFR_LESService service = new MI_PO_CFR_LESService();
                service.Credentials = base.Credentials;
                service.Timeout = base.TimeOut;
                service.Url = ReplaceSAPServiceUrl(service.Url);

                ZSPOCOMF input = new ZSPOCOMF();
                input.AUFNR = prodOpReport.AUFNR;
                input.ARBPL = prodOpReport.WORKCENTER;
                input.LMNGA = prodOpReport.GAMNG;
                input.LMNGASpecified = input.LMNGA != 0;
                input.SCRAP = prodOpReport.SCRAP;
                input.SCRAPSpecified = input.SCRAP != 0;
                input.TEXT = prodOpReport.Id.ToString();
                //将工序的报工时间传给sap
                input.POSTG_DATE = prodOpReport.CreateDate.ToString("yyyyMMdd");
                //将报工工序的aufpl，plnfl，vornr传给sap
                OrderOperation orderOp = genericMgr.FindById<OrderOperation>(prodOpReport.OrderOpId);
                input.AUFPL = orderOp.AUFPL;
                input.PLNFL = orderOp.PLNFL;
                input.VORNR = orderOp.VORNR;

                string result = service.MI_PO_CFR_LES(input);

                if (result.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
                {
                    log.DebugFormat("生产报工成功，生产单号{0}, 工作中心{1}，数量{2}，废品数{3}。", prodOpReport.AUFNR, prodOpReport.WORKCENTER, prodOpReport.GAMNG, prodOpReport.SCRAP);
                }
                else
                {
                    prodOpReport.Status = Entity.SAP.StatusEnum.Fail;
                    prodOpReport.TEXT = result.Substring(0, result.Length < 250 ? result.Length : 250);
                    prodOpReport.ErrorCount++;
                    this.UpdateSiSap(prodOpReport);

                    string logMessage = string.Format("生产报工失败，生产单号{0}, 工作中心{1}，数量{2}，废品数{3}，失败信息：{4}。", prodOpReport.AUFNR, prodOpReport.WORKCENTER, prodOpReport.GAMNG, prodOpReport.SCRAP, result);
                    log.Error(logMessage);
                    if (prodOpReport.ErrorCount >= maxFailCount)
                    {
                        errorMessageList.Add(new ErrorMessage
                        {
                            Template = NVelocityTemplateRepository.TemplateEnum.ReportProdOrderOpFail,
                            Message = logMessage
                        });
                    }
                }

                this.genericMgr.FlushSession();
            }
            catch (Exception ex)
            {
                this.genericMgr.CleanSession();

                prodOpReport.Status = Entity.SAP.StatusEnum.Fail;
                prodOpReport.TEXT = ex.Message.Substring(0, ex.Message.Length < 250 ? ex.Message.Length : 250);
                prodOpReport.ErrorCount++;
                this.UpdateSiSap(prodOpReport);

                string logMessage = string.Format("生产报工出现异常，生产单号{0}, 工作中心{1}，数量{2}，废品数{3}，异常信息：{4}。", prodOpReport.AUFNR, prodOpReport.WORKCENTER, prodOpReport.GAMNG, prodOpReport.SCRAP, ex.Message);
                log.Error(logMessage, ex);
                if (prodOpReport.ErrorCount == 10)
                {
                    errorMessageList.Add(new ErrorMessage
                    {
                        Template = NVelocityTemplateRepository.TemplateEnum.ReportProdOrderOpFail,
                        Exception = ex,
                        Message = logMessage
                    });
                }
            }
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void CancelReportProdOrderOperation(string AUFNR, string TEXT)
        {
            try
            {
                #region 生产单报工已经传给SAP
                log.DebugFormat("开始连接Web服务进行取消生产报工, 生产单号{0}, 行号{1}", AUFNR, TEXT);

                #region 取消报工传给SAP
                MI_POCANCLE_LESService service = new MI_POCANCLE_LESService();
                service.Credentials = base.Credentials;
                service.Timeout = base.TimeOut;
                service.Url = ReplaceSAPServiceUrl(service.Url);

                ZSPOCOMF_YZ input = new ZSPOCOMF_YZ();
                input.AUFNR = AUFNR;
                input.CONF_TEXT = TEXT;

                string result = service.MI_POCANCLE_LES(input);
                #endregion

                #region 判断返回结果
                if (result.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
                {
                    log.DebugFormat("取消生产报工成功, 生产单号{0}, 行号{1}", AUFNR, TEXT);
                }
                else
                {
                    string logMessage = string.Format("取消生产报工失败, 生产单号{0}, 行号{1}，失败信息：{2}", AUFNR, TEXT, result);
                    log.Error(logMessage);

                    throw new BusinessException(result);
                }
                #endregion
                #endregion
            }
            catch (Exception ex)
            {
                string logMessage = string.Format("取消生产报工异常, 生产单号{0}, 行号{1}，异常信息：{2}", AUFNR, TEXT, ex.Message);
                log.Error(logMessage, ex);

                throw new BusinessException(logMessage);
            }
        }
    }
}
