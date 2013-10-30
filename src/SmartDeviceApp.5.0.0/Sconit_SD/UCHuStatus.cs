using System;
using System.Windows.Forms;
using System.Drawing;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;

namespace com.Sconit.SmartDevice
{
    public partial class UCHuStatus : UserControl
    {
        public event MainForm.ModuleSelectHandler ModuleSelectionEvent;
        private SmartDeviceService smartDeviceService;
        private User user;
        private Hu hu;
        private bool isMark;
        private int btnIndex;

        public UCHuStatus(User user)
        {
            InitializeComponent();
            this.smartDeviceService = new SmartDeviceService();
            this.user = user;
            this.Reset();
        }

        private void tbBarCode_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.isMark)
            {
                this.isMark = false;
                return;
            }
            try
            {
                if (e == null || (e.KeyData & Keys.KeyCode) == Keys.Enter)
                {
                    this.ScanBarCode();
                }
                else if (((e.KeyData & Keys.KeyCode) == Keys.Escape))
                {
                    if (this.hu != null)
                    {
                        this.Reset();
                    }
                    else
                    {
                        this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
                    }
                }
                else if (e.KeyCode.ToString() == "199")
                {
                    this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
                }
                else if ((e.KeyData & Keys.KeyCode) == Keys.F1)
                {
                    //todo Help
                }
            }
            catch (Exception ex)
            {
                this.isMark = true;
                this.Reset();
                Utility.ShowMessageBox(ex.Message);
            }
        }

        private void ScanBarCode()
        {
            if (this.tbBarCode.Text.Trim() == string.Empty)
            {
                if (this.btnIndex == 1)
                {
                    this.btnMore2_Click(null, null);
                }
                else if (this.btnIndex == 2)
                {
                    this.btnMore3_Click(null, null);
                }
                else if (this.btnIndex == 3)
                {
                    this.btnMore1_Click(null, null);
                }
            }
            else
            {
                this.hu = smartDeviceService.GetHu(this.tbBarCode.Text.Trim());
                this.tbBarCode.Text = string.Empty;
                //this.tbBarCode.Focus();
                this.btnMore1_Click(null, null);
            }
        }

        private void Reset()
        {
            this.hu = null; this.tbBarCode.Text = string.Empty;
            //this.tbBarCode.Focus();
            this.label01.Text = "物料号:";
            this.label02.Text = "旧图号:";
            this.label03.Text = "批号:";
            this.label04.Text = "数量:";
            this.label05.Text = "单位:";
            this.label06.Text = "库位:";
            this.label07.Text = "库格:";
            this.label08.Text = "状态:";
            this.label09.Text = "制造时间:";
            this.label10.Text = "制造厂商:";
            this.lbl01.Text = string.Empty;
            this.lbl02.Text = string.Empty;
            this.lbl03.Text = string.Empty;
            this.lbl04.Text = string.Empty;
            this.lbl05.Text = string.Empty;
            this.lbl06.Text = string.Empty;
            this.lbl07.Text = string.Empty;
            this.lbl08.Text = string.Empty;
            this.lbl09.Text = string.Empty;
            this.lbl10.Text = string.Empty;
            this.lblBarCodeInfo.Text = string.Empty;
            this.lblItemDescInfo.Text = string.Empty;
        }

        private void btnMore1_Click(object sender, EventArgs e)
        {
            this.label01.Text = "物料号:";
            this.label02.Text = "旧图号:";
            this.label03.Text = "批号:";
            this.label04.Text = "数量:";
            this.label05.Text = "单位:";
            this.label06.Text = "库位:";
            this.label07.Text = "库格:";
            this.label08.Text = "状态:";
            this.label09.Text = "制造时间:";
            this.label10.Text = "制造厂商:";
            //
            if (this.hu != null)
            {
                this.lbl01.Text = this.hu.Item;
                this.lbl02.Text = this.hu.ReferenceItemCode;
                this.lbl03.Text = this.hu.LotNo;
                this.lbl04.Text = this.hu.Qty.ToString("0.########");
                this.lbl05.Text = this.hu.Uom;
                this.lbl06.Text = this.hu.Location;
                this.lbl07.Text = this.hu.Bin;
                this.lbl08.Text = this.hu.Status.ToString();
                this.lbl09.Text = this.hu.ManufactureDate.ToString("yyyy-MM-dd HH:mm");
                this.lbl10.Text = this.hu.ManufactureParty;
                this.lblBarCodeInfo.Text = this.hu.HuId;
                this.lblItemDescInfo.Text = this.hu.ItemDescription;
            }
            this.tbBarCode.Text = string.Empty;
            //this.tbBarCode.Focus();
            this.btnIndex = 1;
        }

        private void btnMore2_Click(object sender, EventArgs e)
        {
            this.label01.Text = "库存单位:";
            this.label02.Text = "单包装:";
            this.label03.Text = "转换:";
            this.label04.Text = "过期时间:";
            this.label05.Text = "初次入库:";
            this.label06.Text = "打印次数:";
            this.label07.Text = "质量状态:";
            this.label08.Text = "创建用户:";
            this.label09.Text = "创建时间:";
            this.label10.Text = "区域";
            //
            if (this.hu != null)
            {
                this.lbl01.Text = this.hu.BaseUom;
                this.lbl02.Text = this.hu.UnitCount.ToString("0.########");
                this.lbl03.Text = this.hu.UnitQty.ToString("0.########");
                this.lbl04.Text = this.hu.ExpireDate.HasValue ? this.hu.ExpireDate.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty;
                this.lbl05.Text = this.hu.FirstInventoryDate.HasValue ? this.hu.FirstInventoryDate.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty;
                this.lbl06.Text = this.hu.PrintCount.ToString();
                this.lbl07.Text = this.hu.QualityType.ToString();
                this.lbl08.Text = this.hu.CreateUserName;
                this.lbl09.Text = this.hu.CreateDate.ToString("yyyy-MM-dd HH:mm");
                this.lbl10.Text = this.hu.Region;
                this.lblBarCodeInfo.Text = this.hu.HuId;
                this.lblItemDescInfo.Text = this.hu.ItemDescription;
            }
            this.tbBarCode.Text = string.Empty;
            //this.tbBarCode.Focus();
            this.btnIndex = 2;
        }

        private void btnMore3_Click(object sender, EventArgs e)
        {
            this.label01.Text = "是否冻结:";
            this.label02.Text = "是否寄售:";
            this.label03.Text = "是否ATP:";
            this.label04.Text = "来源库位:";
            this.label05.Text = "目的库位:";
            this.label06.Text = "占用状态:";
            this.label07.Text = "占用订单:";
            this.label08.Text = string.Empty;
            this.label09.Text = string.Empty;
            this.label10.Text = string.Empty;
            //
            if (this.hu != null)
            {
                this.lbl01.Text = this.hu.IsFreeze ? "是" : "否";
                this.lbl02.Text = this.hu.IsConsignment ? "是" : "否";
                this.lbl03.Text = this.hu.IsATP ? "是" : "否";
                this.lbl04.Text = this.hu.LocationFrom;
                this.lbl05.Text = this.hu.LocationTo;
                this.lbl06.Text = this.hu.OccupyType.ToString();
                this.lbl07.Text = this.hu.OccupyReferenceNo;
                this.lbl08.Text = string.Empty;
                this.lbl09.Text = string.Empty;
                this.lbl10.Text = string.Empty;
                this.lblBarCodeInfo.Text = this.hu.HuId;
                this.lblItemDescInfo.Text = this.hu.ItemDescription;
            }
            this.tbBarCode.Text = string.Empty;
            //this.tbBarCode.Focus();
            this.btnIndex = 3;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            this.tbBarCode_KeyUp(null, null);
        }

    }
}
