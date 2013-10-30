using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;

namespace com.Sconit.SmartDevice
{
    public partial class UCPick : UserControl
    {
        private DataGridTableStyle ts;
        private DataGridTextBoxColumn columnPickId;
        private DataGridTextBoxColumn columnOrderNo;
        private DataGridTextBoxColumn columnFlow;
        private DataGridTextBoxColumn columnItemDescription;
        private DataGridTextBoxColumn columnItemCode;
        private DataGridTextBoxColumn columnOrderedQty;
        private DataGridTextBoxColumn columnPickedQty;
        private DataGridTextBoxColumn columnUom;
        private DataGridTextBoxColumn columnUnitCount;
        private DataGridTextBoxColumn columnWindowTime;
        private DataGridTextBoxColumn columnReleaseTime;
        private DataGridTextBoxColumn columnPartyFromName;
        private DataGridTextBoxColumn columnLocFromName;
        private DataGridTextBoxColumn columnPartyToName;
        private DataGridTextBoxColumn columnLocToName;
        private DataGridTextBoxColumn columnManufactureParty;

        public event MainForm.ModuleSelectHandler ModuleSelectionEvent;
        private SmartDeviceService smartDeviceService;
        private User user;

        public UCPick(User user)
        {
            InitializeComponent();
            this.smartDeviceService = new SmartDeviceService();
            this.user = user;
            this.InitializeDataGrid();
            this.Reset();

            dgPickTaskDataBind(false);
        }

        private void dgPickTaskDataBind(bool isReset)
        {
            this.ts = new DataGridTableStyle();
            this.ts.MappingName = new PickTask[] { }.GetType().Name;
            this.ts.GridColumnStyles.Add(this.columnPickId);
            this.ts.GridColumnStyles.Add(this.columnOrderNo);
            this.ts.GridColumnStyles.Add(this.columnFlow);
            this.ts.GridColumnStyles.Add(this.columnItemDescription);
            this.ts.GridColumnStyles.Add(this.columnItemCode);
            this.ts.GridColumnStyles.Add(this.columnOrderedQty);
            this.ts.GridColumnStyles.Add(this.columnPickedQty);
            this.ts.GridColumnStyles.Add(this.columnUom);
            this.ts.GridColumnStyles.Add(this.columnUnitCount);
            this.ts.GridColumnStyles.Add(this.columnWindowTime);
            this.ts.GridColumnStyles.Add(this.columnReleaseTime);
            this.ts.GridColumnStyles.Add(this.columnManufactureParty);

            this.dgPickTask.TableStyles.Clear();
            this.dgPickTask.TableStyles.Add(this.ts);

            if (isReset)
            {
                this.dgPickTask.DataSource = new PickTask[] { };
            }
            else
            {
                PickTask[] pts = this.smartDeviceService.GetPickerTasks(user.Code);
                this.dgPickTask.DataSource = pts;
            }

            this.ResumeLayout();
        }

        private void Reset()
        {
            this.tbBarCode.Text = string.Empty;
            this.lblMessage.Text = string.Empty;
            this.dgPickTaskDataBind(true);
        }

        private void DoSubmit()
        {
            try
            {
                smartDeviceService.Pick(this.tbBarCode.Text.Trim(), this.user.Code);
                this.Reset();
                this.lblMessage.Text = "拣货成功";
                dgPickTaskDataBind(false);

            }
            catch (SoapException ex)
            {
                this.tbBarCode.Text = string.Empty;
                Utility.ShowMessageBox(ex.Message);
            }
            catch (BusinessException ex)
            {
                this.tbBarCode.Text = string.Empty;
                Utility.ShowMessageBox(ex);
            }
            catch (Exception ex)
            {
                Utility.ShowMessageBox(ex.Message);
                this.tbBarCode.Text = string.Empty;
            }
        }

