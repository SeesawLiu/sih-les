using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;

namespace com.Sconit.SmartDevice
{
    public partial class UCTransfer : UCBase
    {
        //public event MainForm.ModuleSelectHandler ModuleSelectionEvent;

        private FlowMaster flowMaster;
        private DateTime? effDate;

        private static UCTransfer ucTransfer;
        private static object obj = new object();

        private UCTransfer(User user)
            : base(user)
        {
            this.InitializeComponent();
            base.btnOrder.Text = "收货";
        }

        public static UCTransfer GetUCTransfer(User user)
        {
            if (ucTransfer == null)
            {
                lock (obj)
                {
                    if (ucTransfer == null)
                    {
                        ucTransfer = new UCTransfer(user);
                    }
                }
            }
            ucTransfer.Reset();
            ucTransfer.lblMessage.Text = "请先扫描路线或库位或库格";
            return ucTransfer;
        }


        protected override void ScanBarCode()
        {
            base.ScanBarCode();

            if (this.flowMaster == null)
            {
                if (base.op == CodeMaster.BarCodeType.F.ToString())
                {
                    base.barCode = base.barCode.Substring(2, base.barCode.Length - 2);
                    var flowMaster = smartDeviceService.GetFlowMaster(base.barCode, true);

                    //检查订单类型
                    if (flowMaster.Type != OrderType.Transfer)
                    {
                        throw new BusinessException("不是移库路线。");
                    }

                    //是否有效
                    if (!flowMaster.IsActive)
                    {
                        throw new BusinessException("此移库路线无效。");
                    }

                    //检查权限
                    if (!Utility.HasPermission(flowMaster, base.user))
                    {
                        throw new BusinessException("没有此移库路线的权限");
                    }
                    this.flowMaster = flowMaster;
                    base.lblMessage.Text = this.flowMaster.Description;
                    this.gvListDataBind();
                }
                else if (base.op == CodeMaster.BarCodeType.B.ToString())
                {
                    base.barCode = base.barCode.Substring(2, base.barCode.Length - 2);
                    Bin bin = smartDeviceService.GetBin(base.barCode);
                    //检查权限
                    if (!Utility.HasPermission(user.Permissions, OrderType.Transfer, false, true, null, bin.Region))
                    {
                        throw new BusinessException("没有此移库路线的权限");
                    }
                    this.flowMaster = new FlowMaster();
                    this.flowMaster.PartyTo = bin.Region;
                    this.flowMaster.LocationTo = bin.Location;
                    this.flowMaster.Bin = bin.Code;

                    this.lblMessage.Text = "目的库格:" + bin.Code;
                }
                else if (base.op == CodeMaster.BarCodeType.L.ToString())
                {
                    base.barCode = base.barCode.Substring(2, base.barCode.Length - 2);
                    Location location = smartDeviceService.GetLocation(base.barCode);

                    //检查权限
                    if (!Utility.HasPermission(user.Permissions, OrderType.Transfer, false, true, null, location.Region))
                    {
                        throw new BusinessException("没有此移库路线的权限");
                    }
                    this.flowMaster = new FlowMaster();
                    this.flowMaster.PartyTo = location.Region;
                    this.flowMaster.LocationTo = location.Code;
                    this.lblMessage.Text = "目的库位:" + location.Code;
                }
                else
                {
                    throw new BusinessException("输入的库位或路线条码不正确。");
                }
            }
            else
            {
                if (base.op == CodeMaster.BarCodeType.HU.ToString())
                {
                    if (this.flowMaster == null)
                    {
                        throw new BusinessException("请先扫描路线或库位或库格");
                    }
                    Hu hu = smartDeviceService.GetHu(barCode);
                    this.MatchHu(hu);
                }
                else if (base.op == CodeMaster.BarCodeType.DATE.ToString())
                {
                    //todo 权限校验
                    base.barCode = base.barCode.Substring(2, base.barCode.Length - 2);
                    this.effDate = base.smartDeviceService.GetEffDate(base.barCode);

                    this.lblMessage.Text = "生效时间:" + this.effDate.Value.ToString("yyyy-MM-dd HH:mm");
                    this.tbBarCode.Text = string.Empty;
                    //this.tbBarCode.Focus();
                    this.flowMaster.EffectiveDate = effDate;
                }
                else
                {
                    throw new BusinessException("条码格式不合法");
                }
            }
        }

