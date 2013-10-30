using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;

namespace com.Sconit.SmartDevice
{
    public partial class UCAnDon_1 : UCBase
    {
        //public event MainForm.ModuleSelectHandler ModuleSelectionEvent;

        protected DataGridTableStyle ts;

        private DataGridTextBoxColumn columnFlow;
        private DataGridTextBoxColumn columnLocationTo;
        private DataGridTextBoxColumn columnItemCode;
        private DataGridTextBoxColumn columnManufactureParty;
        private DataGridTextBoxColumn columnUnitCount;
        private DataGridTextBoxColumn columnUom;
        private DataGridTextBoxColumn columnSequence;
        private DataGridTextBoxColumn columnNote;

        private List<AnDonInput> anDonInputs;
        private List<string> cardNos;
        private List<FlowMaster> flowMasters;

        public UCAnDon_1(User user)
            : base(user)
        {
            InitializeComponent();
            base.btnOrder.Text = "按灯";
        }

        protected override void ScanBarCode()
        {
            base.ScanBarCode();

            if (base.op == CodeMaster.BarCodeType.K.ToString())
            {
                base.barCode = base.barCode.Substring(2, base.barCode.Length - 2);

                if(!this.cardNos.All(c=>Equals(c,base.barCode)))
                {
                    throw new BusinessException("请不要重复扫描看板卡");
                }

                AnDonInput anDonInput = smartDeviceService.GetKanBanCard(base.barCode);

                anDonInputs.Add(anDonInput);

                //string[] code = barCode.Split('$');
                //string flowCode = code[0];
                //string itemCode = code[1];
                //if (code.Length != 2)
                //{
                //    throw new BusinessException("条码格式不合法");
                //}
                //if (this.flowMasters == null)
                //{
                //    this.flowMasters = new List<FlowMaster>();
                //}

                //Item item = smartDeviceService.GetItem(itemCode);
                //if (!item.IsActive)
                //{
                //    throw new BusinessException("物料已被禁用", itemCode);
                //}

                //FlowMaster flowMaster = new FlowMaster();
                //if (!this.flowMasters.Select(f => f.Code).Contains(flowCode))
                //{
                //    //如果慢的话,就改成false
                //    flowMaster = smartDeviceService.GetFlowMaster(flowCode, true);

                //    //检查订单类型
                //    if (flowMaster.Type != OrderType.Transfer)
                //    {
                //        throw new BusinessException("不是移库路线。");
                //    }

                //    //是否有效
                //    if (!flowMaster.IsActive)
                //    {
                //        throw new BusinessException("此移库路线无效。");
                //    }

                //    //检查权限
                //    if (!Utility.HasPermission(flowMaster, this.user))
                //    {
                //        throw new BusinessException("没有此路线的权限");
                //    }
                //    this.lblMessage.Text = flowMaster.Description;
                //    this.flowMasters.Add(flowMaster);
                //}
                //else
                //{
                //    flowMaster = this.flowMasters.Single(f => f.Code.Equals(flowCode, StringComparison.OrdinalIgnoreCase));
                //}
                //this.MatchItem(flowMaster, item);
            }
            else
            {
                throw new BusinessException("条码格式不合法");
            }
        }

        private void MatchItem(FlowMaster flowMaster, Item item)
        {
            var flowDetails = flowMaster.FlowDetails;

            Hu hu = new Hu();

            hu.HuId = flowMaster.Code;
            hu.CurrentQty = item.UnitCount;
            hu.Item = item.Code;
            hu.Uom = item.Uom;
            hu.ReferenceItemCode = item.ReferenceCode;
            hu.ItemDescription = item.Description; ;
            hu.UnitCount = item.UnitCount;
            hu.Uom = item.Uom;

            if (isCancel)
            {
                this.CancelHu(hu);
            }
            else
            {
                if (flowMaster.IsManualCreateDetail)
                {
                    if (flowDetails != null)
                    {
                        var q = flowDetails.Where(f => f.Item.Equals(item.Code, StringComparison.OrdinalIgnoreCase));

                        if (q.Count() > 0)
                        {
                            var firstFlowDetail = q.First();
                            firstFlowDetail.CurrentQty += item.UnitCount;
                            firstFlowDetail.Carton++;
                        }
                        else
                        {
                            List<FlowDetail> flowDetailList = flowDetails.ToList();
                            flowDetailList.Add(Item2FlowDetail(flowMaster, item));
                            flowMaster.FlowDetails = flowDetailList.ToArray();
                        }
                    }
                    else
                    {
                        List<FlowDetail> flowDetailList = new List<FlowDetail>();
                        flowDetailList.Add(Item2FlowDetail(flowMaster, item));
                        flowMaster.FlowDetails = flowDetailList.ToArray();
                    }

                }
                else
                {
                    #region 物料匹配
                    if (flowDetails == null)
                    {
                        throw new BusinessException("没有找到和路线{0}的零件号{1}匹配的明细。", flowMaster.Code, item.Code);
                    }

                    var matchedOrderDetailList = flowDetails.Where(o => o.Item == item.Code);
                    if (matchedOrderDetailList == null || matchedOrderDetailList.Count() == 0)
                    {
                        throw new BusinessException("没有找到和路线{0}的零件号{1}匹配的明细。", flowMaster.Code, item.Code);
                    }

                    matchedOrderDetailList = matchedOrderDetailList.Where(o => o.Uom.Equals(item.Uom, StringComparison.OrdinalIgnoreCase));
                    if (matchedOrderDetailList == null || matchedOrderDetailList.Count() == 0)
                    {
                        throw new BusinessException("没有找到和路线{0}的单位{1}匹配的订单明细。", flowMaster.Code, item.Code);
                    }

                    if (flowMaster.IsOrderFulfillUC)
                    {
                        matchedOrderDetailList = matchedOrderDetailList.Where(o => o.UnitCount == item.UnitCount);
                        if (matchedOrderDetailList == null || matchedOrderDetailList.Count() == 0)
                        {
                            throw new BusinessException("没有找到和路线{0}的包装数{1}匹配的订单明细。", flowMaster.Code, item.UnitCount.ToString());
                        }
                    }
                    FlowDetail matchedFlowDetail = matchedOrderDetailList.First();
                    #endregion

                    matchedFlowDetail.CurrentQty += item.UnitCount;
                    matchedFlowDetail.Carton++;

                }

                if (this.hus == null)
                {
                    this.hus = new List<Hu>();
                }

                this.hus.Insert(0, hu);
            }
            //
            this.gvListDataBind();
        }

