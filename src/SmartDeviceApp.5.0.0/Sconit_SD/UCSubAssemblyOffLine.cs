using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;

namespace com.Sconit.SmartDevice
{
    public partial class UCSubAssemblyOffLine : UCBase
    {
        //public event MainForm.ModuleSelectHandler ModuleSelectionEvent;

        private static UCSubAssemblyOffLine ucSubAssemblyOffLine;
        private static object obj = new object();
        private OrderMaster orderMaster;
        private List<OrderMaster> subKitOrders;
        private DateTime? effDate;
        private Location location;
        private List<OrderDetailInput> orderDetailInputs;

        private UCSubAssemblyOffLine(User user)
            : base(user)
        {
            this.InitializeComponent();
            base.btnOrder.Text = "下线";
        }

        public static UCSubAssemblyOffLine GetUCSubAssemblyOffLine(User user)
        {
            if (ucSubAssemblyOffLine == null)
            {
                lock (obj)
                {
                    if (ucSubAssemblyOffLine == null)
                    {
                        ucSubAssemblyOffLine = new UCSubAssemblyOffLine(user);
                    }
                }
            }
            ucSubAssemblyOffLine.Reset();
            return ucSubAssemblyOffLine;
        }


        protected override void ScanBarCode()
        {
            base.ScanBarCode();

            if (this.orderMaster == null)
            {
                if (base.op == CodeMaster.BarCodeType.ORD.ToString())
                {
                    //var orderMaster = base.smartDeviceService.GetOrderKeyParts(base.barCode);
                    if (orderMaster.PauseStatus == PauseStatus.Paused)
                    {
                        throw new BusinessException("订单已暂停");
                    }
                    //检查订单类型
                    //else if (orderMaster.Type != OrderType.Transfer && orderMaster.OrderStrategy != FlowStrategy.KIT)
                    //{
                    //    throw new BusinessException("扫描的单号不为分装生产单。");
                    //}
                    //检查订单状态,应该为Close
                    //else if (orderMaster.Status != OrderStatus.Close)
                    //{
                    //    throw new BusinessException("扫描的分装生产单状态不正确");
                    //}
                    List<OrderDetail> orderDetails = new List<OrderDetail>();
                    List<OrderMaster> kitOrders = null;//this.smartDeviceService.GetKitBindingOrders(orderMaster.OrderNo).ToList();
                    foreach (var kitOrder in kitOrders)
                    {
                        var orderDetail = new OrderDetail();
                        orderDetail.Item = kitOrder.OrderNo;
                        orderDetail.CurrentQty = 1;
                        orderDetail.OrderedQty = 1;
                        orderDetail.IsScanHu = true;
                        orderDetail.ItemDescription = "分装生产单";
                        orderDetails.Add(orderDetail);
                    }

                    orderMaster.OrderDetails = orderMaster.OrderDetails.Union(orderDetails).ToArray();
                    this.lblMessage.Text = "请扫描关键件条码或者子生产单。";
                    this.orderMaster = orderMaster;
                    this.gvListDataBind();
                    //this.MergeOrderMaster(orderMaster);
                }
                else
                {
                    throw new BusinessException("请扫描分装生产单。");
                }
            }
            else
            {
                if (base.barCode.Length == 17 && Utility.IsValidateLotNo(base.barCode.Substring(9, 4)) == true)
                {
                    base.op = CodeMaster.BarCodeType.HU.ToString();
                }
                if (base.op == CodeMaster.BarCodeType.HU.ToString())
                {

                    Hu hu = new Hu();
                    try
                    {
                        hu = this.smartDeviceService.GetHu(this.barCode);
                    }
                    catch
                    {
                        if (this.barCode.Length == 17)
                        {
                            hu = this.smartDeviceService.ResolveHu(this.barCode, this.user.Code);
                        }
                    }
                    this.MatchHu(hu);
                }
                else if (base.op == CodeMaster.BarCodeType.ORD.ToString())
                {
                    if (isCancel)
                    {
                        if (this.hus.Any(h => h.HuId == base.barCode))
                        {
                            this.hus.Remove(this.hus.FirstOrDefault(h => h.HuId == base.barCode));
                        }
                        else
                        {
                            throw new BusinessException("条码未扫入，不可以取消");
                        }
                    }
                    else
                    {
                        var orderMaster = base.smartDeviceService.GetOrder(base.barCode, false);
                        if (orderMaster.PauseStatus == PauseStatus.Paused)
                        {
                            throw new BusinessException("订单已暂停");
                        }
                        //检查订单类型
                        //else if (orderMaster.Type != OrderType.Transfer && orderMaster.OrderStrategy != FlowStrategy.KIT)
                        //{
                        //    throw new BusinessException("扫描的单号不为分装生产单。");
                        //}
                        else if (orderMaster.Status != OrderStatus.Close)
                        {
                            throw new BusinessException("扫描的分装生产单未关闭");
                        }
                        else if (this.orderMaster.OrderDetails.All(o => o.Item != base.barCode))
                        {
                            throw new BusinessException("分装生产单{0}不是生产单{1}的子生产单", base.barCode, this.orderMaster.OrderNo);
                        }
                        
                        var orderDetail = this.orderMaster.OrderDetails.FirstOrDefault(h => h.Item == base.barCode);
                        orderDetail.ReceivedQty = 1;
                        orderDetail.CurrentQty = 0;
                        Hu hu = new Hu();
                        hu.HuId = orderMaster.OrderNo;
                        hu.Item = orderMaster.OrderNo;
                        hu.ItemDescription = "分装生产单";
                        this.hus.Add(hu);
                        this.subKitOrders.Add(orderMaster);
                        this.gvListDataBind();
                        this.lblMessage.Text = "请扫描关键件条码或者子生产单。";
                    }
                }
                else if (this.op == CodeMaster.BarCodeType.L.ToString())
                {
                    this.barCode = this.barCode.Substring(2, this.barCode.Length - 2);
                    Location location = smartDeviceService.GetLocation(this.barCode);

                    //检查权限
                    if (!Utility.HasPermission(user.Permissions, OrderType.Transfer, false, true, null, location.Region))
                    {
                        throw new BusinessException("没有此区域的权限");
                    }
                    this.location = location;
                }
                else if (base.op == CodeMaster.BarCodeType.DATE.ToString())
                {
                    base.barCode = base.barCode.Substring(2, base.barCode.Length - 2);
                    this.effDate = base.smartDeviceService.GetEffDate(base.barCode);

                    this.lblMessage.Text = "生效时间:" + this.effDate.Value.ToString("yyyy-MM-dd HH:mm");
                    this.tbBarCode.Text = string.Empty;
                    //this.tbBarCode.Focus();
                }
                else
                {
                    throw new BusinessException("条码格式不合法");
                }
            }
        }

