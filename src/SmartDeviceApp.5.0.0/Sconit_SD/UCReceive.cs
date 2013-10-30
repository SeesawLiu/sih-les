using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;

namespace com.Sconit.SmartDevice
{
    public partial class UCReceive : UCBase
    {
        //public event MainForm.ModuleSelectHandler ModuleSelectionEvent;

        private List<OrderMaster> orderMasters;
        private IpMaster ipMaster;
        private string binCode;
        private DateTime? effDate;
        private bool isScanOne;
        private FlowMaster flowMaster;
        private bool isContinueScanOrder;
        //private List<IpDetailInput> ipDetailProcess;
        private static UCReceive ucReceive;
        private static object obj = new object();

        private UCReceive(User user)
            : base(user)
        {
            this.InitializeComponent();
            base.btnOrder.Text = "收货";
        }

        public static UCReceive GetUCReceive(User user)
        {
            if (ucReceive == null)
            {
                lock (obj)
                {
                    if (ucReceive == null)
                    {
                        ucReceive = new UCReceive(user);
                    }
                }
            }
            ucReceive.Reset();
            ucReceive.lblMessage.Text = "请扫描订单或送货单";
            return ucReceive;
        }

        #region Event

        protected override void ScanBarCode()
        {
            base.ScanBarCode();

            if (this.orderMasters.Count == 0 && this.ipMaster == null)
            {
                if (base.op == CodeMaster.BarCodeType.ORD.ToString())
                {
                    this.orderMasters = new List<OrderMaster>();
                    var orderMaster = smartDeviceService.GetOrder(base.barCode, true);
                    if (orderMaster.PauseStatus == PauseStatus.Paused)
                    {
                        throw new BusinessException("订单已暂停");
                    }
                    this.CheckAndMerge(orderMaster);
                    this.lblMessage.Text = "请继续扫描订单或者物料条码";
                }
                else if (base.op == CodeMaster.BarCodeType.ASN.ToString() || base.op == CodeMaster.BarCodeType.W.ToString() || base.op == CodeMaster.BarCodeType.SP.ToString())
                {
                    var ipMaster = new IpMaster();
                    if (base.op == CodeMaster.BarCodeType.ASN.ToString())
                    {
                        ipMaster = smartDeviceService.GetIp(base.barCode, true);
                    }
                    else
                    {
                        ipMaster = smartDeviceService.GetIpByWmsIpNo(base.barCode, true);
                    }

                    if (ipMaster.IpDetails == null || ipMaster.IpDetails.Length == 0)
                    {
                        throw new BusinessException("没有送货单明细");
                    }
                    if (!Utility.HasPermission(ipMaster, base.user))
                    {
                        throw new BusinessException("没有送货单的权限。");
                    }
                    if (ipMaster.IsReceiveScanHu == false && ipMaster.Type != IpType.KIT)
                    {
                        DialogResult dr = MessageBox.Show("送货单收货不需要扫描条码，是否继续?", "无需扫描", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        if (dr == DialogResult.No)
                        {
                            this.isMark = true;
                            return;
                        }
                    }
                    var ipDetailList = ipMaster.IpDetails.Where(o => o.RemainReceivedQty > 0).ToList();
                    if (ipDetailList.Count == 0)
                    {
                        throw new BusinessException("送货单{0}已完成收货。", ipMaster.IpNo);
                    }

                    ipDetailList = new List<IpDetail>();
                    ipDetailList = ipMaster.IpDetails.Where(o => !string.IsNullOrEmpty(o.GapReceiptNo)).ToList();
                    if (ipDetailList.Count > 0)
                    {
                        throw new BusinessException("送货单{0}已完成收货。", ipMaster.IpNo);
                    }
                    this.lblMessage.Text = "请扫描物料条码。";
                    this.ipMaster = ipMaster;
                    //this.tbBarCode.Focus();
                    this.gvListDataBind();
                }
                else
                {
                    throw new BusinessException("请扫描订单或送货单。");
                }
            }
            else
            {
                if (base.op == CodeMaster.BarCodeType.ORD.ToString() && this.isContinueScanOrder == true)
                {
                    var orderMaster = smartDeviceService.GetOrder(base.barCode, true);
                    if (orderMaster.PauseStatus == PauseStatus.Paused)
                    {
                        throw new BusinessException("订单已暂停。");
                    }
                    this.CheckAndMerge(orderMaster);
                }
                else if (base.op == CodeMaster.BarCodeType.HU.ToString())
                {
                    if ((this.orderMasters == null || this.orderMasters.Count() == 0) && (this.ipMaster == null))
                    {
                        throw new BusinessException("请先扫描订单。");
                    }
                    Hu hu = smartDeviceService.GetHu(barCode);

                    if (this.ipMaster == null)
                    {
                        this.MatchOrderMaster(hu);
                    }
                    else
                    {
                        this.MatchIpMaster(hu);
                    }
                    this.isContinueScanOrder = false;

                }
                else if (base.op == CodeMaster.BarCodeType.B.ToString())
                {
                    base.barCode = base.barCode.Substring(2, base.barCode.Length - 2);
                    Bin bin = smartDeviceService.GetBin(base.barCode);
                    this.binCode = bin.Code;
                    this.lblMessage.Text = "当前库格:" + bin.Code;
                    //检查权限
                    if (!Utility.HasPermission(user.Permissions, null, false, true, null, bin.Region))
                    {
                        throw new BusinessException("没有此移库路线的权限");
                    }
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

        #endregion

        #region DataBind
        protected override void gvListDataBind()
        {
            base.gvListDataBind();

            if (this.ipMaster == null)
            {
                List<OrderDetail> orderDetailList = new List<OrderDetail>();
                if (this.orderMasters != null)
                {
                    foreach (var om in this.orderMasters)
                    {
                        orderDetailList.AddRange(om.OrderDetails.Where(o => o.CurrentQty > 0));
                    }
                }
                base.dgList.DataSource = orderDetailList;
                ts.MappingName = orderDetailList.GetType().Name;
            }
            else
            {
                List<IpDetail> ipDetailList = new List<IpDetail>();
                if (this.ipMaster.IpDetails != null)
                {
                    if (this.ipMaster.Type == IpType.KIT)
                    {
                        foreach (var ipDetail in this.ipMaster.IpDetails)
                        {
                            if (ipDetail.IsScanHu && ipDetail.CurrentQty > 0)
                            {
                                ipDetailList.Add(ipDetail);
                            }
                        }
                    }
                    else
                    {
                        ipDetailList = this.ipMaster.IpDetails.Where(o => o.CurrentQty > 0).ToList();
                    }
                }
                base.ts.MappingName = ipDetailList.GetType().Name;
                base.dgList.DataSource = ipDetailList;
            }
        }

        #endregion

        #region Init Reset
        protected override void Reset()
        {
            //this.ipDetailProcess = new List<IpDetailInput>();
            this.flowMaster = null;
            this.orderMasters = new List<OrderMaster>();
            this.ipMaster = null;
            this.isContinueScanOrder = true;
            base.Reset();
            this.effDate = null;
        }
        #endregion

        protected override void DoSubmit()
        {
            if ((this.orderMasters == null || this.orderMasters.Count == 0)
                && (this.ipMaster == null))
            {
                throw new BusinessException("请先扫描订单或送货单。");
            }

            #region 要货单收货的逻辑
            //ORD收货
            if (this.ipMaster == null)
            {
                List<OrderDetail> orderDetailList = new List<OrderDetail>();
                List<OrderDetailInput> orderDetailInputList = new List<OrderDetailInput>();

                ////2012-06-09 丁丁说后台可以接受多个KIT收货。最后讨论的是只有安吉或者外部的才会合并 在前面做判断即可
                //if (this.orderMasters.Count > 1)
                //{
                //    if (this.orderMasters.Where(o => o.OrderStrategy == FlowStrategy.KIT).Count() > 0)
                //    {
                //        throw new BusinessException("KIT单不能合并收货。");
                //    }
                //}

                foreach (var om in orderMasters)
                {
                    if (om.OrderDetails != null)
                    {
                        orderDetailList.AddRange(om.OrderDetails);
                    }
                }

                //if (orderDetailList.Any(od => od.CurrentQty > 0))
                //{
                //    DialogResult dr = MessageBox.Show("本次收货有未收完的明细,是否继续?", "未全部收货", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                //    if (dr == DialogResult.No)
                //    {
                //        this.isMark = true;
                //        //this.tbBarCode.Focus();
                //        return;
                //    }
                //}

                if (this.isScanOne == false)
                {
                    foreach (var orderDetail in orderDetailList)
                    {
                        if (orderDetail.OrderedQty - orderDetail.ReceivedQty > 0)
                        {
                            var orderDetailInput = new OrderDetailInput();
                            orderDetailInput.Id = orderDetail.Id;
                            orderDetailInput.ReceiveQty = orderDetail.OrderedQty - orderDetail.ReceivedQty;
                            orderDetailInputList.Add(orderDetailInput);
                        }
                    }
                }
                else
                {
                    foreach (var orderDetail in orderDetailList)
                    {
                        if (orderDetail.OrderDetailInputs != null)
                        {
                            orderDetailInputList.AddRange(orderDetail.OrderDetailInputs);
                        }
                    }
                }

                //if (this.orderMasters[0].OrderStrategy == FlowStrategy.KIT && this.orderMasters[0].Status == OrderStatus.InProcess)
                //{
                //    this.smartDeviceService.DoReceiveKit(this.orderMasters[0].OrderNo, this.effDate, base.user.Code);
                //}

                if (orderDetailInputList.Count == 0)
                {
                    throw new BusinessException("没有扫描条码");
                }
                //if (this.orderMasters[0].OrderStrategy == FlowStrategy.KIT && this.orderMasters[0].Status == OrderStatus.Submit
                //    && this.orderMasters[0].OrderDetails.Count(o => o.CurrentQty != 0) > 0)
                //{
                //    throw new BusinessException("必须满足KIT单收货数。");
                //}
                this.smartDeviceService.DoReceiveOrder(orderDetailInputList.ToArray(), this.effDate, base.user.Code);

            }
            #endregion

            #region ASN收货逻辑
            //ASN收货
            else
            {
                List<IpDetailInput> ipDetailInputList = new List<IpDetailInput>();

                if (this.ipMaster.IpDetails.Any(od => od.CurrentQty > 0))
                {
                    DialogResult dr = MessageBox.Show("本次收货有未收完的明细,是否继续?", "未全部收货", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (dr == DialogResult.No)
                    {
                        this.isMark = true;
                        //this.tbBarCode.Focus();
                        return;
                    }
                }
                //安吉或者外部直送的排序单,全部需要扫描
                if (this.ipMaster.Type == IpType.SEQ)
                {
                    ////如果是外部的直送的排序单，必须扫描全部关键件
                    //if (this.ipMaster.PartyFrom == this.smartDeviceService.GetEntityPreference(CodeEnum.WMSAnjiRegion) || this.ipMaster.OrderType == OrderType.ScheduleLine || this.ipMaster.OrderType == OrderType.Procurement)
                    //{
                    //    foreach (var ipDetail in this.ipMaster.IpDetails)
                    //    {
                    //        //确认关键全部扫描,将条码传给后（排序单所有零件都是关键件）
                    //        if (this.ipMaster.IpDetailInputs.Where(i => i.Id == ipDetail.Id && !string.IsNullOrEmpty(i.HuId)).Count() == 0)
                    //        {
                    //            throw new BusinessException("安吉或者外部直送排序单需扫描全部关键件。");
                    //        }

                    //        var ipDetailInput = this.ipMaster.IpDetailInputs.FirstOrDefault(i => i.Id == ipDetail.Id && !string.IsNullOrEmpty(i.HuId));
                    //        ipDetailInputList.Add(ipDetailInput);
                    //    }
                    //}
                    ////内部排序要么全部扫描要么全部不扫描
                    //else
                    //{
                    //如果扫了一个必须全部扫描
                    if (this.isScanOne == true)
                    {
                        foreach (var ipDetail in this.ipMaster.IpDetails)
                        {
                            if (this.ipMaster.IpDetailInputs.Where(i => i.Id == ipDetail.Id && !string.IsNullOrEmpty(i.HuId)).Count() == 0)
                            {
                                throw new BusinessException("已扫入一个或多个条码，需将剩下的条码全部扫描。");
                            }

                            var ipDetailInput = this.ipMaster.IpDetailInputs.FirstOrDefault(i => i.Id == ipDetail.Id && !string.IsNullOrEmpty(i.HuId));
                            ipDetailInputList.Add(ipDetailInput);
                        }
                    }
                    else
                    {
                        foreach (var ipDetail in this.ipMaster.IpDetails)
                        {
                            //所有零件都直接填充数量传到后台
                            if (ipDetail.RemainReceivedQty > 0)
                            {
                                var ipDetailInput = new IpDetailInput();
                                ipDetailInput.Id = ipDetail.Id;
                                ipDetailInput.ReceiveQty = ipDetail.Qty;
                                ipDetailInputList.Add(ipDetailInput);
                            }
                        }
                    }
                    //}
                }
                //如果是KIT需要区分内部和外部，外部的KIT不需要扫描条码，内部的必须扫描全部关键件。
                if (this.ipMaster.Type == IpType.KIT)
                {
                    //外部送货的KIT单
                    if (this.ipMaster.PartyFrom == this.smartDeviceService.GetEntityPreference(CodeEnum.WMSAnjiRegion) || this.ipMaster.OrderType == OrderType.ScheduleLine || this.ipMaster.OrderType == OrderType.Procurement)
                    {
                        foreach (var ipDetail in this.ipMaster.IpDetails)
                        {
                            //所有零件都直接填充数量传到后台
                            if (ipDetail.RemainReceivedQty > 0)
                            {
                                var ipDetailInput = new IpDetailInput();
                                ipDetailInput.Id = ipDetail.Id;
                                ipDetailInput.ReceiveQty = ipDetail.Qty;
                                ipDetailInputList.Add(ipDetailInput);
                            }
                        }
                        //ipDetailInputList.AddRange(this.ipMaster.IpDetailInputs);
                    }
                    //内部移库的KIT单
                    else
                    {
                        foreach (var ipDetail in this.ipMaster.IpDetails)
                        {
                            //确认关键全部扫描,将条码传给后台
                            if (ipDetail.IsScanHu == true)
                            {
                                if (this.ipMaster.IpDetailInputs.Where(i => i.Id == ipDetail.Id && !string.IsNullOrEmpty(i.HuId)).Count() == 0)
                                {
                                    throw new BusinessException("内部KIT单需要扫描全部关键件");
                                }

                                var ipDetailInput = this.ipMaster.IpDetailInputs.FirstOrDefault(i => i.Id == ipDetail.Id && !string.IsNullOrEmpty(i.HuId));
                                ipDetailInputList.Add(ipDetailInput);
                            }
                            //非关键件直接填充数量
                            else if (ipDetail.RemainReceivedQty > 0)
                            {
                                var ipDetailInput = new IpDetailInput();
                                ipDetailInput.Id = ipDetail.Id;
                                ipDetailInput.ReceiveQty = ipDetail.Qty;
                                ipDetailInputList.Add(ipDetailInput);
                            }
                        }
                        //ipDetailInputList.AddRange(this.ipMaster.IpDetailInputs); 
                    }
                }
                //如果普通收货没有有扫描条码的，全部按数量收货
                if (this.isScanOne == false && this.ipMaster.IpDetailInputs.Count() == 0 && this.ipMaster.Type != IpType.KIT && this.ipMaster.Type != IpType.SEQ)
                {
                    foreach (var ipDetail in this.ipMaster.IpDetails)
                    {
                        if (ipDetail.Qty - ipDetail.ReceivedQty > 0)
                        {
                            var ipDetailInput = new IpDetailInput();
                            ipDetailInput.Id = ipDetail.Id;
                            ipDetailInput.ReceiveQty = ipDetail.Qty - ipDetail.ReceivedQty;
                            ipDetailInputList.Add(ipDetailInput);
                        }
                    }
                }
                else
                {
                    foreach (var ipDetailInput in this.ipMaster.IpDetailInputs)
                    {
                        //ipDetailInput.ReceiveQty = ipDetailInput.ReceiveQty;
                        if (ipDetailInput.ReceiveQty > 0)
                        {
                            ipDetailInputList.Add(ipDetailInput);
                        }
                    }
                }
                if (ipDetailInputList.Count == 0)
                {
                    this.Reset();
                    throw new BusinessException("本次收货出现错误，请重新收货");
                }
                this.smartDeviceService.DoReceiveIp(ipDetailInputList.ToArray(), this.effDate, base.user.Code);
            #endregion

            }

            this.Reset();
            base.lblMessage.Text = "收货成功";
        }

        private void MatchOrderMaster(Hu hu)
        {
            if (hu == null)
            {
                throw new BusinessException("条码不存在");
            }

            hu.CurrentQty = hu.Qty;
            var matchHu = this.hus.Where(h => h.HuId.Equals(hu.HuId, StringComparison.OrdinalIgnoreCase));

            #region 取消条码
            if (this.isCancel)
            {
                if (matchHu == null || matchHu.Count() == 0)
                {
                    throw new BusinessException("没有需要取消匹配条码:{0}", hu.HuId);
                }
                else if (matchHu.Count() == 1)
                {
                    this.CancelHu(hu);
                }
                else
                {
                    throw new Exception("匹配了多个条码");
                }
            }
            #endregion
            #region 检查条码并匹配收货条件。
            else
            {
                if (hu.IsFreeze)
                {
                    throw new BusinessException("条码被冻结!");
                }
                if (!Utility.HasPermission(user.Permissions, null, false, true, hu.Region, null))
                {
                    throw new BusinessException("没有此条码的权限");
                }
                if (hu.Status == HuStatus.Location)
                {
                    throw new BusinessException("在库存中的条码不能被收货");
                }
                if (this.orderMasters[0].Type == OrderType.Procurement)
                {
                    if (hu.Status == HuStatus.Location)
                    {
                        throw new BusinessException("条码已经在库位{0}中", hu.Location);
                    }
                    if (hu.Status == HuStatus.Ip)
                    {
                        throw new BusinessException("条码已经在途{0}");
                    }
                }
                else if (this.orderMasters[0].Type == OrderType.Transfer)
                {
                    if (hu.Status != HuStatus.Location)
                    {
                        throw new BusinessException("条码不在库位中");
                    }
                }

                if (matchHu != null && matchHu.Count() > 0)
                {
                    throw new BusinessException("条码重复扫描!");
                }

                //if (this.orderMasters[0].OrderStrategy == FlowStrategy.KIT)
                //{
                //    if (this.orderMasters.Count > 1)
                //    {
                //        this.Reset();
                //        throw new BusinessException("合并发货包括了KIT单，全部取消，重新扫描");
                //    }
                //    //接下来应该是统一的逻辑去处理 
                //}

                var orderDetailList = new List<OrderDetail>();
                foreach (var om in this.orderMasters)
                {
                    orderDetailList.AddRange(om.OrderDetails);
                }
                #region  标准匹配流程
                List<Hu> inputHus = new List<Hu>();
                List<Hu> needRemoveHus = new List<Hu>();
                inputHus.Add(hu);


                //循环条码做非条码匹配
                foreach (Hu inputHu in inputHus)
                {
                    //var matchIpDetail = new IpDetail();
                    //匹配零件，包装，供应商，质量状态(没有手动修改单包装的)
                    var matchOrderDetailsFirst = new List<OrderDetail>();
                    matchOrderDetailsFirst = orderDetailList.Where(i => i.Item == inputHu.Item &&
                                                i.UnitCount == inputHu.UnitCount &&
                                                i.Uom == inputHu.Uom &&
                                                i.ManufactureParty == inputHu.ManufactureParty &&
                                                i.RemainReceivedQty >= inputHu.Qty &&
                                                i.QualityType == inputHu.QualityType &&
                                                i.IsChangeUnitCount == false).ToList();

                    if (matchOrderDetailsFirst.Count() > 0)
                    {
                        var matchedOrderDetail = matchOrderDetailsFirst.First();

                        var orderDetailInput = new OrderDetailInput();
                        orderDetailInput.HuId = inputHu.HuId;
                        orderDetailInput.Id = matchedOrderDetail.Id;
                        orderDetailInput.LotNo = inputHu.LotNo;
                        orderDetailInput.ReceiveQty = inputHu.Qty;
                        matchedOrderDetail.RemainReceivedQty = matchedOrderDetail.RemainReceivedQty - inputHu.Qty;
                        matchedOrderDetail.CurrentQty = matchedOrderDetail.CurrentQty - inputHu.Qty;
                        matchedOrderDetail.Carton++;
                        List<OrderDetailInput> orderDetailsProcess = matchedOrderDetail.OrderDetailInputs.ToList();
                        orderDetailsProcess.Add(orderDetailInput);
                        matchedOrderDetail.OrderDetailInputs = orderDetailsProcess.ToArray();
                        needRemoveHus.Add(inputHu);
                    }

                    //匹配零件，供应商，质量状态(手动修改单包装的)
                    matchOrderDetailsFirst = new List<OrderDetail>();
                    matchOrderDetailsFirst = orderDetailList.Where(i => i.Item == inputHu.Item &&
                                                i.Uom == inputHu.Uom &&
                                                i.ManufactureParty == inputHu.ManufactureParty &&
                                                i.RemainReceivedQty >= inputHu.Qty &&
                                                i.QualityType == inputHu.QualityType &&
                                                i.IsChangeUnitCount == true).ToList();

                    if (matchOrderDetailsFirst.Count() > 0)
                    {
                        var matchedOrderDetail = matchOrderDetailsFirst.First();

                        var orderDetailInput = new OrderDetailInput();
                        orderDetailInput.HuId = inputHu.HuId;
                        orderDetailInput.Id = matchedOrderDetail.Id;
                        orderDetailInput.LotNo = inputHu.LotNo;
                        orderDetailInput.ReceiveQty = inputHu.Qty;
                        matchedOrderDetail.RemainReceivedQty = matchedOrderDetail.RemainReceivedQty - inputHu.Qty;
                        matchedOrderDetail.CurrentQty = matchedOrderDetail.CurrentQty - inputHu.Qty;
                        matchedOrderDetail.Carton++;
                        List<OrderDetailInput> orderDetailsProcess = matchedOrderDetail.OrderDetailInputs.ToList();
                        orderDetailsProcess.Add(orderDetailInput);
                        matchedOrderDetail.OrderDetailInputs = orderDetailsProcess.ToArray();
                        needRemoveHus.Add(inputHu);
                    }
                }

                foreach (var item in needRemoveHus)
                {
                    inputHus.Remove(item);
                }
                needRemoveHus = new List<Hu>();

                //如果inputHus没有全部匹配，接下来匹配零件，包装，质量状态
                if (inputHus.Count > 0)
                {
                    foreach (Hu inputHu in inputHus)
                    {
                        //var matchIpDetail = new IpDetail();
                        //再匹配零件，质量状态
                        var matchOrderDetailsFirst = new List<OrderDetail>();
                        matchOrderDetailsFirst = orderDetailList.Where(i => i.Item.Equals(inputHu.Item, StringComparison.OrdinalIgnoreCase) &&
                                                        i.UnitCount == inputHu.UnitCount &&
                                                        i.Uom == inputHu.Uom &&
                            //i.ManufactureParty.Equals(inputHu.ManufactureParty, StringComparison.OrdinalIgnoreCase) &&
                                                        i.RemainReceivedQty >= inputHu.Qty &&
                                                        i.QualityType == inputHu.QualityType &&
                                                        i.IsChangeUnitCount == false).ToList();

                        if (matchOrderDetailsFirst.Count() > 0)
                        {
                            var matchedOrderDetail = matchOrderDetailsFirst.First();

                            var orderDetailInput = new OrderDetailInput();
                            orderDetailInput.HuId = inputHu.HuId;
                            orderDetailInput.Id = matchedOrderDetail.Id;
                            orderDetailInput.LotNo = inputHu.LotNo;
                            orderDetailInput.ReceiveQty = inputHu.Qty;
                            matchedOrderDetail.RemainReceivedQty = matchedOrderDetail.RemainReceivedQty - inputHu.Qty;
                            matchedOrderDetail.CurrentQty = matchedOrderDetail.CurrentQty - inputHu.Qty;
                            matchedOrderDetail.Carton++;
                            List<OrderDetailInput> orderDetailsProcess = matchedOrderDetail.OrderDetailInputs.ToList();
                            orderDetailsProcess.Add(orderDetailInput);
                            matchedOrderDetail.OrderDetailInputs = orderDetailsProcess.ToArray();
                            needRemoveHus.Add(inputHu);
                        }

                        //再匹配零件，包装，质量状态
                        matchOrderDetailsFirst = new List<OrderDetail>();
                        matchOrderDetailsFirst = orderDetailList.Where(i => i.Item.Equals(inputHu.Item, StringComparison.OrdinalIgnoreCase) &&
                            //i.UnitCount == inputHu.UnitCount &&
                                                    i.Uom == inputHu.Uom &&
                            //i.ManufactureParty.Equals(inputHu.ManufactureParty, StringComparison.OrdinalIgnoreCase) &&
                                                    i.RemainReceivedQty >= inputHu.Qty &&
                                                    i.QualityType == inputHu.QualityType &&
                                                    i.IsChangeUnitCount == true).ToList();

                        if (matchOrderDetailsFirst.Count() > 0)
                        {
                            var matchedOrderDetail = matchOrderDetailsFirst.First();

                            var orderDetailInput = new OrderDetailInput();
                            orderDetailInput.HuId = inputHu.HuId;
                            orderDetailInput.Id = matchedOrderDetail.Id;
                            orderDetailInput.LotNo = inputHu.LotNo;
                            orderDetailInput.ReceiveQty = inputHu.Qty;
                            matchedOrderDetail.RemainReceivedQty = matchedOrderDetail.RemainReceivedQty - inputHu.Qty;
                            matchedOrderDetail.CurrentQty = matchedOrderDetail.CurrentQty - inputHu.Qty;
                            matchedOrderDetail.Carton++;
                            List<OrderDetailInput> orderDetailsProcess = matchedOrderDetail.OrderDetailInputs.ToList();
                            orderDetailsProcess.Add(orderDetailInput);
                            matchedOrderDetail.OrderDetailInputs = orderDetailsProcess.ToArray();
                            needRemoveHus.Add(inputHu);
                        }
                    }
                    foreach (var item in needRemoveHus)
                    {
                        inputHus.Remove(item);
                    }
                }

                //如果还有未匹配成功的就报错
                if (inputHus.Count > 0)
                {
                    throw new BusinessException("条码{0}在不满足订单所有收货明细行收货条件", hu.HuId, this.ipMaster.IpNo);
                }

                #endregion
                this.gvListDataBind();
                this.lblMessage.Text = "请扫描收货物料条码";
                this.hus.Add(hu);
            #endregion
            }

        }
        //private void MatchOrderMaster(Hu hu)
        //{
        //    if (this.orderMasters[0].OrderStrategy == FlowStrategy.KIT && this.orderMasters[0].Status == OrderStatus.InProcess)
        //    {
        //        throw new BusinessException("此KIT单无需扫描条码");
        //    }

        //    base.CheckHu(hu);

        //    if (!base.isCancel)
        //    {
        //        if (this.orderMasters[0].Type == OrderType.Procurement)
        //        {
        //            if (hu.Status == HuStatus.Location)
        //            {
        //                throw new BusinessException("条码已经在库位{0}中", hu.Location);
        //            }
        //            if (hu.Status == HuStatus.Ip)
        //            {
        //                throw new BusinessException("条码已经在途{0}");
        //            }
        //        }
        //        else if (this.orderMasters[0].Type == OrderType.Transfer)
        //        {
        //            if (hu.Status != HuStatus.Location)
        //            {
        //                throw new BusinessException("条码不在库位中");
        //            }
        //        }

        //        #region 条码匹配
        //        var orderDetails = new List<OrderDetail>();
        //        var orderMaster = this.orderMasters.First();
        //        string huId = hu.HuId;

        //        //先按开始日期排序，在按订单序号排序
        //        foreach (var om in orderMasters.OrderBy(o => o.StartTime))
        //        {
        //            orderDetails.AddRange(om.OrderDetails.OrderBy(o => o.Sequence));
        //        }

        //        var matchedOrderDetailList = orderDetails.Where(o => o.Item == hu.Item);
        //        if (matchedOrderDetailList == null || matchedOrderDetailList.Count() == 0)
        //        {
        //            throw new BusinessException("没有找到和条码{0}的零件号{1}匹配的订单明细。", huId, hu.Item);
        //        }

        //        matchedOrderDetailList = matchedOrderDetailList.Where(o => o.Uom.Equals(hu.Uom, StringComparison.OrdinalIgnoreCase));
        //        if (matchedOrderDetailList == null || matchedOrderDetailList.Count() == 0)
        //        {
        //            throw new BusinessException("没有找到和条码{0}的单位{1}匹配的订单明细。", huId, hu.Uom);
        //        }

        //        if (orderMaster.IsOrderFulfillUC)
        //        {
        //            matchedOrderDetailList = matchedOrderDetailList.Where(o => o.UnitCount == hu.UnitCount);
        //            if (matchedOrderDetailList == null || matchedOrderDetailList.Count() == 0)
        //            {
        //                throw new BusinessException("没有找到和条码{0}的包装数{1}匹配的订单明细。", huId, hu.UnitCount.ToString());
        //            }
        //        }

        //        matchedOrderDetailList = matchedOrderDetailList.Where(o => o.QualityType == hu.QualityType);
        //        if (matchedOrderDetailList == null || matchedOrderDetailList.Count() == 0)
        //        {
        //            throw new BusinessException("没有找到和条码{0}的质量状态匹配的订单明细。", huId);
        //        }

        //        #region 先匹配未满足订单发货数的（未超收的）
        //        OrderDetail matchedOrderDetail = MatchOrderDetail(hu, matchedOrderDetailList.Where(o => o.CurrentQty >= hu.Qty).ToList());
        //        #endregion

        //        #region 再匹配允许超收的订单，未发满但是+本次发货超收了 todo
        //        //if (matchedOrderDetail == null)
        //        //{
        //        //    IList<string> orderNoList = orderMasters.Where(o => o.IsReceiveExceed).Select(o => o.OrderNo).ToList();
        //        //    matchedOrderDetail = MatchOrderDetail(hu, matchedOrderDetailList.Where(o => (o.CurrentQty > 0)
        //        //        && (o.CurrentQty < hu.Qty) && orderNoList.Contains(o.OrderNo)).ToList());

        //        //    #region 再匹配允许超发的订单，已经满了或已经超发了
        //        //    if (matchedOrderDetail == null)
        //        //    {
        //        //        matchedOrderDetail = MatchOrderDetail(hu, matchedOrderDetailList.Where(o => (o.CurrentQty <= 0) && orderNoList.Contains(o.OrderNo)).ToList());
        //        //    }
        //        //    #endregion
        //        //}
        //        #endregion

        //        #region 未找到匹配的订单，报错信息
        //        if (matchedOrderDetail == null)
        //        {
        //            if (string.IsNullOrEmpty(hu.ManufactureParty))
        //            {
        //                //条码未指定制造商
        //                if (matchedOrderDetailList.Where(o => string.IsNullOrEmpty(o.ManufactureParty)).Count() > 0)
        //                {
        //                    //有未指定制造商的订货明细
        //                    throw new BusinessException("和条码{0}匹配的订单明细的发货数已经全部满足。", huId, hu.Item);
        //                }
        //                else
        //                {
        //                    //没有未指定制造商的订货明细
        //                    throw new BusinessException("待发货订单明细指定了制造商，而条码{0}没有指定制造商", huId);
        //                }
        //            }
        //            else
        //            {
        //                //条码指定了制造商
        //                if (matchedOrderDetailList.Where(o => o.ManufactureParty == hu.ManufactureParty).Count() > 0)
        //                {
        //                    //有未指定制造商的订货明细
        //                    throw new BusinessException("和条码{0}匹配的订单明细的发货数已经全部满足。", huId);
        //                }
        //                else
        //                {
        //                    //没有未指定制造商的订货明细
        //                    throw new BusinessException("待发货订单明细指定的制造商和条码{0}制造商{1}不匹配", huId, hu.ManufactureParty);
        //                }
        //            }
        //        }
        //        #endregion

        //        OrderDetailInput input = new OrderDetailInput();
        //        input.HuId = hu.HuId;
        //        input.ReceiveQty = hu.Qty;
        //        input.LotNo = hu.LotNo;
        //        input.Id = matchedOrderDetail.Id;

        //        List<OrderDetailInput> orderDetailInputs = new List<OrderDetailInput>();
        //        if (matchedOrderDetail.OrderDetailInputs != null)
        //        {
        //            orderDetailInputs = matchedOrderDetail.OrderDetailInputs.ToList();
        //        }
        //        orderDetailInputs.Add(input);

        //        matchedOrderDetail.OrderDetailInputs = orderDetailInputs.ToArray();
        //        matchedOrderDetail.CurrentQty -= hu.Qty;
        //        matchedOrderDetail.Carton++;
        //        base.hus.Insert(0, hu);
        //        #endregion
        //    }
        //    else
        //    {
        //        this.CancelHu(hu);
        //    }

        //    this.gvListDataBind();
        //}

        /// <summary>
        /// 匹配逻辑:
        /// 1.如果发货时扫描了条码,就按条码匹配,条码匹配上Hu.ShipQty=Hu.Qty;如果条码匹配不上,就按物料匹配,物料匹配上Hu.ShipQty=0;
        ///   如果ipDetail上匹配的条码总数量大于ipDetail.Qty,则把Hu.ShipQty=0的拿出来再重新按物料匹配一次.
        /// 2.如果发货没有扫描条码,就按物料匹配.
        /// </summary>
        /// <param name="hu"></param>
        private void MatchIpMaster(Hu hu)
        {
            if (hu == null)
            {
                throw new BusinessException("条码不存在");
            }

            hu.CurrentQty = hu.Qty;
            var matchHu = this.hus.Where(h => h.HuId.Equals(hu.HuId, StringComparison.OrdinalIgnoreCase));

            #region 取消条码
            if (this.isCancel)
            {
                if (matchHu == null || matchHu.Count() == 0)
                {
                    throw new BusinessException("没有需要取消匹配条码:{0}", hu.HuId);
                }
                else if (matchHu.Count() == 1)
                {
                    this.CancelHu(hu);
                }
                else
                {
                    throw new Exception("匹配了多个条码");
                }
            }
            #endregion
            #region 检查条码并匹配收货条件。
            else
            {
                #region 检查条码
                if (hu.IsFreeze)
                {
                    throw new BusinessException("条码被冻结!");
                }
                if (!Utility.HasPermission(user.Permissions, null, false, true, hu.Region, null))
                {
                    throw new BusinessException("没有此条码的权限");
                }
                if (hu.Status == HuStatus.Location)
                {
                    throw new BusinessException("在库存中的条码不能被收货");
                }

                if (matchHu != null && matchHu.Count() > 0)
                {
                    throw new BusinessException("条码重复扫描!");
                }
                #endregion

                var ipDetails = this.ipMaster.IpDetails;

                string huId = hu.HuId;

                if (this.ipMaster.Type == IpType.KIT)
                {
                    if (this.ipMaster.PartyFrom == this.smartDeviceService.GetEntityPreference(CodeEnum.WMSAnjiRegion) || this.ipMaster.OrderType == OrderType.ScheduleLine || this.ipMaster.OrderType == OrderType.Procurement)
                    {
                        throw new BusinessException("外部KIT单收货不需要扫描条码");
                    }
                    else
                    {
                        this.Reset();
                        throw new BusinessException("内部KIT单不需要扫描送货单。");
                    }
                }

                if (this.ipMaster.Type == IpType.SEQ)
                {
                    //如果在途库存上全部有条码，全部匹配条码，否则全部按物料匹配
                    if (this.ipMaster.IpDetails.All(i => i.RemainReceivedQty == 0))
                    {
                        throw new BusinessException("排序送货单{0}已收满。", this.ipMaster.IpNo);
                    }
                    if (this.ipMaster.IpDetailInputs.Where(i => !string.IsNullOrEmpty(i.HuId) && i.IsOriginal == true).Count() > 0)
                    {
                        //通过在途条码匹配
                        if (this.ipMaster.IpDetailInputs.All(i => i.HuId != hu.HuId))
                        {
                            throw new BusinessException("条码{0}不是排序单所需的零件。", hu.HuId);
                        }
                        else
                        {
                            var matchedIpDetailInput = this.ipMaster.IpDetailInputs.FirstOrDefault(i => i.HuId == hu.HuId);
                            matchedIpDetailInput.ReceiveQty = hu.Qty;
                            var ipDetail = this.ipMaster.IpDetails.FirstOrDefault(i => i.Id == matchedIpDetailInput.Id);
                            ipDetail.RemainReceivedQty = ipDetail.RemainReceivedQty - hu.Qty;
                            ipDetail.CurrentQty = ipDetail.CurrentQty - hu.Qty;
                            ipDetail.Carton++;
                        }
                    }
                    else
                    {
                        if (!this.ipMaster.IsReceiveScanHu)
                        {
                            throw new BusinessException("按数量收货不需要扫描条码，直接按确定收货。");
                        }
                        //通过物料号，单包装，供应商和质量状态匹配
                        int minSeq = this.ipMaster.IpDetails.Where(i => i.RemainReceivedQty > 0).Min(i => i.Sequence);
                        var needMatchIpDetail = this.ipMaster.IpDetails.FirstOrDefault(i => i.Sequence == minSeq);
                        if (needMatchIpDetail.Item == hu.Item && needMatchIpDetail.UnitCount == hu.UnitCount
                            && needMatchIpDetail.ManufactureParty == hu.ManufactureParty && needMatchIpDetail.QualityType == hu.QualityType)
                        {
                            var ipDetailInput = new IpDetailInput();
                            ipDetailInput.HuId = hu.HuId;
                            ipDetailInput.Id = needMatchIpDetail.Id;
                            ipDetailInput.LotNo = hu.LotNo;
                            ipDetailInput.ReceiveQty = hu.Qty;
                            needMatchIpDetail.RemainReceivedQty = needMatchIpDetail.RemainReceivedQty - hu.Qty;
                            needMatchIpDetail.CurrentQty = needMatchIpDetail.CurrentQty - hu.Qty;
                            needMatchIpDetail.Carton++;
                            List<IpDetailInput> ipDetailProcess = this.ipMaster.IpDetailInputs.ToList();
                            ipDetailProcess.Add(ipDetailInput);
                            this.ipMaster.IpDetailInputs = ipDetailProcess.ToArray();
                        }

                    }
                    this.hus.Add(hu);
                    this.isScanOne = true;
                }
                else
                {
                    var isNeedRematch = false;
                    var matchedHuDetailInputs = new List<IpDetailInput>();
                    //首先匹配条码，如果条码匹配的话就调整ipDetail的数量RemainReceivedQty
                    //只有在条码完全匹配并且数量不满足收货的情况下产生IpDetailInputs的全部调整
                    var ipDetailInputMatchHuIds = this.ipMaster.IpDetailInputs.Where(id => id.HuId.Equals(hu.HuId, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (ipDetailInputMatchHuIds.Count != 0)
                    {
                        var ipDetailInput = ipDetailInputMatchHuIds.First();
                        //var ipDetailMatchQty = this.ipMaster.IpDetails.Where(i => i.Id == ipDetailInput.Id && i.UnitCount ==hu.UnitCount && i.ManufactureParty==hu.ManufactureParty && i.RemainReceivedQty > hu.Qty).ToList();
                        var ipDetailMatchHu = this.ipMaster.IpDetails.Where(i => i.Id == ipDetailInput.Id).Single();
                        if (ipDetailMatchHu.RemainReceivedQty < hu.Qty)
                        {
                            isNeedRematch = true;
                            //将该零件号未匹配条码的先移开
                            foreach (var ipDetail in this.ipMaster.IpDetails.Where(i => i.Item == hu.Item))
                            {
                                matchedHuDetailInputs.AddRange(this.ipMaster.IpDetailInputs.Where(i => i.Id == ipDetail.Id && i.IsOriginal == true).ToList());
                                var matchTotal = this.ipMaster.IpDetailInputs.Where(i => i.Id == ipDetail.Id && i.IsMatchedHu == true).ToList().Sum(n => n.ReceiveQty);
                                ipDetail.RemainReceivedQty = ipDetail.Qty - ipDetail.ReceivedQty - matchTotal;
                                ipDetail.CurrentQty = ipDetail.Qty - ipDetail.ReceivedQty - matchTotal;
                            }
                            this.ipMaster.IpDetailInputs = matchedHuDetailInputs.ToArray();

                        }
                        //将满足的条码先放进去
                        ipDetailInput.ReceiveQty = hu.Qty;
                        ipDetailInput.LotNo = hu.LotNo;
                        ipDetailInput.IsMatchedHu = true;
                        ipDetailMatchHu.RemainReceivedQty = ipDetailMatchHu.RemainReceivedQty - hu.Qty;

                        ipDetailMatchHu.CurrentQty = ipDetailMatchHu.CurrentQty - hu.Qty;
                        ipDetailMatchHu.Carton++;
                        //this.ipMaster.IpDetails.Where(i => i.Id == ipDetailInput.Id).Single().CurrentQty -= hu.Qty;

                        //如果条码匹配上并且其他的零件也没有溢出那么直接返回
                        if (isNeedRematch == false)
                        {
                            base.hus.Insert(0, hu);
                            this.gvListDataBind();
                            return;
                        }
                    }

                    //以下为非条码匹配的标准流程
                    #region  标准匹配流程
                    List<Hu> inputHus = new List<Hu>();
                    List<Hu> needRemoveHus = new List<Hu>();
                    //如果有条码被从已经匹配的地方挪出来，那么这个物料除了条码已经匹配上的，其他全部重新匹配
                    if (isNeedRematch == true)
                    {
                        var matchedInputHus = (from h in this.hus
                                               from detailInput in this.ipMaster.IpDetailInputs
                                               where h.HuId == detailInput.HuId
                                               && detailInput.IsMatchedHu
                                               select h).ToList();

                        foreach (var item in this.hus)
                        {
                            if (matchedInputHus.All(m => m.HuId != item.HuId))
                            {
                                inputHus.Add(item);
                            }
                        }
                    }
                    else
                    {
                        inputHus.Add(hu);
                    }

                    //循环条码做非条码匹配
                    foreach (Hu inputHu in inputHus)
                    {
                        //var matchIpDetail = new IpDetail();
                        //匹配零件，包装，供应商，质量状态(没有手动修改单包装的)
                        var matchIpDetailsFirst = new List<IpDetail>();
                        matchIpDetailsFirst = this.ipMaster.IpDetails.Where(i => i.Item == inputHu.Item &&
                                                        i.Uom == inputHu.Uom &&
                                                        i.UnitCount == inputHu.UnitCount &&
                                                        i.ManufactureParty == inputHu.ManufactureParty &&
                                                        i.RemainReceivedQty >= inputHu.Qty &&
                                                        i.QualityType == inputHu.QualityType &&
                                                        i.IsChangeUnitCount == false).ToList();

                        if (matchIpDetailsFirst.Count() > 0)
                        {
                            var matchIpDetail = matchIpDetailsFirst.First();

                            var ipDetailInput = new IpDetailInput();
                            ipDetailInput.HuId = inputHu.HuId;
                            ipDetailInput.Id = matchIpDetail.Id;
                            ipDetailInput.LotNo = inputHu.LotNo;
                            ipDetailInput.ReceiveQty = inputHu.Qty;
                            matchIpDetail.RemainReceivedQty = matchIpDetail.RemainReceivedQty - inputHu.Qty;
                            matchIpDetail.CurrentQty = matchIpDetail.CurrentQty - inputHu.Qty;
                            matchIpDetail.Carton++;
                            List<IpDetailInput> ipDetailProcess = this.ipMaster.IpDetailInputs.ToList();
                            ipDetailProcess.Add(ipDetailInput);
                            this.ipMaster.IpDetailInputs = ipDetailProcess.ToArray();
                            needRemoveHus.Add(inputHu);
                        }

                        //匹配零件，供应商，质量状态(手动修改单包装的)
                        matchIpDetailsFirst = new List<IpDetail>();
                        matchIpDetailsFirst = this.ipMaster.IpDetails.Where(i => i.Item == inputHu.Item &&
                                                        i.Uom == inputHu.Uom &&
                                                        i.ManufactureParty == inputHu.ManufactureParty &&
                                                        i.RemainReceivedQty >= inputHu.Qty &&
                                                        i.QualityType == inputHu.QualityType &&
                                                        i.IsChangeUnitCount == true).ToList();

                        if (matchIpDetailsFirst.Count() > 0)
                        {
                            var matchIpDetail = matchIpDetailsFirst.First();

                            var ipDetailInput = new IpDetailInput();
                            ipDetailInput.HuId = inputHu.HuId;
                            ipDetailInput.Id = matchIpDetail.Id;
                            ipDetailInput.LotNo = inputHu.LotNo;
                            ipDetailInput.ReceiveQty = inputHu.Qty;
                            matchIpDetail.RemainReceivedQty = matchIpDetail.RemainReceivedQty - inputHu.Qty;
                            matchIpDetail.CurrentQty = matchIpDetail.CurrentQty - inputHu.Qty;
                            matchIpDetail.Carton++;
                            List<IpDetailInput> ipDetailProcess = this.ipMaster.IpDetailInputs.ToList();
                            ipDetailProcess.Add(ipDetailInput);
                            this.ipMaster.IpDetailInputs = ipDetailProcess.ToArray();
                            needRemoveHus.Add(inputHu);
                        }
                    }

                    foreach (var item in needRemoveHus)
                    {
                        inputHus.Remove(item);
                    }
                    needRemoveHus = new List<Hu>();

                    //如果inputHus没有全部匹配，接下来匹配零件，包装，质量状态
                    if (inputHus.Count > 0)
                    {
                        foreach (Hu inputHu in inputHus)
                        {
                            //var matchIpDetail = new IpDetail();
                            //再匹配零件，质量状态
                            var matchIpDetailsFirst = new List<IpDetail>();
                            matchIpDetailsFirst = this.ipMaster.IpDetails.Where(i => i.Item.Equals(inputHu.Item, StringComparison.OrdinalIgnoreCase) &&
                                                            i.UnitCount == inputHu.UnitCount &&
                                                            i.Uom == inputHu.Uom &&
                                //i.ManufactureParty.Equals(inputHu.ManufactureParty, StringComparison.OrdinalIgnoreCase) &&
                                                            i.RemainReceivedQty >= inputHu.Qty &&
                                                            i.QualityType == inputHu.QualityType &&
                                                            i.IsChangeUnitCount == false).ToList();

                            if (matchIpDetailsFirst.Count() > 0)
                            {
                                var matchIpDetail = matchIpDetailsFirst.First();

                                var ipDetailInput = new IpDetailInput();
                                ipDetailInput.HuId = inputHu.HuId;
                                ipDetailInput.Id = matchIpDetail.Id;
                                ipDetailInput.LotNo = inputHu.LotNo;
                                ipDetailInput.ReceiveQty = inputHu.Qty;
                                matchIpDetail.RemainReceivedQty = matchIpDetail.RemainReceivedQty - inputHu.Qty;
                                matchIpDetail.CurrentQty = matchIpDetail.CurrentQty - inputHu.Qty;
                                matchIpDetail.Carton++;
                                List<IpDetailInput> ipDetailProcess = this.ipMaster.IpDetailInputs.ToList();
                                ipDetailProcess.Add(ipDetailInput);
                                this.ipMaster.IpDetailInputs = ipDetailProcess.ToArray();
                                needRemoveHus.Add(inputHu);
                            }

                            //再匹配零件，包装，质量状态
                            matchIpDetailsFirst = new List<IpDetail>();
                            matchIpDetailsFirst = this.ipMaster.IpDetails.Where(i => i.Item.Equals(inputHu.Item, StringComparison.OrdinalIgnoreCase) &&
                                                            i.Uom == inputHu.Uom &&
                                //i.UnitCount == inputHu.UnitCount &&
                                //i.ManufactureParty.Equals(inputHu.ManufactureParty, StringComparison.OrdinalIgnoreCase) &&
                                                            i.RemainReceivedQty >= inputHu.Qty &&
                                                            i.QualityType == inputHu.QualityType &&
                                                            i.IsChangeUnitCount == true).ToList();

                            if (matchIpDetailsFirst.Count() > 0)
                            {
                                var matchIpDetail = matchIpDetailsFirst.First();

                                var ipDetailInput = new IpDetailInput();
                                ipDetailInput.HuId = inputHu.HuId;
                                ipDetailInput.Id = matchIpDetail.Id;
                                ipDetailInput.LotNo = inputHu.LotNo;
                                ipDetailInput.ReceiveQty = inputHu.Qty;
                                matchIpDetail.RemainReceivedQty = matchIpDetail.RemainReceivedQty - inputHu.Qty;
                                matchIpDetail.CurrentQty = matchIpDetail.CurrentQty - inputHu.Qty;
                                matchIpDetail.Carton++;
                                List<IpDetailInput> ipDetailProcess = this.ipMaster.IpDetailInputs.ToList();
                                ipDetailProcess.Add(ipDetailInput);
                                this.ipMaster.IpDetailInputs = ipDetailProcess.ToArray();
                                needRemoveHus.Add(inputHu);
                            }
                        }
                        foreach (var item in needRemoveHus)
                        {
                            inputHus.Remove(item);
                        }
                    }

                    //如果还有未匹配成功的就报错
                    if (inputHus.Count > 0)
                    {
                        if (isNeedRematch == false)
                        {
                            throw new BusinessException("条码{0}不满足送货单{1}明细行的收货条件", hu.HuId, this.ipMaster.IpNo);
                        }
                        else
                        {
                            throw new BusinessException("物料{0}不满足送货单{1}明细行的收货条件", hu.Item, this.ipMaster.IpNo);
                        }
                    }
                    #endregion
                }

                this.hus.Add(hu);
            #endregion
            }

            this.gvListDataBind();
        }

        //private void MatchIpItem(Hu hu)
        //{
        //    //var ipDetails = this.ipMaster.IpDetails;
        //    string huId = hu.HuId;
        //    #region 条码匹配物料
        //    //ipDetails = ipDetails.OrderBy(i => i.Id).ToArray();
        //    var matchedIpDetailList = this.ipMaster.IpDetails.Where(o => o.Item.Equals(hu.Item, StringComparison.OrdinalIgnoreCase));
        //    if (matchedIpDetailList == null || matchedIpDetailList.Count() == 0)
        //    {
        //        throw new BusinessException("没有找到和条码{0}的零件号{1}匹配的ASN明细。", huId, hu.Item);
        //    }

        //    matchedIpDetailList = matchedIpDetailList.Where(o => o.Uom.Equals(hu.Uom, StringComparison.OrdinalIgnoreCase));
        //    if (matchedIpDetailList == null || matchedIpDetailList.Count() == 0)
        //    {
        //        throw new BusinessException("没有找到和条码{0}的单位{1}匹配的ASN明细。", huId, hu.Uom);
        //    }

        //    if (ipMaster.IsReceiveFulfillUC)
        //    {
        //        matchedIpDetailList = matchedIpDetailList.Where(o => o.UnitCount == hu.UnitCount);
        //        if (matchedIpDetailList == null || matchedIpDetailList.Count() == 0)
        //        {
        //            throw new BusinessException("没有找到和条码{0}的包装数{1}匹配的ASN明细。", huId, hu.UnitCount.ToString());
        //        }
        //    }

        //    matchedIpDetailList = matchedIpDetailList.Where(o => o.QualityType == hu.QualityType);
        //    if (matchedIpDetailList == null || matchedIpDetailList.Count() == 0)
        //    {
        //        throw new BusinessException("没有找到和条码{0}的质量状态匹配的ASN明细。", huId);
        //    }

        //    #region 先匹配未满足订单发货数的（未超收的）
        //    IpDetail matchedIpDetail = MatchIpDetail(hu, matchedIpDetailList.Where(o => o.CurrentQty >= hu.Qty).ToList());
        //    #endregion

        //    #region 再匹配允许超收的订单，未发满但是+本次发货超收了 不支持
        //    if (matchedIpDetail == null && this.ipMaster.IsReceiveExceed && false)
        //    {
        //        matchedIpDetail = MatchIpDetail(hu, matchedIpDetailList.Where(o => (o.CurrentQty > 0)
        //            && (o.CurrentQty < hu.Qty)).ToList());

        //        #region 再匹配允许超收的订单，已经满了或已经超收了
        //        if (matchedIpDetail == null)
        //        {
        //            matchedIpDetail = MatchIpDetail(hu, matchedIpDetailList.Where(o => (o.CurrentQty <= 0)).ToList());
        //        }
        //        #endregion
        //    }
        //    #endregion

        //    #region 未找到匹配的订单，报错信息
        //    if (matchedIpDetail == null)
        //    {
        //        if (string.IsNullOrEmpty(hu.ManufactureParty))
        //        {
        //            //条码未指定制造商
        //            if (matchedIpDetailList.Where(o => string.IsNullOrEmpty(o.ManufactureParty)).Count() > 0)
        //            {
        //                //有未指定制造商的订货明细
        //                throw new BusinessException("和条码{0}匹配的订单明细的发货数已经全部满足。", huId, hu.Item);
        //            }
        //            else
        //            {
        //                //没有未指定制造商的订货明细
        //                throw new BusinessException("待发货订单明细指定了制造商，而条码{0}没有指定制造商", huId);
        //            }
        //        }
        //        else
        //        {
        //            //条码指定了制造商
        //            if (matchedIpDetailList.Where(o => o.ManufactureParty == hu.ManufactureParty).Count() > 0)
        //            {
        //                //有未指定制造商的订货明细
        //                throw new BusinessException("和条码{0}匹配的订单明细的发货数已经全部满足。", huId);
        //            }
        //            else
        //            {
        //                //没有未指定制造商的订货明细
        //                throw new BusinessException("待发货订单明细指定的制造商和条码{0}制造商{1}不匹配", huId, hu.ManufactureParty);
        //            }
        //        }
        //    }
        //    #endregion
        //    //matchedIpDetail.IpDetailInputs
        //    var ipDetailInputs = new List<IpDetailInput>();
        //    //if (matchedIpDetail != null)
        //    //{
        //    //    ipDetailInputs = matchedIpDetail.IpDetailInputs.ToList();
        //    //}

        //    //var ipDetailInput = new IpDetailInput();
        //    //ipDetailInput.HuId = hu.HuId;
        //    //ipDetailInput.Qty = hu.Qty;
        //    //ipDetailInput.ReceiveQty = hu.Qty;
        //    //ipDetailInput.LotNo = hu.LotNo;
        //    //ipDetailInput.Id = matchedIpDetail.Id;
        //    //ipDetailInputs.Add(ipDetailInput);
        //    //matchedIpDetail.IpDetailInputs = ipDetailInputs.ToArray();

        //    //matchedIpDetail.CurrentQty -= hu.Qty;
        //    //matchedIpDetail.Carton++;

        //    base.hus.Insert(0, hu);
        //    #endregion
        //}

        //private IpDetail MatchIpDetail(Hu hu, List<IpDetail> ipDetailList)
        //{
        //    if (ipDetailList != null && ipDetailList.Count > 0)
        //    {
        //        //先匹配发货明细的制造商
        //        var matchedOrderDetail = ipDetailList.Where(o => (o.ManufactureParty == null ? string.Empty : o.ManufactureParty.Trim())
        //            == (hu.ManufactureParty == null ? string.Empty : hu.ManufactureParty.Trim())).FirstOrDefault();

        //        //再匹配没有制造上的发货明细
        //        if (matchedOrderDetail == null)
        //        {
        //            matchedOrderDetail = ipDetailList.Where(o => string.IsNullOrEmpty(o.ManufactureParty)).FirstOrDefault();
        //        }

        //        return matchedOrderDetail;
        //    }

        //    return null;
        //}

        //private OrderDetail MatchOrderDetail(Hu hu, List<OrderDetail> orderDetailList)
        //{
        //    if (orderDetailList != null && orderDetailList.Count > 0)
        //    {
        //        //先匹配发货明细的制造商
        //        OrderDetail matchedOrderDetail = orderDetailList.Where(o => (o.ManufactureParty == null ? string.Empty : o.ManufactureParty.Trim())
        //            == (hu.ManufactureParty == null ? string.Empty : hu.ManufactureParty.Trim())).FirstOrDefault();

        //        //再匹配没有制造上的发货明细
        //        if (matchedOrderDetail == null)
        //        {
        //            matchedOrderDetail = orderDetailList.Where(o => string.IsNullOrEmpty(o.ManufactureParty)).FirstOrDefault();
        //        }

        //        return matchedOrderDetail;
        //    }
        //    return null;
        //}

        private void CheckAndMerge(OrderMaster orderMaster)
        {

            //if (orderMaster.OrderStrategy == FlowStrategy.KIT && this.orderMasters.Count>0)
            //{
            //    throw new BusinessException("KIT单不能合并收货。");
            //}

            //检查权限
            if (!Utility.HasPermission(orderMaster, base.user))
            {
                throw new BusinessException("没有此订单的权限。");
            }
            if (this.orderMasters == null)
            {
                this.orderMasters = new List<OrderMaster>();
            }
            if (orderMasters.Count(o => o.OrderNo == orderMaster.OrderNo) > 0)
            {
                //订单重复扫描检查
                throw new BusinessException("重复扫描订单。");
            }

            //检查订单类型
            if (orderMaster.Type == OrderType.Production)
            {
                throw new BusinessException("扫描的为生产单，不能收货。");
            }
            else if (orderMaster.Type == OrderType.SubContract)
            {
                throw new BusinessException("扫描的为委外生产单，不能发货。");
            }

            //检查订单状态
            if (orderMaster.Status != OrderStatus.Submit
                && orderMaster.Status != OrderStatus.InProcess)
            {
                throw new BusinessException("不是Submit或InProcess状态不能收货");
            }

            //收货扫描条码 收数量不能再手持设备上做
            //if (!orderMaster.IsReceiveScanHu)
            //{
            //    throw new BusinessException("收货不用扫描条码,不能再手持终端上操作。");
            //}

            #region IsRecCreateHu
            //var createHuOption = from om in orderMasters
            //                     where om.CreateHuOption == CreateHuOption.Receive
            //                     select om.CreateHuOption;
            //if (createHuOption != null && createHuOption.Count() > 0 && createHuOption.Count() != orderMasters.Count())
            //{
            //    throw new BusinessErrorException("收货创建条码选项不同不能合并发货。");
            //}
            #endregion

            #region 订单类型
            var orderType = orderMasters.Where(o => o.Type != orderMaster.Type);
            if (orderType.Count() > 0)
            {
                throw new BusinessException("订单类型不同不能合并收货。");
            }
            #endregion

            #region 订单质量类型
            var qualityType = orderMasters.Where(o => o.QualityType != orderMaster.QualityType);
            if (qualityType.Count() > 0)
            {
                throw new BusinessException("订单质量状态不同不能合并收货。");
            }
            #endregion

            #region PartyFrom
            var partyFrom = orderMasters.Where(o => o.PartyFrom != orderMaster.PartyFrom);
            if (partyFrom.Count() > 0)
            {
                throw new BusinessException("来源组织不同不能合并收货。");
            }
            #endregion

            #region PartyTo
            var partyTo = orderMasters.Where(o => o.PartyTo != orderMaster.PartyTo);
            if (partyTo.Count() > 0)
            {
                throw new BusinessException("目的组织不同不能合并收货。");
            }
            #endregion

            #region ShipFrom
            var shipFrom = orderMasters.Where(o => o.ShipFrom != orderMaster.ShipFrom);
            if (shipFrom.Count() > 0)
            {
                throw new BusinessException("发货地址不同不能合并收货。");
            }
            #endregion

            #region ShipTo
            var shipTo = orderMasters.Where(o => o.ShipTo != orderMaster.ShipTo);
            if (shipTo.Count() > 0)
            {
                throw new BusinessException("收货地址不同不能合并收货。");
            }
            #endregion

            #region Dock
            var dock = orderMasters.Where(o => o.Dock != orderMaster.Dock);
            if (dock.Count() > 0)
            {
                throw new BusinessException("道口不同不能合并收货。");
            }
            #endregion

            #region IsAutoReceive
            //var isAutoReceive = orderMasters.Where(o => o.IsAutoReceive != orderMaster.IsAutoReceive);
            //if (isAutoReceive.Count() > 1)
            //{
            //    throw new BusinessErrorException("自动收货选项不同不能合并收货。");
            //}
            #endregion

            #region IsShipScanHu
            //var isShipScanHu = orderMasters.Where(o => o.IsShipScanHu != orderMaster.IsShipScanHu);
            //if (isShipScanHu.Count() > 1)
            //{
            //    throw new BusinessErrorException("发货扫描条码选项不同不能合并收货。");
            //}
            #endregion

            #region IsRecScanHu
            var isRecScanHu = orderMasters.Where(o => o.IsReceiveScanHu != orderMaster.IsReceiveScanHu);
            if (isRecScanHu.Count() > 0)
            {
                throw new BusinessException("收货扫描条码选项不同不能合并收货。");
            }
            #endregion

            #region IsRecExceed
            var isRecExceed = orderMasters.Where(o => o.IsReceiveExceed != orderMaster.IsReceiveExceed);
            if (isRecExceed.Count() > 0)
            {
                throw new BusinessException("允许超收选项不同不能合并收货。");
            }
            #endregion

            #region IsRecFulfillUC
            var isRecFulfillUC = orderMasters.Where(o => o.IsReceiveFulfillUC != orderMaster.IsReceiveFulfillUC);
            if (isRecFulfillUC.Count() > 0)
            {
                throw new BusinessException("收货满足包装选项不同不能合并收货。");
            }
            #endregion

            #region IsRecFifo
            var isRecFifo = orderMasters.Where(o => o.IsReceiveFifo != orderMaster.IsReceiveFifo);
            if (isRecFifo.Count() > 0)
            {
                throw new BusinessException("收货先进先出选项不同不能合并收货。");
            }
            #endregion

            #region IsAsnAuotClose
            var isAsnAuotClose = orderMasters.Where(o => o.IsAsnAutoClose != orderMaster.IsAsnAutoClose);
            if (isAsnAuotClose.Count() > 0)
            {
                throw new BusinessException("ASN自动关闭选项不同不能合并收货。");
            }
            #endregion

            #region IsAsnUniqueRec
            //var isAsnUniqueRec = orderMasters.Where(o => o.IsAsnUniqueReceive != orderMaster.IsAsnUniqueReceive);
            //if (isAsnUniqueRec.Count() > 1)
            //{
            //    throw new BusinessErrorException("ASN一次性收货选项不同不能合并收货。");
            //}
            #endregion
            if (this.flowMaster == null)
            {
                this.flowMaster = this.smartDeviceService.GetFlowMaster(orderMaster.Flow, false);
            }
            //分装生产单不能直接收货
            //if (orderMaster.OrderStrategy == FlowStrategy.KIT && !string.IsNullOrEmpty(this.flowMaster.Routing))
            //{
            //    throw new BusinessException("分装生产单不可以收货。");
            //}
            ////KIT单不能合并收货
            //if (this.orderMasters.Count > 0)
            //{
            //    if(orderMaster.OrderStrategy == FlowStrategy.KIT )
            //    {
            //        throw new BusinessException("KIT单不能合并收货");
            //    }
            //}
            this.orderMasters.Add(orderMaster);
            this.gvListDataBind();
        }

        protected override Hu DoCancel()
        {
            Hu firstHu = base.DoCancel();
            this.CancelHu(firstHu);
            return firstHu;
        }

        private void CancelHu(Hu hu)
        {
            //if (this.ipMaster == null && (this.orderMasters == null || this.orderMasters.Count() == 0))
            if (this.hus == null)
            {
                //this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
                this.Reset();
                return;
            }

            if (hu != null)
            {
                if (this.ipMaster == null)
                {
                    var orderDetailList = new List<OrderDetail>();
                    foreach (var om in this.orderMasters)
                    {
                        orderDetailList.AddRange(om.OrderDetails);
                    }

                    foreach (var orderDetail in orderDetailList)
                    {
                        if (orderDetail.OrderDetailInputs != null)
                        {
                            var q_pdi = orderDetail.OrderDetailInputs.Where(p => p.HuId == hu.HuId);
                            if (q_pdi != null && q_pdi.Count() > 0)
                            {
                                orderDetail.OrderDetailInputs.ToList().Remove(q_pdi.First());
                                orderDetail.CurrentQty += hu.Qty;
                                orderDetail.RemainReceivedQty += hu.Qty;
                                orderDetail.Carton--;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (this.ipMaster.IpDetails == null || this.ipMaster.IpDetails.Count() == 0)
                    {
                        this.Reset();
                        throw new BusinessException("此ASN无明细");
                    }

                    //need change
                    foreach (var ipDetailInput in this.ipMaster.IpDetailInputs)
                    {
                        if (ipDetailInput.HuId == hu.HuId)
                        {
                            var ipDetail = this.ipMaster.IpDetails.Where(i => i.Id == ipDetailInput.Id).FirstOrDefault();
                            ipDetail.CurrentQty = ipDetail.CurrentQty + hu.Qty;
                            ipDetail.RemainReceivedQty += hu.Qty;
                            ipDetail.Carton--;
                            ipDetailInput.ReceiveQty -= hu.Qty;
                            if (ipDetailInput.IsOriginal == false)
                            {
                                ipDetailInput.HuId = "";
                            }
                        }
                        //if (ipDetail.IpDetailInputs != null)
                        //{
                        //    var q_pdi = ipDetail.IpDetailInputs.Where(p => p.HuId == hu.HuId);
                        //    if (q_pdi != null && q_pdi.Count() > 0)
                        //    {
                        //        ipDetail.IpDetailInputs.ToList().Remove(q_pdi.First());
                        //        ipDetail.CurrentQty += hu.Qty;
                        //        ipDetail.Carton--;
                        //        break;
                        //    }
                        //}
                    }
                }
                base.hus = base.hus.Where(h => !h.HuId.Equals(hu.HuId, StringComparison.OrdinalIgnoreCase)).ToList();
                this.gvHuListDataBind();
            }
        }
    }
}
