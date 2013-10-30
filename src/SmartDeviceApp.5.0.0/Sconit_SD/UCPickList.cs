using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;

namespace com.Sconit.SmartDevice
{
    public partial class UCPickList : UCBase
    {
        //public event MainForm.ModuleSelectHandler ModuleSelectionEvent;

        protected PickListMaster pickListMaster;

        public UCPickList(User user)
            : base(user)
        {
            this.InitializeComponent();
            base.btnOrder.Text = "拣货";
        }

        protected override void ScanBarCode()
        {
            base.ScanBarCode();
            if (this.pickListMaster == null)
            {
                if (base.op == CodeMaster.BarCodeType.PIK.ToString())
                {
                    var pickListMaster = smartDeviceService.GetPickList(this.barCode, true);

                    if (pickListMaster.PickListDetails == null || pickListMaster.PickListDetails.Count()==0)
                    {
                        throw new BusinessException("此拣货单没有明细");
                    }
                    pickListMaster.PickListDetails=pickListMaster.PickListDetails.Where(p=>p.IsInventory==true).ToArray();
                    if (pickListMaster.PickListDetails.Count()==0)
                    {
                        throw new BusinessException("此拣货单没有明细");
                    }
                    if (pickListMaster.Status == PickListStatus.Cancel)
                    {
                        throw new BusinessException("此拣货单已经取消,不能拣货");
                    }
                    if (pickListMaster.Status == PickListStatus.Close)
                    {
                        throw new BusinessException("此拣货单已经关闭,不能拣货");
                    }
                    //检查权限
                    if (!Utility.HasPermission(pickListMaster, this.user))
                    {
                        throw new BusinessException("没有此拣货单的权限");
                    }
                    foreach (var pickListDetail in pickListMaster.PickListDetails)
                    {
                        pickListDetail.CurrentQty = pickListDetail.Qty;
                    }
                    this.lblMessage.Text = "请扫描待拣的条码";
                    this.pickListMaster = pickListMaster;
                    this.gvListDataBind();
                }
                else
                {
                    throw new BusinessException("请先扫描拣货单。");
                }
            }
            else
            {
                if (base.op == CodeMaster.BarCodeType.HU.ToString())
                {
                    if (this.pickListMaster == null || this.pickListMaster.PickListDetails == null)
                    {
                        throw new BusinessException("请先扫描拣货单");
                    }
                    Hu hu = smartDeviceService.GetHu(barCode);
                    this.MatchHu(hu);
                }
                else
                {
                    throw new BusinessException("条码格式不合法");
                }
            }
        }

        protected override void gvListDataBind()
        {
            base.gvListDataBind();

            List<PickListDetail> pickListDetails = new List<PickListDetail>();
            if (this.pickListMaster != null && this.pickListMaster.PickListDetails != null)
            {
                pickListDetails = this.pickListMaster.PickListDetails.ToList();
            }
            this.dgList.DataSource = pickListDetails;
            this.ts.MappingName = pickListDetails.GetType().Name;
        }

        protected override void Reset()
        {
            this.pickListMaster = null;
            base.Reset();
            this.lblMessage.Text = "请扫描拣货单";
        }

        protected override void DoSubmit()
        {
            try
            {
                if (this.pickListMaster == null)
                {
                    throw new BusinessException("请先扫描拣货单。");
                }

                List<PickListDetailInput> pickListDetailInputList = new List<PickListDetailInput>();

                if (this.pickListMaster.PickListDetails != null)
                {
                    foreach (var pickListDetail in this.pickListMaster.PickListDetails)
                    {
                        if (pickListDetail.PickListDetailInputs != null)
                        {
                            pickListDetailInputList.AddRange(pickListDetail.PickListDetailInputs);
                        }
                    }
                }
                if (pickListDetailInputList.Count == 0)
                {
                    throw new BusinessException("没有扫描条码");
                }

                this.smartDeviceService.DoPickList(pickListDetailInputList.ToArray(), this.user.Code);

                this.Reset();
                this.lblMessage.Text = "拣货成功"; ;
            }
            catch (Exception ex)
            {
                Utility.ShowMessageBox(ex.Message);
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
                //this.tbBarCode.Focus();
                return;
            } 
 
        }