        protected override void gvListDataBind()
        {
            base.columnIsOdd.Width = 0;
            base.columnLotNo.Width = 0;
            base.gvListDataBind();
            List<FlowDetail> flowDetails = new List<FlowDetail>();
            if (this.flowMaster != null && this.flowMaster.FlowDetails != null)
            {
                flowDetails = this.flowMaster.FlowDetails.Where(f => f.CurrentQty > 0).ToList();
            }
            base.dgList.DataSource = flowDetails;
            base.ts.MappingName = flowDetails.GetType().Name;
        }


        protected override void Reset()
        {
            this.flowMaster = null;
            base.Reset();
            //this.lblMessage.Text = "请先扫描路线或库位或库格";
            this.effDate = null;
        }

        protected override void DoSubmit()
        {
            try
            {
                if (this.flowMaster == null)
                {
                    throw new BusinessException("请先扫描路线，库位或者库格。");
                }
                List<OrderDetailInput> orderDetailInputList = new List<OrderDetailInput>();

                if (this.flowMaster.FlowDetails != null)
                {
                    foreach (var flowDetail in this.flowMaster.FlowDetails)
                    {
                        if (flowDetail.FlowDetailInputs != null)
                        {
                            foreach (var fdi in flowDetail.FlowDetailInputs)
                            {
                                OrderDetailInput orderDetailInput = new OrderDetailInput();
                                orderDetailInput.HuId = fdi.HuId;
                                orderDetailInput.Qty = fdi.Qty;
                                orderDetailInput.LotNo = fdi.LotNo;
                                orderDetailInput.Id = flowDetail.Id;

                                orderDetailInputList.Add(orderDetailInput);
                            }
                        }
                    }
                }
                this.smartDeviceService.DoTransfer(flowMaster, orderDetailInputList.ToArray(), base.user.Code);
                this.Reset();
                base.lblMessage.Text = "移库成功";
            }
            catch (Exception ex)
            {
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
                //this.tbBarCode.Focus();
                Utility.ShowMessageBox(ex.Message);
            }
        }