        private void tbBarCode_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                string barCode = this.tbBarCode.Text.Trim();
                if (sender is Button)
                {
                    if (e == null)
                    {
                        this.DoSubmit();
                    }
                    else
                    {
                        if ((e.KeyData & Keys.KeyCode) == Keys.Enter)
                        {
                            this.DoSubmit();
                        }
                    }
                }
                else
                {
                    if ((e.KeyData & Keys.KeyCode) == Keys.Enter)
                    {
                        this.DoSubmit();
                    }
                    else if (((e.KeyData & Keys.KeyCode) == Keys.Escape))
                    {
                        if (!string.IsNullOrEmpty(barCode))
                        {
                            this.tbBarCode.Text = string.Empty;
                        }
                    }
                    else if (e.KeyCode.ToString() == "199")
                    {
                        this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
                    }

                }
            }
            catch (BusinessException ex)
            {
                this.tbBarCode.Text = string.Empty;
                Utility.ShowMessageBox(ex);
            }
            catch (SoapException ex)
            {
                this.tbBarCode.Text = string.Empty;
                Utility.ShowMessageBox(ex.Message);
            }
            catch (Exception ex)
            {
                this.Reset();
                Utility.ShowMessageBox(ex.Message);
            }
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            this.tbBarCode_KeyUp(sender, null);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
        }

        private void InitializeDataGrid()
        {
            this.columnPickId = new DataGridTextBoxColumn();
            this.columnPickId.Format = "";
            this.columnPickId.FormatInfo = null;
            this.columnPickId.HeaderText = "任务ID";
            this.columnPickId.MappingName = "PickId";
            this.columnPickId.Width = 100;

            this.columnOrderNo = new DataGridTextBoxColumn();
            this.columnOrderNo.Format = "";
            this.columnOrderNo.FormatInfo = null;
            this.columnOrderNo.HeaderText = "订单号";
            this.columnOrderNo.MappingName = "OrderNo";
            this.columnOrderNo.Width = 100;

            this.columnFlow = new DataGridTextBoxColumn();
            this.columnFlow.Format = "";
            this.columnFlow.FormatInfo = null;
            this.columnFlow.HeaderText = "路线";
            this.columnFlow.MappingName = "Flow";
            this.columnFlow.Width = 50;

            this.columnItemDescription = new DataGridTextBoxColumn();
            this.columnItemDescription.Format = "";
            this.columnItemDescription.FormatInfo = null;
            this.columnItemDescription.HeaderText = "描述";
            this.columnItemDescription.MappingName = "ItemDesc";
            this.columnItemDescription.Width = 150;

            this.columnItemCode = new DataGridTextBoxColumn();
            this.columnItemCode.Format = "";
            this.columnItemCode.FormatInfo = null;
            this.columnItemCode.HeaderText = "物料";
            this.columnItemCode.MappingName = "Item";
            this.columnItemCode.Width = 100;

            this.columnOrderedQty = new DataGridTextBoxColumn();
            this.columnOrderedQty.Format = "";
            this.columnOrderedQty.FormatInfo = null;
            this.columnOrderedQty.HeaderText = "订单数";
            this.columnOrderedQty.MappingName = "OrderedQty";
            this.columnOrderedQty.NullText = "";
            this.columnOrderedQty.Width = 100;

            this.columnPickedQty = new DataGridTextBoxColumn();
            this.columnPickedQty.Format = "";
            this.columnPickedQty.FormatInfo = null;
            this.columnPickedQty.HeaderText = "已拣数";
            this.columnPickedQty.MappingName = "PickedQty";
            this.columnPickedQty.NullText = "";
            this.columnPickedQty.Width = 100;

            this.columnUom = new DataGridTextBoxColumn();
            this.columnUom.Format = "";
            this.columnUom.FormatInfo = null;
            this.columnUom.HeaderText = "单位";
            this.columnUom.MappingName = "Uom";
            this.columnUom.Width = 40;

            this.columnUnitCount = new DataGridTextBoxColumn();
            this.columnUnitCount.Format = "0.##";
            this.columnUnitCount.FormatInfo = null;
            this.columnUnitCount.HeaderText = "包装";
            this.columnUnitCount.MappingName = "UnitCount";
            this.columnUnitCount.Width = 40;

            this.columnWindowTime = new DataGridTextBoxColumn();
            this.columnWindowTime.Format = "yyyy-MM-dd HH:mm";
            this.columnWindowTime.FormatInfo = null;
            this.columnWindowTime.HeaderText = "窗口时间";
            this.columnWindowTime.MappingName = "WindowTime";
            this.columnWindowTime.Width = 100;

            this.columnReleaseTime = new DataGridTextBoxColumn();
            this.columnReleaseTime.Format = "yyyy-MM-dd HH:mm";
            this.columnReleaseTime.FormatInfo = null;
            this.columnReleaseTime.HeaderText = "释放时间";
            this.columnReleaseTime.MappingName = "ReleaseDate";
            this.columnReleaseTime.Width = 100;

            this.columnManufactureParty = new DataGridTextBoxColumn();
            this.columnManufactureParty.Format = "";
            this.columnManufactureParty.FormatInfo = null;
            this.columnManufactureParty.HeaderText = "制造商";
            this.columnManufactureParty.MappingName = "SupplierName";
            this.columnManufactureParty.NullText = "";
            this.columnManufactureParty.Width = 200;
        }
    }
}