        private void MatchHu(Hu hu)
        {
            base.CheckHu(hu);

            if (!base.isCancel)
            {
                #region 条码匹配
                //if (hu.Status != HuStatus.Location)
                //{
                //    if (this.location == null)
                //    {
                //        throw new BusinessException("请先扫描库位条码。");
                //    }
                //}


                var orderDetails = new List<OrderDetail>();
                //var orderMaster = this.orderMasters.First();
                string huId = hu.HuId;

                //先按开始日期排序，在按订单序号排序
                //foreach (var om in orderMasters.OrderBy(o => o.StartTime))
                //{
                //    orderDetails.AddRange(om.OrderDetails.Where(o => o.OrderedQty > o.ShippedQty).OrderBy(o => o.Sequence));
                //}



                var matchedOrderDetail = this.orderMaster.OrderDetails.Where(o => o.Item == hu.Item && o.IsScanHu).FirstOrDefault();
                if (matchedOrderDetail == null)
                {
                    throw new BusinessException("分装生产单{0}不需要扫描零件号为{1}的零件。", this.orderMaster.OrderNo, hu.Item);
                }
                else
                {
                    if (matchedOrderDetail.OrderedQty != hu.Qty)
                    {
                        throw new BusinessException("条码{0}的数量不等于零件{1}需要投入的数量。", huId, hu.Item);
                    }
                }
                matchedOrderDetail.ReceivedQty = hu.Qty;
                matchedOrderDetail.CurrentQty = matchedOrderDetail.CurrentQty - hu.Qty;
                #endregion

                OrderDetailInput orderDetailInput = new OrderDetailInput();
                orderDetailInput.HuId = hu.HuId;
                orderDetailInput.ReceiveQty = hu.Qty;
                orderDetailInput.LotNo = hu.LotNo;
                orderDetailInput.Id = matchedOrderDetail.Id;
                if (hu.Status == HuStatus.Location)
                {
                    orderDetailInput.IsHuInLocation = true;
                }
                else
                {
                    orderDetailInput.IsHuInLocation = false;
                }
                this.orderDetailInputs.Add(orderDetailInput);

                this.lblMessage.Text = "请扫描关键件条码或者子生产单。";
                base.hus.Insert(0, hu);
            }
            else
            {
                #region 取消
                this.CancelHu(hu);
                #endregion
            }
            this.gvListDataBind();
        }

