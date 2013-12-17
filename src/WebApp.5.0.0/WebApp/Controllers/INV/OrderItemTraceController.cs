using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Telerik.Web.Mvc;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.ORD;
using com.Sconit.Service;
using com.Sconit.Web.Models;
using com.Sconit.Web.Util;
using com.Sconit.Web.Models.SearchModels.INV;
using com.Sconit.Entity.MD;
using com.Sconit.Web.Models.SearchModels.ORD;
using com.Sconit.Entity.CUST;

namespace com.Sconit.Web.Controllers.INV
{
    public class OrderItemTraceController : WebAppBaseController
    {
        #region Properties
        public IOrderMgr orderMgr { get; set; }

        public IInspectMgr iInspectMgr { get; set; }

        #endregion

        private static string selectCountStatement = "select count(*) from OrderItemTraceResult as r";

        private static string selectStatement = "select r from OrderItemTraceResult as r";

        #region 关键件追溯
        [SconitAuthorize(Permissions = "Url_OrderItemTraceResult_View")]
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderItemTraceResult_View")]
        public ActionResult List(GridCommand command, OrderItemTraceSearchModel searchModel)
        {
            SearchCacheModel searchCacheModel = this.ProcessSearchModel(command, searchModel);
            ViewBag.PageSize = base.ProcessPageSize(command.PageSize);
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_OrderItemTraceResult_View")]
        public ActionResult _AjaxList(GridCommand command, OrderItemTraceSearchModel searchModel)
        {
            SearchStatementModel searchStatementModel = this.PrepareSearchStatement(command, searchModel);
            return PartialView(GetAjaxPageData<OrderItemTraceResult>(searchStatementModel, command));
        }

        public void ExportOrderItemTraceResultXLS(OrderItemTraceSearchModel searchModel)
        {
            string hql = " select r from OrderItemTraceResult as r where 1=1 ";
            IList<object> param = new List<object>();

            if (!string.IsNullOrWhiteSpace(searchModel.BarCode))
            {
                hql += " and i.BarCode = ? ";
                param.Add(searchModel.BarCode);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Item))
            {
                hql += " and i.Item = ? ";
                param.Add(searchModel.Item);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.OpReference))
            {
                hql += " and i.OpReference = ? ";
                param.Add(searchModel.OpReference);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.TraceCode))
            {
                hql += " and i.TraceCode = ? ";
                param.Add(searchModel.TraceCode);
            }

