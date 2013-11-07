
namespace com.Sconit.Web.Controllers.ORD
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using com.Sconit.Service;
    using com.Sconit.Web.Util;
    using Telerik.Web.Mvc;
    using System;
    using AutoMapper;
    using com.Sconit.Entity.MD;
    using NHibernate.Criterion;
    using com.Sconit.Entity.Exception;
    using com.Sconit.Utility;
    using Telerik.Web.Mvc.UI;
    using System.Collections;
    using System.Web;
    using com.Sconit.Entity.ORD;
    using com.Sconit.PrintModel.ORD;
    using com.Sconit.Utility.Report;
    using com.Sconit.Entity.PRD;
    using System.Linq;
    using com.Sconit.Entity.SCM;
    using NHibernate;
    using NHibernate.Type;


    public class TransferOrderController : WebAppBaseController
    {
        //public IIpMgr ipMgr { get; set; }
        public IOrderMgr orderMgr { get; set; }
        public IStockTakeMgr stockTakeMgr { get; set; }
        public IReportGen reportGen { get; set; }
        public TransferOrderController()
        {
        }

        [SconitAuthorize(Permissions = "Url_TransferOrder_View,Url_OrderMstr_Procurement_Import,Url_OrderMstr_Procurement_ReturnNew,Url_OrderMstr_Procurement_ReturnQuickNew")]
        public ActionResult Index(int? id)
        {
            if (id.HasValue)
            { 
                switch (id.Value)
	            {
                    case  10:
                        ViewBag.ManuCode="Url_TransferOrder_View";//移库
                        break;
                    case 20:
                        ViewBag.ManuCode = "Url_OrderMstr_Procurement_Import";//要货单导入
                        break;
                    case 30:
                        ViewBag.ManuCode = "Url_OrderMstr_Procurement_ReturnNew";//退货
                        break;
                    case 40:
                        ViewBag.ManuCode = "Url_OrderMstr_Procurement_ReturnQuickNew";//快速退货
                        break;
		            default:
                        ViewBag.ManuCode = "Url_TransferOrder_View";
                        break;
                }
            }
            IList<Uom> uoms = base.genericMgr.FindAll<Uom>();
            ViewData.Add("uoms", uoms);
            ViewBag.Status = com.Sconit.CodeMaster.StockTakeStatus.Create;
            return View();
        }

        [SconitAuthorize(Permissions = "Url_TransferOrder_View,Url_OrderMstr_Procurement_Import,Url_OrderMstr_Procurement_ReturnNew,Url_OrderMstr_Procurement_ReturnQuickNew")]
        public ActionResult ImportFreeLocationDetail(IEnumerable<HttpPostedFileBase> attachments,string manuCode,bool isAutoPrint,string shift)
        {
            ViewBag.ManuCode = manuCode;
            try
            {
                if (string.IsNullOrWhiteSpace(shift) && manuCode == "Url_OrderMstr_Procurement_Import")
                {
                    throw new BusinessException("要货原因代码不能为空。");
                }
                foreach (var file in attachments)
                {
                    string orderNos = orderMgr.CreateTransferOrderFromXls(shift,file.InputStream, manuCode);
                    string printUrl = string.Empty;
                    if (isAutoPrint)
                    {
                        string[] orderArr = orderNos.Split('*');
                        foreach (var orderNo in orderArr)
                        {
                            printUrl += Print(orderNo)+"*";
                        }
                        SaveWarningMessage(printUrl.Substring(0, printUrl.Length - 1));
                    }
                    SaveSuccessMessage("导入成功，生成单号:" + orderNos );
                }
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                SaveErrorMessage("导入失败。 - " + ex.Message);
            }

            return Content(string.Empty);
        }


        [GridAction(EnableCustomBinding = true)]
        [SconitAuthorize(Permissions = "Url_TransferOrder_View,Url_OrderMstr_Procurement_Import,Url_OrderMstr_Procurement_ReturnNew,Url_OrderMstr_Procurement_ReturnQuickNew")]
        public ActionResult _Select()
        {
            return PartialView(new GridModel(new List<OrderDetail>()));
        }

        public ActionResult _OrderDetailList(string manuCode)
        {
            ViewBag.ManuCode = manuCode;
            IList<Uom> uoms = base.genericMgr.FindAll<Uom>();
            ViewData.Add("uoms", uoms);
            return PartialView();
        }


        public ActionResult _WebInserintDetail(string itemCode)
        {
            if (!string.IsNullOrEmpty(itemCode))
            {
                Item item = base.genericMgr.FindById<Item>(itemCode);
              
                return this.Json(item);
            }
            return null;
        }


        [SconitAuthorize(Permissions = "Url_TransferOrder_View,Url_OrderMstr_Procurement_Import,Url_OrderMstr_Procurement_ReturnNew,Url_OrderMstr_Procurement_ReturnQuickNew")]
        public JsonResult CreateOrder(string PartyFrom, string PartyTo, DateTime? StartTime, DateTime? WindowTime, string manuCode, bool isAutoPrint,string shift, [Bind(Prefix =
                  "inserted")]IEnumerable<OrderDetail> insertedOrderDetails, [Bind(Prefix =
                  "updated")]IEnumerable<OrderDetail> updatedOrderDetails)
        {
            try
            {
                ViewBag.ManuCode = manuCode;
                if (string.IsNullOrEmpty(PartyFrom))
                {
                    throw new BusinessException("来源区域不能为空！");
                }
                if (string.IsNullOrWhiteSpace(shift) && manuCode == "Url_OrderMstr_Procurement_Import")
                {
                    throw new BusinessException(string.Format("{0}不能为空！",Resources.ORD.OrderMaster.OrderMaster_Shift_CreateOrderCode));
                }
                //if (int.Parse(Consignment) == 1)
                //{
                //    if (string.IsNullOrEmpty(manufactureParty))
                //    {
                //        throw new BusinessException("寄售状态供应商为必填！");
                //    }
                //}
                //else//现在后台是按明细有供应商就是寄售 所以选择非寄售的话，把供应商清空。
                //{
                //    manufactureParty = null;
                //}

                if (StartTime==null)
                {
                    StartTime = DateTime.Now;
                }
                if (WindowTime == null && manuCode == "Url_OrderMstr_Procurement_Import")
                {
                    throw new BusinessException("窗口时间不能为空！");
                }
                if (WindowTime == null)
                {
                    WindowTime = System.DateTime.Now;
                }
                else {
                    if (WindowTime < System.DateTime.Now)
                    {
                        throw new BusinessException("窗口时间不能小于当前时间！");
                    }
                }
                if (string.IsNullOrEmpty(PartyTo))
                {
                    PartyTo = PartyFrom;
                }

                if (manuCode == "Url_OrderMstr_Procurement_Import")
                {
                    var workingCalendars = this.genericMgr.FindAll<WorkingCalendar>(" select w from WorkingCalendar as w where w.Region=? and w.WorkingDate=? ", new object[] { PartyTo, WindowTime.Value.Date });
                    if (workingCalendars != null && workingCalendars.Count > 0)
                    {
                        if (insertedOrderDetails.Select(io => io.LocationFrom).Distinct().Count() > 1)
                        {
                            throw new BusinessException(string.Format("手工要货，不能同时向多个库位要货。"));
                        }
                        if (workingCalendars.First().Type == com.Sconit.CodeMaster.WorkingCalendarType.Rest)
                        {
                            throw new BusinessException(string.Format("窗口时间{0}是休息时间，请确认。", WindowTime));
                        }
                        string times = (WindowTime.Value.Hour).ToString().PadLeft(2, '0') + ":" + (WindowTime.Value.Minute).ToString().PadLeft(2, '0');
                        var shiftDet = this.genericMgr.FindAll<ShiftDetail>(" select s from ShiftDetail as s where s.Shift=? and s.StartTime<=? and s.EndTime>=? ", new object[] { workingCalendars.FirstOrDefault().Shift, times, times });
                        if (shiftDet == null || shiftDet.Count == 0)
                        {
                            throw new BusinessException(string.Format("窗口时间{0}是休息时间，请确认。", WindowTime));
                        }
                    }
                }

                int i = 0;
                foreach (OrderDetail orderDetail in insertedOrderDetails)
                {
                    i++;
                    if (orderDetail.OrderedQty == 0)
                    {
                        throw new BusinessException("第" + i + "行明细数量不能为空！");
                    }
                    if (string.IsNullOrEmpty(orderDetail.LocationFrom))
                    {
                        throw new BusinessException("第" + i + "行来源库位不能为空。");
                    }
                    if (string.IsNullOrEmpty(orderDetail.LocationTo))
                    {
                        throw new BusinessException("第" + i + "行目的库位不能为空。");
                    }
                    if (string.IsNullOrEmpty(orderDetail.Item))
                    {
                        throw new BusinessException("第" + i + "行明细物料不能为空！");
                    }

                    else
                    {
                        if (manuCode == "Url_OrderMstr_Procurement_Import")
                        {
                           string hql= " select fd from FlowDetail as fd where Item=? and exists( select 1 from FlowMaster as fm where fm.Code=fd.Flow and fm.PartyFrom=? and fm.PartyTo=? and fm.Type=2 ) ";
                           var flowDetails = this.genericMgr.FindAll<FlowDetail>(hql, new object[] { orderDetail.Item, PartyFrom, PartyTo }, new IType[] { NHibernateUtil.String, NHibernateUtil.String, NHibernateUtil.String });
                           if (flowDetails != null && flowDetails.Count > 0)
                           {
                               if (flowDetails.FirstOrDefault().RoundUpOption != com.Sconit.CodeMaster.RoundUpOption.None)
                               {
                                   if (orderDetail.OrderedQty % flowDetails.FirstOrDefault().MinUnitCount > 0)
                                   {
                                       throw new BusinessException(string.Format("第{0}行明细物料{1}的要货数{2}不符合圆整包装，包装系数为{3}不能为空！",i,orderDetail.Item,orderDetail.OrderedQty,flowDetails.FirstOrDefault().MinUnitCount));
                                   }
                               }
                           }
                        }
                        Item item = base.genericMgr.FindById<Item>(orderDetail.Item);
                        orderDetail.ItemDescription = item.Description;
                        orderDetail.ReferenceItemCode = item.ReferenceCode;
                        orderDetail.Uom = item.Uom;
                        orderDetail.BaseUom = item.Uom;
                        orderDetail.UnitCount = item.UnitCount;
                        orderDetail.QualityType = com.Sconit.CodeMaster.QualityType.Qualified;

                    }
                    //orderDetail.ManufactureParty = manufactureParty != string.Empty ? manufactureParty : null;
                    OrderDetailInput orderDetailInput = new OrderDetailInput();
                    orderDetailInput.ReceiveQty = orderDetail.OrderedQty;
                    orderDetailInput.ConsignmentParty = orderDetail.ManufactureParty != string.Empty ? orderDetail.ManufactureParty : null;
                    orderDetailInput.QualityType = orderDetail.QualityType;

                    if (manuCode == "Url_TransferOrder_View") //快速移库的需要 将供应商存到 orderdetailinput
                    {
                        orderDetailInput.ConsignmentParty = !string.IsNullOrWhiteSpace(orderDetail.ManufactureParty) ? orderDetail.ManufactureParty : null;
                        orderDetail.ManufactureParty = null;
                    }

                    orderDetail.AddOrderDetailInput(orderDetailInput);
                }
                bool isReturn = false;
                bool isQuick = true;
                if (manuCode == "Url_TransferOrder_View")
                {
                    isReturn = false;
                    isQuick = true;
                }
                else if (manuCode == "Url_OrderMstr_Procurement_Import")
                {
                    isReturn = false;
                    isQuick = false;
                }
                else if (manuCode == "Url_OrderMstr_Procurement_ReturnNew")
                {
                    isReturn = true;
                    isQuick = false;
                }
                else if (manuCode == "Url_OrderMstr_Procurement_ReturnQuickNew")
                {
                    isReturn = true;
                    isQuick = true;
                }
                IList<OrderDetail> orderDetailList = insertedOrderDetails as List<OrderDetail>;
                string orderNo = orderMgr.CreateFreeTransferOrderMaster(shift,PartyFrom, PartyTo, orderDetailList, StartTime.Value, WindowTime.Value, isQuick, isReturn);
                SaveSuccessMessage("操作成功！生成单据号：" + orderNo);
                string printUrl = string.Empty;
                if (isAutoPrint)
                {
                    printUrl=Print(orderNo);
                }
                return Json(new { printUrl = printUrl });
            }
            catch (BusinessException ex)
            {
                SaveBusinessExceptionMessage(ex);
            }
            catch(Exception ex)
            {
                SaveErrorMessage(ex);
            }
            return Json(null);
        }


        public string Print(string orderNo)
        {
            OrderMaster orderMaster = base.genericMgr.FindById<OrderMaster>(orderNo);
            IList<OrderDetail> orderDetails = base.genericMgr.FindAll<OrderDetail>("select od from OrderDetail as od where od.OrderNo=?", orderNo);
            orderMaster.OrderDetails = orderDetails;
            PrintOrderMaster printOrderMstr = Mapper.Map<OrderMaster, PrintOrderMaster>(orderMaster);
            IList<object> data = new List<object>();
            data.Add(printOrderMstr);
            data.Add(printOrderMstr.OrderDetails);
            //string reportFileUrl = reportGen.WriteToFile(orderMaster.OrderTemplate, data);
            string reportFileUrl = reportGen.WriteToFile("ORD_Transfer.xls", data);
            return reportFileUrl;
        }
    }
}