        protected override void gvListDataBind()
        {
            base.gvListDataBind();
            List<OrderDetail> orderDetailList = new List<OrderDetail>();
            if (this.orderMaster != null)
            {
                orderDetailList.AddRange(this.orderMaster.OrderDetails.Where(o => o.CurrentQty > 0 && o.IsScanHu));
            }
            base.dgList.DataSource = orderDetailList;
            base.ts.MappingName = orderDetailList.GetType().Name;
        }

        protected override void Reset()
        {
            this.subKitOrders = new List<OrderMaster>();
            this.orderMaster = null;
            this.orderDetailInputs = new List<OrderDetailInput>();
            base.Reset();
            this.lblMessage.Text = "请扫描分装生产单";
            this.effDate = null;
        }

        protected override void DoSubmit()
        {
            try
            {
                List<OrderDetailInput> orderDetailInputList = new List<OrderDetailInput>();
                if (this.orderMaster == null)
                {
                    throw new BusinessException("请先扫描分装生产单。");
                }
                if (this.orderMaster.OrderDetails.Where(o => o.ReceivedQty != o.OrderedQty && o.IsScanHu) != null && this.orderMaster.OrderDetails.Where(o => o.ReceivedQty != o.OrderedQty && o.IsScanHu).Count() > 0)
                {
                    throw new BusinessException("关键件或子生产单未全部扫描。");
                }
                else
                {
                    orderDetailInputList.AddRange(this.orderDetailInputs);
                }
                //2012-06-09 下线直接收货,一步收货不用创建IP。
                //this.smartDeviceService.DoReceiveOrder(this.orderMaster.OrderNo,orderDetailInputList.ToArray(), this.effDate, base.user.Code);
                //this.smartDeviceService.DoShipOrder(orderDetailInputList.ToArray(), this.effDate, this.user.Code);
                //this.smartDeviceService.KitOrderOffline(this.orderMaster.OrderNo, orderDetailInputList.ToArray(), this.subKitOrders.Select(o => o.OrderNo).ToArray(), this.effDate, base.user.Code);
                this.Reset();
                base.lblMessage.Text = "下线成功";
            }
            catch (Exception ex)
            {
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
                //this.tbBarCode.Focus();
                Utility.ShowMessageBox(ex.Message);
            }
        }

        protected override Hu DoCancel()
        {
            Hu firstHu = base.DoCancel();
            this.CancelHu(firstHu);
            return firstHu;
        }

        private void CancelHu(Hu hu)
        {
            if (this.hus == null)
            {
                this.Reset();
            }
            if (this.hus.Count > 0)
            {
                this.hus.Remove(hu);
            }
        }
    }
}