        private void MatchHu(Hu hu)
        {
            base.CheckHu(hu);

            if (!base.isCancel)
            {
                //if (hu.Status != HuStatus.Location)
                //{
                //    throw new BusinessException("条码不在库位中");
                //}
                //新的条码逻辑
                if (this.flowMaster.IsShipScanHu && hu.Status != HuStatus.Location)
                {
                    throw new BusinessException("条码不在库位中");
                }

                if (!this.flowMaster.IsShipScanHu && this.flowMaster.IsReceiveScanHu)
                {
                    throw new BusinessException("快速移库不支持数量发货条码收货的配置");
                }

                if (hu.QualityType == QualityType.Inspect)
                {
                    throw new BusinessException("待验条码不能移库");
                }

                if (hu.QualityType == QualityType.Reject)
                {
                    throw new BusinessException("不合格条码不能移库");
                }

                FlowDetail matchedFlowDetail = new FlowDetail();
                var flowDetails = this.flowMaster.FlowDetails;

                if (string.IsNullOrEmpty(this.flowMaster.Code))
                {
                    if (flowDetails == null)
                    {
                        flowDetails = new List<FlowDetail>().ToArray();
                        this.flowMaster.LocationFrom = hu.Location;
                        this.flowMaster.PartyFrom = hu.Region;
                    }
                    else
                    {
                        if (!this.flowMaster.LocationFrom.Equals(hu.Location, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new BusinessException("当前条码{0}的来源库位{1}与其他物料的来源库位{2}不一致。", hu.HuId, hu.Location, this.flowMaster.LocationFrom);
                        }
                    }

                    var q = flowDetails
                        .Where(f => f.Item.Equals(hu.Item, StringComparison.OrdinalIgnoreCase)
                            && f.Uom.Equals(hu.Uom, StringComparison.OrdinalIgnoreCase)
                            && f.UnitCount == hu.UnitCount);
                    if (q.Count() > 0)
                    {
                        matchedFlowDetail = q.Single();
                    }
                    else
                    {
                        //检查权限
                        if (!Utility.HasPermission(user.Permissions, OrderType.Transfer, false, true, null, hu.Region))
                        {
                            throw new BusinessException("没有此移库路线的权限");
                        }
                        matchedFlowDetail.Id = FindMinId() - 1;
                        matchedFlowDetail.Item = hu.Item;
                        //matchedFlowDetail.Sequence++;
                        matchedFlowDetail.ReferenceItemCode = hu.ReferenceItemCode;
                        matchedFlowDetail.UnitCount = hu.UnitCount;
                        matchedFlowDetail.Uom = hu.Uom;

                        var flowDetailList = new List<FlowDetail>();
                        if (this.flowMaster.FlowDetails != null)
                        {
                            flowDetailList = this.flowMaster.FlowDetails.ToList();
                        }
                        flowDetailList.Add(matchedFlowDetail);
                        this.flowMaster.FlowDetails = flowDetailList.ToArray();
                    }
                }
                else
                {
                    #region 物料匹配

                    if (flowMaster.IsManualCreateDetail)
                    {
                        if (flowDetails != null)
                        {
                            var q = flowDetails.Where(f => f.Item.Equals(hu.Item, StringComparison.OrdinalIgnoreCase));
                            if (q.Count() > 0)
                            {
                                matchedFlowDetail = q.First();
                                //matchedFlowDetail.CurrentQty += hu.Qty;
                                //matchedFlowDetail.Carton++;
                            }
                            else
                            {
                                List<FlowDetail> flowDetailList = flowDetails.ToList();
                                if (flowMaster.FlowDetails != null)
                                {
                                    flowDetailList = flowMaster.FlowDetails.ToList();
                                }
                                matchedFlowDetail = this.Hu2FlowDetail(flowMaster, hu);
                                flowDetailList.Add(matchedFlowDetail);
                                flowMaster.FlowDetails = flowDetailList.ToArray();
                            }
                        }
                        else
                        {
                            List<FlowDetail> flowDetailList = new List<FlowDetail>();
                            if (flowMaster.FlowDetails != null)
                            {
                                flowDetailList = flowMaster.FlowDetails.ToList();
                            }
                            matchedFlowDetail = this.Hu2FlowDetail(flowMaster, hu);
                            flowDetailList.Add(matchedFlowDetail);
                            flowMaster.FlowDetails = flowDetailList.ToArray();
                        }
                    }
                    else
                    {
                        if (this.flowMaster.FlowDetails == null)
                        {
                            throw new BusinessException("没有找到和条码{0}的零件号{1}匹配的订单明细。", hu.HuId, hu.Item);
                        }

                        var matchedOrderDetailList = flowDetails.Where(o => o.Item == hu.Item);
                        if (matchedOrderDetailList == null || matchedOrderDetailList.Count() == 0)
                        {
                            throw new BusinessException("没有找到和条码{0}的零件号{1}匹配的订单明细。", hu.HuId, hu.Item);
                        }

                        matchedOrderDetailList = matchedOrderDetailList.Where(o => o.Uom.Equals(hu.Uom, StringComparison.OrdinalIgnoreCase));
                        if (matchedOrderDetailList == null || matchedOrderDetailList.Count() == 0)
                        {
                            throw new BusinessException("没有找到和条码{0}的单位{1}匹配的订单明细。", hu.HuId, hu.Uom);
                        }

                        if (this.flowMaster.IsOrderFulfillUC)
                        {
                            matchedOrderDetailList = matchedOrderDetailList.Where(o => o.UnitCount == hu.UnitCount);
                            if (matchedOrderDetailList == null || matchedOrderDetailList.Count() == 0)
                            {
                                throw new BusinessException("没有找到和条码{0}的包装数{1}匹配的订单明细。", hu.HuId, hu.UnitCount.ToString());
                            }
                        }
                        matchedFlowDetail = matchedOrderDetailList.First();
                    }
                    #endregion
                }

                FlowDetailInput input = new FlowDetailInput();
                input.HuId = hu.HuId;
                input.Qty = hu.Qty;
                input.LotNo = hu.LotNo;

                List<FlowDetailInput> flowDetailInputs = new List<FlowDetailInput>();
                if (matchedFlowDetail.FlowDetailInputs != null)
                {
                    flowDetailInputs = matchedFlowDetail.FlowDetailInputs.ToList();
                }
                flowDetailInputs.Add(input);
                matchedFlowDetail.FlowDetailInputs = flowDetailInputs.ToArray();
                matchedFlowDetail.CurrentQty += hu.Qty;
                matchedFlowDetail.Carton++;
                //
                base.hus.Insert(0, hu);
            }
            else
            {
                this.CancelHu(hu);
            }
            this.gvListDataBind();
        }


        private FlowDetail Hu2FlowDetail(FlowMaster flowMaster, Hu hu)
        {
            //int seq = 10;
            //if (flowMaster.FlowDetails != null)
            //{
            //    seq = flowMaster.FlowDetails.Max(f => f.Sequence) + 10;
            //}

            FlowDetail flowDetail = new FlowDetail();
            flowDetail.Id = this.FindMinId() - 1;
            //flowDetail.CurrentQty = hu.UnitCount;
            flowDetail.Flow = flowMaster.Code;
            flowDetail.Item = hu.Item;
            flowDetail.LocationFrom = flowMaster.LocationFrom;
            flowDetail.LocationTo = flowMaster.LocationTo;
            flowDetail.ReferenceItemCode = hu.ReferenceItemCode;
            //flowDetail.Sequence = seq;
            flowDetail.UnitCount = hu.UnitCount;
            flowDetail.Uom = hu.Uom;

            return flowDetail;
        }

        protected override Hu DoCancel()
        {
            Hu firstHu = base.DoCancel();
            this.CancelHu(firstHu);
            return firstHu;
        }

        protected void CancelHu(Hu hu)
        {
            //if (this.flowMaster == null || this.flowMaster.FlowDetails == null || this.flowMaster.FlowDetails.Count() == 0)
            if(this.hus == null)
            {
                //this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
                this.Reset();
                return;
            }

            if (hu != null)
            {
                foreach (var flowDetail in this.flowMaster.FlowDetails)
                {
                    if (flowDetail.FlowDetailInputs != null)
                    {
                        var q_pdi = flowDetail.FlowDetailInputs.Where(p => p.HuId == hu.HuId);
                        if (q_pdi != null && q_pdi.Count() > 0)
                        {
                            flowDetail.FlowDetailInputs.ToList().Remove(q_pdi.First());
                            flowDetail.CurrentQty -= hu.Qty;
                            flowDetail.Carton--;
                            break;
                        }
                    }
                }
                base.hus = base.hus.Where(h => !h.HuId.Equals(hu.HuId, StringComparison.OrdinalIgnoreCase)).ToList();
                this.gvHuListDataBind();
            }
        }

        private int FindMinId()
        {
            if (this.flowMaster.FlowDetails != null && this.flowMaster.FlowDetails.Length > 0)
            {
                return this.flowMaster.FlowDetails.Min(f => f.Id);
            }
            return 0;
        }
    }
}