            string supplierStatement = string.Empty;
            if (!string.IsNullOrWhiteSpace(searchModel.Suppliers))
            {
                string suppliers = searchModel.Suppliers.Replace("\r\n", ",");
                suppliers = suppliers.Replace("\n", ",");
                string[] supplierArr = suppliers.Split(',');
                for (int i = 0; i < supplierArr.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(supplierStatement))
                    {
                        supplierStatement = " and Supplier in (? ";
                    }
                    else
                    {
                        supplierStatement += " ,? ";
                    }
                    param.Add(supplierArr[i]);
                }
                hql += supplierStatement + " ) ";
            }

            string lotNoStatement = string.Empty;
            if (!string.IsNullOrWhiteSpace(searchModel.LotNos))
            {
                string lotNos = searchModel.LotNos.Replace("\r\n", ",");
                lotNos = lotNos.Replace("\n", ",");
                string[] lotNoArr = lotNos.Split(',');
                for (int i = 0; i < lotNoArr.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lotNoStatement))
                    {
                        lotNoStatement = " and LotNo in (? ";
                    }
                    else
                    {
                        lotNoStatement += " ,? ";
                    }
                    param.Add(lotNoArr[i]);
                }
                hql += lotNoStatement + " ) ";
            }

            hql += " order by r.CreateDate desc ";
            IList<OrderItemTraceResult> exportList = this.genericMgr.FindAll<OrderItemTraceResult>(hql,param.ToArray());
            ExportToXLS<OrderItemTraceResult>("OrderItemTraceResultXLS", "xls", exportList);
        }

        private SearchStatementModel PrepareSearchStatement(GridCommand command, OrderItemTraceSearchModel searchModel)
        {
            string whereStatement = " where 1=1 ";

            IList<object> param = new List<object>();
            string supplierStatement = string.Empty;
            if (!string.IsNullOrWhiteSpace(searchModel.Suppliers))
            {
                string suppliers = searchModel.Suppliers.Replace("\r\n", ",");
                suppliers = suppliers.Replace("\n", ",");
                string[] supplierArr = suppliers.Split(',');
                for (int i = 0; i < supplierArr.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(supplierStatement))
                    {
                        supplierStatement = " and Supplier in (? ";
                    }
                    else
                    {
                        supplierStatement += " ,? ";
                    }
                    param.Add(supplierArr[i]);
                }
                whereStatement += supplierStatement + " ) ";  
            }

            string lotNoStatement = string.Empty;
            if (!string.IsNullOrWhiteSpace(searchModel.LotNos))
            {
                string lotNos = searchModel.LotNos.Replace("\r\n", ",");
                lotNos = lotNos.Replace("\n", ",");
                string[] lotNoArr = lotNos.Split(',');
                for (int i = 0; i < lotNoArr.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lotNoStatement))
                    {
                        lotNoStatement = " and LotNo in (? ";
                    }
                    else
                    {
                        lotNoStatement += " ,? ";
                    }
                    param.Add(lotNoArr[i]);
                }
                whereStatement += lotNoStatement + " ) ";
            }

            HqlStatementHelper.AddEqStatement("BarCode", searchModel.BarCode, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("Item", searchModel.Item, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("OpReference", searchModel.OpReference, "r", ref whereStatement, param);
            HqlStatementHelper.AddEqStatement("TraceCode", searchModel.TraceCode, "r", ref whereStatement, param);

            string sortingStatement = HqlStatementHelper.GetSortingStatement(command.SortDescriptors);

            SearchStatementModel searchStatementModel = new SearchStatementModel();
            searchStatementModel.SelectCountStatement = selectCountStatement;
            searchStatementModel.SelectStatement = selectStatement;
            searchStatementModel.WhereStatement = whereStatement;
            searchStatementModel.SortingStatement = sortingStatement;
            searchStatementModel.Parameters = param.ToArray<object>();

            return searchStatementModel;
        }
        #endregion

        [SconitAuthorize(Permissions = "Url_OrderItemTrace_New")]
        public ActionResult New()
        {
            TempData["ErrorBarCodeList"] = null;
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderItemTrace_WithdrawBarCode")]
        public ActionResult WithdrawBarCode()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderItemTrace_New")]
        public ActionResult _GetOrderItemTraceList(string orderNo, bool? isFirst)
        {
            TempData["ErrorBarCodeList"] = null;
            try
            {
                if (isFirst.HasValue && isFirst.Value)
                {
                    TempData["ErrorBarCodeList"] = null;
                }
                if (string.IsNullOrWhiteSpace(orderNo))
                {
                    SaveErrorMessage("请扫描Van号。");
                    return Json(null);
                }
                var ordermstrList = this.genericMgr.FindAll<OrderMaster>("select o from OrderMaster as o where  o.TraceCode=? and o.ProdLineType in(1,2,3,4,9)  ", orderNo);
                if (ordermstrList == null || ordermstrList.Count == 0)
                {
                    SaveErrorMessage("Van号不存在。");
                    return Json(null);
                }
                List<OrderItemTraceResult> results = new List<OrderItemTraceResult>();
                results.AddRange(this.GetOrderItemTraceInfo(orderNo));
                return PartialView(results);
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                return Json(null);
            }
            catch (Exception)
            {
                SaveErrorMessage("关键件扫描出现异常。");
                return Json(null);
            }
        }

        [SconitAuthorize(Permissions = "Url_OrderItemTrace_New,Url_OrderItemTrace_WithdrawBarCode")]
        public JsonResult ScanQualityBarCode(string orderNo, string qualityBarcode, string opRef, int? orderItemTraceId, bool isForce, bool isWithdraw, string withdrawBarcode)
        {
            IList<ErrorBarCode> errorBarCodeList = TempData["ErrorBarCodeList"] != null ? TempData["ErrorBarCodeList"] as IList<ErrorBarCode> : new List<ErrorBarCode>();
            bool isAdded = false;
            try
            {
                if (isWithdraw)
                {
                    if (string.IsNullOrWhiteSpace(qualityBarcode))
                    {
                        SaveErrorMessage("关键件条码不能为空。");
                    }
                    else if (string.IsNullOrWhiteSpace(withdrawBarcode))
                    {
                        SaveErrorMessage("旧条码不能为空。");
                    }
                    else
                    {
                        if (qualityBarcode.ToUpper() == "Y")
                        {
                            orderMgr.WithdrawQualityBarCode(withdrawBarcode);
                        }
                        else
                        {
                            orderMgr.ReplaceQualityBarCode(withdrawBarcode, qualityBarcode);
                        }
                    }
                }
                else
                {
                    orderMgr.ScanQualityBarCode(orderNo, qualityBarcode, opRef, orderItemTraceId, isForce, false);
                    isAdded = true;
                }
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                if (isWithdraw)
                {
                    return Json(null);
                }
                ErrorBarCode errorBarCode = new ErrorBarCode();
                errorBarCode.BarCode = qualityBarcode;
                errorBarCode.Message = ex.GetMessages()[0].GetMessageString();
                errorBarCode.ProdCodeSeq = orderNo;
                errorBarCode.Sequence = errorBarCodeList.Count > 0 ? errorBarCodeList.OrderByDescending(e => e.Sequence).First().Sequence + 1 : 1;
                errorBarCodeList.Add(errorBarCode);
                isAdded = false;
            }
            TempData["ErrorBarCodeList"] = errorBarCodeList;
            return Json(new { isAdded = isAdded });
        }

        public ActionResult _ErrorBarCodeList()
        {
            IList<ErrorBarCode> errorBarCodeList = TempData["ErrorBarCodeList"] != null ? TempData["ErrorBarCodeList"] as IList<ErrorBarCode> : new List<ErrorBarCode>();
            TempData["ErrorBarCodeList"] = errorBarCodeList;
            return PartialView(errorBarCodeList);
        }

        public ActionResult DeleteErrorBarCodeList(string sequence)
        {
            IList<ErrorBarCode> errorBarCodeList = TempData["ErrorBarCodeList"] != null ? TempData["ErrorBarCodeList"] as IList<ErrorBarCode> : new List<ErrorBarCode>();
            errorBarCodeList = errorBarCodeList.Where(e => e.Sequence != Convert.ToInt32(sequence)).ToList();
            TempData["ErrorBarCodeList"] = errorBarCodeList;
            return PartialView("_ErrorBarCodeList", errorBarCodeList);
        }

        [SconitAuthorize(Permissions = "Url_OrderItemTraceResult_New")]
        public ActionResult _NewItemTraceResultList(string prodCodeSeq, string sequence)
        {
            //tempData 跳action会丢失，这里保存一次
            TempData["ErrorBarCodeList"] = TempData["ErrorBarCodeList"];
            ViewBag.prodCodeSeq = prodCodeSeq.Trim();
            ViewBag.sequence = sequence;
            return PartialView();
        }

        [GridAction]
        [SconitAuthorize(Permissions = "Url_OrderItemTraceResult_New")]
        public ActionResult _SelectBatchEditing(string prodCodeSeq)
        {
            prodCodeSeq = prodCodeSeq.Trim();
            //tempData 跳action会丢失，这里保存一次
            IList<ErrorBarCode> errorBarCodeList = TempData["ErrorBarCodeList"] != null ? TempData["ErrorBarCodeList"] as IList<ErrorBarCode> : new List<ErrorBarCode>();
            TempData["ErrorBarCodeList"] = errorBarCodeList;

            IList<OrderItemTrace> orderItemTraceList = new List<OrderItemTrace>();

            orderItemTraceList = base.genericMgr.FindEntityWithNativeSql<OrderItemTrace>(@"select trace.* from ORD_OrderItemTrace as trace where trace.Qty>0 and (trace.OrderNo=? or trace.TraceCode=?) and 
Qty>( select isnull(COUNT(*),0)from ORD_OrderItemTraceResult as result where trace.Id=result.OrderItemTraceId and IsWithdraw=? ) ", new object[] { prodCodeSeq, prodCodeSeq, false });
            foreach (var result in orderItemTraceList)
            {
                if (!string.IsNullOrWhiteSpace(result.Item))
                {
                    Item item = this.genericMgr.FindById<Item>(result.Item);
                    result.ItemShortCode = item.ShortCode;
                }
            }

            return PartialView(new GridModel(orderItemTraceList));
        }

        [SconitAuthorize(Permissions = "Url_OrderItemTraceResult_New")]
        public ActionResult _NewItemTraceResultByBomId(string id, string sequence)
        {
            IList<ErrorBarCode> errorBarCodeList = TempData["ErrorBarCodeList"] != null ? TempData["ErrorBarCodeList"] as IList<ErrorBarCode> : new List<ErrorBarCode>();
            ErrorBarCode errorBarCode = errorBarCodeList.Where(e => e.Sequence == Convert.ToInt32(sequence)).ToList().FirstOrDefault();
            TempData["ErrorBarCodeList"] = errorBarCodeList;
            var orderItemTrace = base.genericMgr.FindById<OrderItemTrace>(Convert.ToInt32(id));
            try
            {
                orderMgr.ScanQualityBarCode(errorBarCode.ProdCodeSeq, errorBarCode.BarCode, orderItemTrace.OpReference, Convert.ToInt32(id), true, false);
                SaveSuccessMessage(Resources.ORD.OrderItemTraceResult.OrderItemTraceResult_Added, errorBarCode.BarCode);
                if (errorBarCodeList.Select(o => o.Sequence == int.Parse(sequence)).Count() > 0)
                {
                    TempData["ErrorBarCodeList"] = errorBarCodeList.Where(o => o.Sequence != int.Parse(sequence)).ToList();
                }
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
                return Json(null);
            }
            #region 扫描成功后 重新获取bom显示到页面
            var returnList = this.GetOrderItemTraceInfo(errorBarCode.ProdCodeSeq);
            return PartialView("_GetOrderItemTraceList", returnList);
            #endregion
        }

        /// <summary>
        /// 强制扫描
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        [SconitAuthorize(Permissions = "Url_OrderItemTraceResult_New")]
        public ActionResult _ForceNewItemTraceResult(string sequence)
        {
            IList<ErrorBarCode> errorBarCodeList = TempData["ErrorBarCodeList"] != null ? TempData["ErrorBarCodeList"] as IList<ErrorBarCode> : new List<ErrorBarCode>();
            ErrorBarCode errorBarCode = errorBarCodeList.Where(e => e.Sequence == Convert.ToInt32(sequence)).ToList().FirstOrDefault();
            TempData["ErrorBarCodeList"] = errorBarCodeList;
            try
            {
                orderMgr.ScanQualityBarCode(errorBarCode.ProdCodeSeq, errorBarCode.BarCode, null, null, true, false);
                SaveSuccessMessage(Resources.ORD.OrderItemTraceResult.OrderItemTraceResult_Added, errorBarCode.BarCode);
                if (errorBarCodeList.Select(o => o.Sequence == int.Parse(sequence)).Count() > 0)
                {
                    TempData["ErrorBarCodeList"] = errorBarCodeList.Where(o => o.Sequence != int.Parse(sequence)).ToList();
                }
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
                return Json(null);
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex);
                return Json(null);
            }
            #region 扫描成功后 重新获取bom显示到页面
            var returnList = this.GetOrderItemTraceInfo(errorBarCode.ProdCodeSeq);
            return PartialView("_GetOrderItemTraceList", returnList);
            #endregion
        }

        private List<OrderItemTraceResult> GetOrderItemTraceInfo(string traceCode)
        {
            string sql = @" select rt.*,mi.ShortCode from (select trace.OpRef,trace.Item,trace.ItemDesc,trace.Qty,trace.Id,result.BarCode,isnull(result.IsWithdraw,0) as IsWithdraw,trace.TraceCode from ORD_OrderItemTrace as trace left join
 ORD_OrderItemTraceResult as result on trace.Id=result.OrderItemTraceId where trace.TraceCode= ?
     union all
 select result.OpRef,result.Item,result.ItemDesc,1 as qty,null as OrderItemTraceId,result.BarCode,isnull(result.IsWithdraw,0) as IsWithdraw,result.TraceCode from ORD_OrderItemTraceResult as result where OrderItemTraceId is null and result.TraceCode=?
) as rt left join MD_Item as mi on rt.Item=mi.Code";
            IList<object[]> objList = base.genericMgr.FindAllWithNativeSql<object[]>(sql, new object[] { traceCode, traceCode });
            IList<OrderItemTraceResult> orderItemTraceResultList = (from tak in objList
                                                                    select new OrderItemTraceResult
                                                                    {
                                                                        OpReference = (string)tak[0],
                                                                        Item = (string)tak[1],
                                                                        ItemDescription = (string)tak[2],
                                                                        Qty = (decimal)tak[3],
                                                                        OrderItemTraceId = (int?)tak[4],
                                                                        BarCode = (string)tak[5],
                                                                        IsWithdraw = (bool)tak[6],
                                                                        TraceCode = (string)tak[7],
                                                                        ItemShortCode = (string)tak[8],
                                                                    }).ToList();
            var returnList = new List<OrderItemTraceResult>();
            var orderItemTraceResultQtyByOne = orderItemTraceResultList.Where(o => o.Qty == 1);
            var orderItemTraceResultQtyIsNoOne = orderItemTraceResultList.Where(o => o.Qty > 1);
            //var orderItemTraceIdIsNull = orderItemTraceResultList.Where(o => o.OrderItemTraceId == null || o.OrderItemTraceId == 0);
            returnList.AddRange(orderItemTraceResultQtyByOne);
            //returnList.AddRange(orderItemTraceIdIsNull);
            if (orderItemTraceResultQtyIsNoOne != null && orderItemTraceResultQtyIsNoOne.Count() > 0)
            {
                #region  bom用量大于1的
                var tempIsNoOneList = (from tak in orderItemTraceResultQtyIsNoOne
                                       group tak by new
                                       {
                                           OrderItemTraceId = tak.OrderItemTraceId
                                       }
                                           into takResult
                                           select new
                                           {
                                               OrderItemTraceId = takResult.Key.OrderItemTraceId,
                                               List = takResult.ToList(),
                                               Count = takResult.Count()
                                           }).ToList();
                foreach (var tempIsNoOne in tempIsNoOneList)
                {
                    #region 按OrderItemTraceId 关键件 拆分
                    int i = 0;
                    foreach (var result in tempIsNoOne.List)
                    {
                        #region 第一条表示 已经扫描的
                        OrderItemTraceResult currentResult = new OrderItemTraceResult()
                        {
                            Item = result.Item,
                            ItemDescription = result.ItemDescription,
                            IsWithdraw = result.IsWithdraw,
                            Qty = 1,
                            BarCode = result.BarCode,
                            OrderItemTraceId = result.OrderItemTraceId,
                            OpReference = result.OpReference,
                            TraceCode = result.TraceCode,
                            ItemShortCode = result.ItemShortCode,
                        };
                        returnList.Add(currentResult);
                        #endregion
                        i++;
                        if (i == 1)
                        {
                            #region  在第一次循环就把 bom拆成 用量条明细
                            //result.OrderQty - tempIsNoOne.List.Count 意思是 bom用量为result.OrderQty 已经扫描了 tempIsNoOne.List.Count
                            //剩下的拆分成 （result.OrderQty - tempIsNoOne.List.Count） 条明细
                            for (int j = 0; j < result.Qty - tempIsNoOne.List.Count; j++)
                            {
                                OrderItemTraceResult newResult = new OrderItemTraceResult()
                                {
                                    Item = result.Item,
                                    ItemDescription = result.ItemDescription,
                                    OpReference = result.OpReference,
                                    Qty = 1,
                                    BarCode = string.Empty,
                                    ItemShortCode = result.ItemShortCode,
                                    TraceCode = result.TraceCode,
                                };
                                returnList.Add(newResult);
                            }
                            #endregion
                        }

                    }
                    #endregion
                }
                #endregion
            }
            return returnList.OrderBy(r => r.BarCode).ToList();
        }

        public ActionResult _GetTraceResut(OrderItemTraceResultSearchModel searchModel)
        {
            try
            {
                if (!this.CheckSearchModelIsNull(searchModel))
                {
                    throw new BusinessException("请输入查询条件。");
                }
                string hql = " select r from OrderItemTraceResult as r where 1=1";
                IList<object> parameters = new List<object>();
                if (!string.IsNullOrWhiteSpace(searchModel.OrderNo))
                {
                    hql += " and  OrderNo=?";
                    parameters.Add(searchModel.OrderNo);
                }
                if (!string.IsNullOrWhiteSpace(searchModel.TraceCode))
                {
                    hql += " and  TraceCode=?";
                    parameters.Add(searchModel.TraceCode);
                }
                if (!string.IsNullOrWhiteSpace(searchModel.Item))
                {
                    hql += " and  Item=?";
                    parameters.Add(searchModel.Item);
                }
                if (!string.IsNullOrWhiteSpace(searchModel.OpReference))
                {
                    hql += " and  OpReference=?";
                    parameters.Add(searchModel.OpReference);
                }
                if (searchModel.TraceDateFrom != null)
                {
                    hql += " and  CreateDate>= ?";
                    parameters.Add(searchModel.TraceDateFrom.Value);
                }
                if (searchModel.TraceDateTo != null)
                {
                    hql += " and  CreateDate <= ?";
                    parameters.Add(searchModel.TraceDateTo.Value);
                }
                var returnList = this.genericMgr.FindAll<OrderItemTraceResult>(hql, parameters.ToArray());

                if (returnList != null && returnList.Count > 0)
                {
                    foreach (var result in returnList)
                    {
                        if (!string.IsNullOrWhiteSpace(result.Item))
                        {
                            Item item = this.genericMgr.FindById<Item>(result.Item);
                            result.ItemShortCode = item.ShortCode;
                        }
                    }
                }
                return View(returnList.OrderBy(r => r.BarCode).ToList());
            }
            catch (BusinessException ex)
            {
                SaveErrorMessage(ex.GetMessages()[0].GetMessageString());
                return Json(null);

            }
            catch (System.Exception)
            {
                SaveErrorMessage("车身条码找不到对应的一车一单号,请确认。");
                return Json(null);
            }
        }

        [SconitAuthorize(Permissions = "Url_OrderItemTrace_WithdrawBarCode")]
        public JsonResult NewWithdrawBarCode(string barCode, string newBarCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barCode))
                {
                    SaveErrorMessage("原条码不能为空。");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(newBarCode))
                    {
                        orderMgr.WithdrawQualityBarCode(barCode);
                    }
                    else
                    {
                        orderMgr.ReplaceQualityBarCode(barCode, newBarCode);
                    }
                }
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return Json(null);
        }


        #region 车架扫描
        [SconitAuthorize(Permissions = "Url_OrderItemTrace_FrameBarCode")]
        public ActionResult FrameBarCode()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderItemTrace_FrameBarCode")]
        public JsonResult ScanFremeBarCode(string traceCode, string frameBarCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(traceCode))
                {
                    SaveErrorMessage("Van号不能为空。");
                }
                else if (string.IsNullOrWhiteSpace(frameBarCode))
                {
                    SaveErrorMessage("车架条码不能为空。");
                }
                else
                {
                    this.orderMgr.ScanQualityBarCode(traceCode, frameBarCode, null, null, true, true);
                    SaveSuccessMessage(string.Format("Van号{0}车架条码{1}扫描成功。", traceCode, frameBarCode));
                }
            }
            catch (Exception ex)
            {
                SaveErrorMessage(ex.Message);
            }
            return Json(null);
        }

        #endregion

        #region 扫描发动机
        [SconitAuthorize(Permissions = "Url_OrderItemTrace_EngineTrace")]
        public ActionResult EngineTrace()
        {
            return View();
        }

        [SconitAuthorize(Permissions = "Url_OrderItemTrace_EngineTrace")]
        public JsonResult ScanEngineTraceBarCode(string engineTrace, string traceCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(engineTrace))
                {
                    throw new BusinessException ("发动机条码不能为空。");
                }
                 if (string.IsNullOrWhiteSpace(traceCode))
                {
                    throw new BusinessException("Van号不能为空。");
                }
                 this.orderMgr.ScanEngineTraceBarCode(engineTrace, traceCode);
                 SaveSuccessMessage("扫描成功。");
                 return Json(new { });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            return Json(null);
        }

        [SconitAuthorize(Permissions = "Url_OrderItemTrace_EngineTrace")]
        public ActionResult _GetEngineTraceView(OrderItemTraceResultSearchModel searchModel)
        {
                string hql = " select e from EngineTraceDet as e where 1=1";
                IList<object> parameters = new List<object>();
                if (!string.IsNullOrWhiteSpace(searchModel.EngineCodeSearch))
                {
                    hql += " and  EngineCode=?";
                    parameters.Add(searchModel.EngineCodeSearch);
                }
                if (!string.IsNullOrWhiteSpace(searchModel.TraceCodeSearch))
                {
                    hql += " and  TraceCode=?";
                    parameters.Add(searchModel.TraceCodeSearch);
                }
                if (searchModel.TraceDateFrom != null)
                {
                    hql += " and  CreateDate>= ?";
                    parameters.Add(searchModel.TraceDateFrom.Value);
                }
                if (searchModel.TraceDateTo != null)
                {
                    hql += " and  CreateDate <= ?";
                    parameters.Add(searchModel.TraceDateTo.Value);
                }
                var returnList = this.genericMgr.FindAll<EngineTraceDet>(hql, parameters.ToArray());

                return View(returnList.OrderBy(r => r.TraceCode).ToList());
        }

        #endregion

        #region 关键件条码生成
        public ActionResult CreateBarCode()
        {
            return View();
        }

        public JsonResult _GetItem(string itemCode)
        {
            if (!string.IsNullOrEmpty(itemCode))
            {
                Item item = base.genericMgr.FindById<Item>(itemCode);


                return this.Json(item);
            }
            return null;
        }

        public JsonResult _GetSupplier(string supplierCode)
        {
            if (!string.IsNullOrEmpty(supplierCode))
            {
                Supplier supplier = base.genericMgr.FindById<Supplier>(supplierCode);
                return this.Json(supplier);
            }
            return null;
        }

        #endregion
    }
}