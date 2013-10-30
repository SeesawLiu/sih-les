using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;

namespace com.Sconit.SmartDevice
{
    public partial class AssemblyOffline : UserControl
    {
        public event MainForm.ModuleSelectHandler ModuleSelectionEvent;
        private SmartDeviceService smartDeviceService;
        private OrderMaster orderMaster;
        private User user;
        private string barCode;
        private string traceCode;
        private string prodLine;

        private static AssemblyOffline ucAssemblyOffline;
        private static object obj = new object();

        public AssemblyOffline(User user)
        {
            this.InitializeComponent();
            this.smartDeviceService = new SmartDeviceService();
            this.user = user;

            this.Reset();
        }

        public static AssemblyOffline GetAssemblyOffline(User user)
        {
            if (ucAssemblyOffline == null)
            {
                lock (obj)
                {
                    if (ucAssemblyOffline == null)
                    {
                        ucAssemblyOffline = new AssemblyOffline(user);
                    }
                }
            }
            return ucAssemblyOffline;
        }


        private void Reset()
        {
            lblMessage.Text = "请扫描Van号";
            lblFlowInfo.Text = string.Empty;
            lblVANInfo.Text = string.Empty;
            this.orderMaster = null;
            this.traceCode = string.Empty;
            this.prodLine = string.Empty;
            this.barCode = string.Empty;
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
                            if (this.tbBarCode.Text.Trim() != string.Empty)
                            {
                                this.ScanBarCode();
                            }
                            else
                            {
                                this.DoSubmit();
                            }
                        }
                    }
                }
                else
                {
                    if ((e.KeyData & Keys.KeyCode) == Keys.Enter)
                    {
                        if (barCode != string.Empty)
                        {
                            this.ScanBarCode();
                        }
                        else
                        {
                            this.tbBarCode.Focus();
                        }
                    }
                    else if (((e.KeyData & Keys.KeyCode) == Keys.Escape))
                    {
                        if (!string.IsNullOrEmpty(barCode))
                        {
                            this.tbBarCode.Text = string.Empty;
                        }
                        else
                        {
                            this.DoCancel();
                        }
                    }
                    //else if ((e.KeyData & Keys.KeyCode) == Keys.F4)
                    else if (e.KeyData.ToString() == "199")
                    {
                        this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
                    }
                    else if ((e.KeyData & Keys.KeyCode) == Keys.F1)
                    {
                        //todo Help
                    }
                }
            }
            catch (SoapException ex)
            {
                this.tbBarCode.Text = string.Empty;
                //this.tbBarCode.Focus();
                Utility.ShowMessageBox(ex.Message);
            }
            catch (BusinessException ex)
            {
                this.tbBarCode.Text = string.Empty;
                //this.tbBarCode.Focus();
                Utility.ShowMessageBox(ex);
            }
            catch (Exception ex)
            {
                if ((ex is System.Net.WebException) || (ex is SoapException))
                {
                    Utility.ShowMessageBox(ex);
                }
                else if (ex is BusinessException)
                {
                    Utility.ShowMessageBox(ex.Message);
                }
                else
                {
                    this.Reset();
                    Utility.ShowMessageBox(ex.Message);
                }
                this.tbBarCode.Text = string.Empty;
                //this.tbBarCode.Focus();
            }
        }

        private void DoCancel()
        {
            this.Reset();
            this.lblMessage.Text = "已全部取消";
        }

        private void DoSubmit()
        {
            try
            {
                //if (this.orderMaster.Status != OrderStatus.InProcess)
                //    throw new BusinessException("总装生产单状态错误，不能入库");
                //else if (this.orderMaster.PauseStatus != PauseStatus.None)
                //    throw new BusinessException("总装生产单已暂停，不能入库");

                bool isCheckItemTrace = bool.Parse(smartDeviceService.GetEntityPreference(CodeEnum.CheckItemTrace));
                smartDeviceService.ReceiveVanOrderTwo(this.traceCode, false, isCheckItemTrace, this.user.Code, this.prodLine);
                this.Reset();
                this.lblMessage.Text = "整车入库成功。";
            }
            catch (SoapException ex)
            {
                this.lblMessage.Text = "请重新扫描Van号。";
                this.tbBarCode.Text = string.Empty;
                Utility.ShowMessageBox(ex.Message);
            }
            catch (BusinessException ex)
            {
                this.lblMessage.Text = "请重新扫描Van号。";
                this.tbBarCode.Text = string.Empty;
                Utility.ShowMessageBox(ex);
            }
            catch (Exception ex)
            {
                this.lblMessage.Text = "请重新扫描Van号。";
                Utility.ShowMessageBox(ex.Message);
                this.tbBarCode.Text = string.Empty;
            }
            this.traceCode = string.Empty;
        }

        private void ScanBarCode()
        {
            this.barCode = this.tbBarCode.Text.Trim();
            this.lblMessage.Text = string.Empty;
            this.tbBarCode.Text = string.Empty;
            if (string.IsNullOrEmpty(this.traceCode))
            {
                if (string.IsNullOrEmpty(this.barCode))
                {
                    lblMessage.Text = "Van号不能为空，请扫描Van号。";
                }
                else
                {
                    lblMessage.Text = "请扫描生产线。";
                    lblVANInfo.Text = this.barCode;
                    lblFlowInfo.Text = "";
                    this.traceCode = this.barCode;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(this.barCode))
                {
                    lblMessage.Text = "生产线不能为空，请扫描生产线。";
                }
                else
                {
                    this.prodLine = this.barCode;
                    lblFlowInfo.Text = this.barCode;
                    this.DoSubmit();
                }
            
            }

            //if (!string.IsNullOrEmpty(this.barCode))
            //{
            //    orderMaster = this.smartDeviceService.GetOrder(barCode, true);
            //}

            //if (orderMaster.ProdLineType != ProdLineType.Assembly
            //    && orderMaster.ProdLineType != ProdLineType.Cab
            //    && orderMaster.ProdLineType != ProdLineType.Chassis
            //    && orderMaster.ProdLineType != ProdLineType.Check
            //    && orderMaster.ProdLineType != ProdLineType.Special)
            //    throw new BusinessException("此生产单不是整车生产单");
            //else
            //{
            //    this.lblFlowInfo.Text = orderMaster.Flow;
            //    this.lblWoInfo.Text = orderMaster.OrderNo;
            //    this.lblVANInfo.Text = orderMaster.TraceCode;
            //    this.lblItemInfo.Text = orderMaster.OrderDetails[0].Item;
            //    this.lblItemDescInfo.Text = orderMaster.OrderDetails[0].ItemDescription;
            //}
        }

        //private void btnOrder_Click(object sender, EventArgs e)
        //{
        //    if (this.btnOrder == null)
        //    {
        //        this.Reset();
        //        this.lblMessage.Text = "请扫描总装生产单";
        //        this.lblMessage.ForeColor = Color.Red;
        //        return;
        //    }

        //    this.tbBarCode_KeyUp(sender, null);

        //}
    }
}
