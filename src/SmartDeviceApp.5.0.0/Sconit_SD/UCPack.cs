using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;

namespace com.Sconit.SmartDevice
{
    public partial class UCPack : UCBase
    {
        //public event MainForm.ModuleSelectHandler ModuleSelectionEvent;

        private List<GroupHu> groupHus;
        private Location location;
        protected DateTime? effDate;

        public UCPack()
        {
        }

        public UCPack(User user)
            : base(user)
        {
            InitializeComponent();
            base.btnOrder.Text = "装箱";
        }

        protected override void ScanBarCode()
        {
            base.ScanBarCode();

            if (this.location == null)
            {
                if (base.op == CodeMaster.BarCodeType.L.ToString())
                {
                    base.barCode = base.barCode.Substring(2, base.barCode.Length - 2);
                    var location = smartDeviceService.GetLocation(base.barCode);
                    //检查权限
                    if (!Utility.HasPermission(user.Permissions, null, false, true, null, location.Region))
                    {
                        throw new BusinessException("没有此库位的权限");
                    }
                    this.location = location;
                    this.lblMessage.Text = "当前库位:" + this.location.Code;
                }
                else
                {
                    throw new BusinessException("请先扫描库位");
                }
            }
            else
            {
                if (base.op == CodeMaster.BarCodeType.HU.ToString())
                {
                    Hu hu = smartDeviceService.GetHu(barCode);
                    if (hu.Status == HuStatus.Location)
                    {
                        throw new BusinessException("条码已在库位中不能装箱");
                    }

                    if (hu.Status == HuStatus.Ip)
                    {
                        throw new BusinessException("条码已在途不能装箱");
                    }
                    this.MatchHu(hu);
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

        protected void MatchHu(Hu hu)
        {
            base.CheckHu(hu);

            if (!base.isCancel)
            {
                GroupHu groupHu = new GroupHu();

                if (groupHus == null)
                {
                    groupHus = new List<GroupHu>();
                }

                var q = groupHus
                    .Where(g => g.Item.Equals(hu.Item, StringComparison.OrdinalIgnoreCase)
                        && g.Uom.Equals(hu.Uom, StringComparison.OrdinalIgnoreCase)
                        && g.UnitCount == hu.UnitCount);
                if (q.Count() > 0)
                {
                    groupHu = q.Single();
                }
                else
                {
                    groupHu.Item = hu.Item;
                    groupHu.ItemDescription = hu.ItemDescription;
                    groupHu.ReferenceItemCode = hu.ReferenceItemCode;
                    groupHu.UnitCount = hu.UnitCount;
                    groupHu.Uom = hu.Uom;

                    var flowDetailList = new List<FlowDetail>();
                    groupHus.Add(groupHu);
                }

                groupHu.CurrentQty += hu.Qty;
                groupHu.Carton++;
                //
                base.hus.Insert(0, hu);
            }
            else
            {
                this.CancelHu(hu);
            }
            this.gvListDataBind();
        }

        protected override void gvListDataBind()
        {
            base.columnIsOdd.Width = 0;
            base.columnLotNo.Width = 0;
            base.gvListDataBind();

            base.dgList.DataSource = groupHus.Where(g => g.CurrentQty > 0).ToList();
            base.ts.MappingName = groupHus.GetType().Name;
        }

        protected override void gvHuListDataBind()
        {
            base.gvHuListDataBind();
        }

        protected override void Reset()
        {
            this.groupHus = new List<GroupHu>();
            base.Reset();
            this.effDate = null;
            this.lblMessage.Text = "正常模式,请扫描库位条码";
        }

        protected override void DoSubmit()
        {
            try
            {
                if (base.hus == null || base.hus.Count == 0)
                {
                    throw new BusinessException("没有装新明细");
                }
                base.smartDeviceService.DoPack(base.hus.Select(h => h.HuId).ToArray(), this.location.Code, this.effDate, this.user.Code);
                this.Reset();
                this.lblMessage.Text = "装箱成功";
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
            if (this.groupHus == null || this.groupHus.Count == 0)
            {
                //this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
                this.Reset();
            }
            if (hu != null)
            {
                var q = this.groupHus
                    .Where(g => g.Item == hu.Item
                        && g.Uom == hu.Uom
                        && g.UnitCount == hu.UnitCount);

                if (q.Count() > 0)
                {
                    var firstFlowDetail = q.First();
                    if (firstFlowDetail.CurrentQty >= hu.UnitCount)
                    {
                        firstFlowDetail.CurrentQty -= hu.UnitCount;
                        firstFlowDetail.Carton--;
                        Hu cancelHu = base.hus.FirstOrDefault(h => h.HuId == hu.HuId);
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