        private void MatchHu(Hu hu)
        {
            base.CheckHu(hu);

            if (!base.isCancel)
            {
                #region 物料匹配
                var pickListDetails = this.pickListMaster.PickListDetails;

                var pickListDetailList = pickListDetails.Where(o => o.Item == hu.Item);
                if (pickListDetailList == null || pickListDetailList.Count() == 0)
                {
                    throw new BusinessException("没有找到和条码{0}的零件号{1}匹配的订单明细。", hu.HuId, hu.Item);
                }

                pickListDetailList = pickListDetailList.Where(o => o.Uom.Equals(hu.Uom, StringComparison.OrdinalIgnoreCase));
                if (pickListDetailList == null || pickListDetailList.Count() == 0)
                {
                    throw new BusinessException("没有找到和条码{0}的单位{1}匹配的订单明细。", hu.HuId, hu.Uom);
                }

                pickListDetailList = pickListDetailList.Where(o => o.UnitCount == hu.UnitCount);
                if (pickListDetailList == null || pickListDetailList.Count() == 0)
                {
                    throw new BusinessException("没有找到和条码{0}的包装数{1}匹配的订单明细。", hu.HuId, hu.UnitCount.ToString());
                }

                //强制拣货不需要严格限制批次号
                pickListDetailList = pickListDetailList.Where(o => o.LotNo == hu.LotNo);
                if (pickListDetailList == null || pickListDetailList.Count() == 0)
                {
                    throw new BusinessException("没有找到和条码{0}的批号{1}匹配的订单明细。", hu.HuId, hu.LotNo.ToString());
                }

                pickListDetailList = pickListDetailList.Where(o => o.ManufactureParty == hu.ManufactureParty);
                if (pickListDetailList == null || pickListDetailList.Count() == 0)
                {
                    throw new BusinessException("没有找到和条码{0}的供应商{1}匹配的订单明细。", hu.HuId, hu.ManufactureParty.ToString());
                }

                var matchedpickListDetail = pickListDetailList.FirstOrDefault(o => o.Item == hu.Item && o.UnitCount == hu.UnitCount && o.ManufactureParty == hu.ManufactureParty);
                #endregion

                PickListDetailInput input = new PickListDetailInput();
                input.HuId = hu.HuId;
                input.Id = matchedpickListDetail.Id;

                List<PickListDetailInput> pickListDetailInputs = new List<PickListDetailInput>();
                if (matchedpickListDetail.PickListDetailInputs != null)
                {
                    pickListDetailInputs = matchedpickListDetail.PickListDetailInputs.ToList();
                }
                pickListDetailInputs.Add(input);
                matchedpickListDetail.PickListDetailInputs = pickListDetailInputs.ToArray();
                matchedpickListDetail.CurrentQty -= hu.Qty;
                matchedpickListDetail.Carton++;
                this.hus.Insert(0, hu);
            }
            this.gvListDataBind();
        }

        protected override Hu DoCancel()
        {
            Hu firstHu = base.DoCancel();
            this.CancelHu(firstHu);
            return firstHu;
        }

        protected void CancelHu(Hu hu)
        {
            //if (this.pickListMaster == null || this.pickListMaster.PickListDetails == null || this.pickListMaster.PickListDetails.Count() == 0)
            if(this.hus == null)
            {
                //this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
                this.Reset();
                return;
            }
            if (hu != null)
            {
                foreach (var pickListDetail in this.pickListMaster.PickListDetails)
                {
                    if (pickListDetail.PickListDetailInputs != null)
                    {
                        var q_pdi = pickListDetail.PickListDetailInputs.Where(p => p.HuId == hu.HuId);
                        if (q_pdi != null && q_pdi.Count() > 0)
                        {
                            pickListDetail.PickListDetailInputs.ToList().Remove(q_pdi.First());
                            pickListDetail.CurrentQty += hu.Qty;
                            pickListDetail.Carton--;
                            break;
                        }
                    }
                }
                base.hus = base.hus.Where(h => !h.HuId.Equals(hu.HuId, StringComparison.OrdinalIgnoreCase)).ToList();
                this.gvHuListDataBind();
            }
        }
    }
}