        private FlowDetail Item2FlowDetail(FlowMaster flowMaster, Item item)
        {
            int seq = 10;
            if (flowMaster.FlowDetails != null)
            {
                seq = flowMaster.FlowDetails.Max(f => f.Sequence) + 10;
            }

            FlowDetail flowDetail = new FlowDetail();
            flowDetail.CurrentQty = item.UnitCount;
            flowDetail.Flow = flowMaster.Code;
            flowDetail.Item = item.Code;
            flowDetail.LocationFrom = flowMaster.LocationFrom;
            flowDetail.LocationTo = flowMaster.LocationTo;
            flowDetail.ReferenceItemCode = item.ReferenceCode;
            flowDetail.Sequence = seq;
            flowDetail.UnitCount = item.UnitCount;
            flowDetail.Uom = item.Uom;

            return flowDetail;
        }

        protected override void gvListDataBind()
        {
            base.columnIsOdd.Width = 0;
            base.columnLotNo.Width = 0;
            base.gvListDataBind();

            List<FlowDetail> flowDetails = new List<FlowDetail>();
            foreach (var flowMaster in this.flowMasters)
            {
                if (flowMaster != null && flowMaster.FlowDetails != null)
                {
                    flowDetails.AddRange(flowMaster.FlowDetails.Where(f => f.CurrentQty > 0));
                }
            }

            base.dgList.DataSource = flowDetails;
            base.ts.MappingName = flowDetails.GetType().Name;
        }

        protected override void gvHuListDataBind()
        {
            base.columnIsOdd.Width = 0;
            base.columnLotNo.Width = 0;
            this.columnHuId.HeaderText = "路线";
            this.columnHuId.Width = 80;
            base.gvHuListDataBind();
        }

        protected override void Reset()
        {
            this.anDonInputs = new List<AnDonInput>();
            this.cardNos = new List<string>();
            //this.flowMasters = new List<FlowMaster>();
            base.Reset();
            this.lblMessage.Text = "正常模式,请扫描按灯条码";
        }

        protected override void DoSubmit()
        {
            List<OrderDetailInput> orderDetailInputList = new List<OrderDetailInput>();

            foreach (var flowMaster in this.flowMasters)
            {
                //this.smartDeviceService.DoAnDon(flowMaster, this.user.Code);
            }
            this.Reset();
            this.lblMessage.Text = "按灯成功";
        }

        protected override Hu DoCancel()
        {
            Hu firstHu = base.DoCancel();
            this.CancelHu(firstHu);
            return firstHu;
        }

        private void CancelHu(Hu hu)
        {
            if (this.flowMasters == null || this.flowMasters.Count == 0)
            {
                //this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
                this.Reset();
            }
            if (hu != null)
            {
                var q = this.flowMasters.Single(f => f.Code == hu.HuId).FlowDetails.Where(f => f.Item.Equals(hu.Item, StringComparison.OrdinalIgnoreCase));

                if (q.Count() > 0)
                {
                    var firstFlowDetail = q.First();
                    if (firstFlowDetail.CurrentQty >= hu.UnitCount)
                    {
                        firstFlowDetail.CurrentQty -= hu.UnitCount;
                        firstFlowDetail.Carton--;
                        Hu cancelHu = base.hus.Last(h => h.Item.Equals(hu.Item, StringComparison.OrdinalIgnoreCase));
                        base.hus.Remove(cancelHu);
                        this.gvHuListDataBind();
                    }
                    else
                    {
                        throw new BusinessException("没有可取消的物料{0}", hu.Item);
                    }
                }
                else
                {
                    throw new BusinessException("没有可取消的物料{0}", hu.Item);
                }
            }
        }

    }
}
