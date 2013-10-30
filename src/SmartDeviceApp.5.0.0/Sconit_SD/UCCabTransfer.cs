using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;
using System.Drawing;
using com.Sconit.SmartDevice.CodeMaster;


namespace com.Sconit.SmartDevice
{
    public partial class UCCabTransfer : UserControl
    {
        public event MainForm.ModuleSelectHandler ModuleSelectionEvent;

        private User user;
        private string orderNo;
        private string flow;
        private string cabBarCode;
        private bool isMark;
        private string inputBarCode;
        private string op;

        private SmartDeviceService smartDeviceService;

        private static UCCabTransfer ucCabTransfer;
        private static object obj = new object();

        protected UCCabTransfer(User user)
        {
            InitializeComponent();
            this.smartDeviceService = new SmartDeviceService();
            this.user = user;
            //this.InitializeDataGrid();
            this.Reset();
        }

        public static UCCabTransfer GetUCCabTransfer(User user)
        {
            if (ucCabTransfer == null)
            {
                lock (obj)
                {
                    if (ucCabTransfer == null)
                    {
                        ucCabTransfer = new UCCabTransfer(user);
                    }
                }
            }
            ucCabTransfer.Reset();
            ucCabTransfer.lblMessage.Text = "请扫描移库路线、驾驶室生产单和驾驶室条码";
            return ucCabTransfer;
        }

        private void tbBarCode_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.isMark)
            {
                this.isMark = false;
                //this.tbBarCode.Focus();
                return;
            }
            try
            {
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
                                if (!string.IsNullOrEmpty(cabBarCode) && !string.IsNullOrEmpty(flow)
                                    && !string.IsNullOrEmpty(orderNo))
                                    this.DoSubmit();
                            }
                        }
                    }
                }
                else
                {
                    if ((e.KeyData & Keys.KeyCode) == Keys.Enter)
                    {
                        if (this.tbBarCode.Text.Trim() != string.Empty)
                        {
                            this.ScanBarCode();
                        }
                        else if (!string.IsNullOrEmpty(cabBarCode) && !string.IsNullOrEmpty(flow)
                                    && !string.IsNullOrEmpty(orderNo))
                        {
                            this.DoSubmit();
                        }
                        else
                        {
                            this.tbBarCode.Focus();
                        }
                    }
                    else if (((e.KeyData & Keys.KeyCode) == Keys.Escape))
                    {
                        //取消时依次取消文本框的内容-驾驶室条码-驾驶室生产单-移库路线
                        if (this.tbBarCode.Text.Trim() != string.Empty)
                        {
                            this.tbBarCode.Text = string.Empty;
                        }
                        else if (!string.IsNullOrEmpty(cabBarCode))
                        {
                            this.cabBarCode = null;
                            this.lbItemCodeInfo.Text = string.Empty;
                            this.lbItemDescInfo.Text = string.Empty;
                            this.lbCabBarcodeInfo.Text = string.Empty;
                        }
                        else if (!string.IsNullOrEmpty(orderNo))
                        {
                            this.orderNo = null;
                            this.lblVANInfo.Text = string.Empty;
                            this.lblWoInfo.Text = string.Empty;
                        }
                        else if (!string.IsNullOrEmpty(flow))
                        {
                            this.flow = null;
                            this.lbFlowInfo.Text = string.Empty;
                        }
                    }
                    //else if ((e.KeyData & Keys.KeyCode) == Keys.F4)
                    else if (e.KeyData.ToString() == "199")
                    {
                        this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
                    }

                    //else if ((e.KeyData & Keys.KeyCode) == Keys.F2 || (e.KeyData & Keys.KeyCode) == Keys.F5)
                    //else if (e.KeyData.ToString() == "197")
                    //{
                    //    if (!this.isCancel)
                    //    {
                    //        this.isCancel = true;
                    //        this.lblBarCode.ForeColor = Color.Red;
                    //        this.lblMessage.Text = "取消模式.";
                    //    }
                    //    else
                    //    {
                    //        this.isCancel = false;
                    //        this.lblBarCode.ForeColor = Color.Black;
                    //        this.lblMessage.Text = "正常模式.";
                    //    }
                    //}
                    else if ((e.KeyData & Keys.KeyCode) == Keys.F1)
                    {
                        //MessageBox.Show("没有任何帮助");
                        //todo Help
                    }
                }
            }
            catch (SoapException ex)
            {
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
                //this.tbBarCode.Focus();
                Utility.ShowMessageBox(ex.Message);
            }
            catch (BusinessException ex)
            {
                this.isMark = true;
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
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
                //this.tbBarCode.Focus();
            }
        }

        private void Reset()
        {
            this.orderNo = null;
            this.flow = null;
            this.inputBarCode = null;
            this.cabBarCode = null;
            this.tbBarCode.Text = string.Empty;
            //this.tbBarCode.Focus();
            this.lblMessage.Text = string.Empty;

            //this.isCancel = false;

            this.lblWoInfo.Text = string.Empty;
            this.lblVANInfo.Text = string.Empty;
            this.lbCabBarcodeInfo.Text = string.Empty;
            this.lbFlowInfo.Text = string.Empty;
            this.lbItemCodeInfo.Text = string.Empty;
            this.lbItemDescInfo.Text = string.Empty;
        }

        //private void DoCancel()
        //{
        //    this.Reset();
        //    this.lblMessage.Text = "已全部取消";
        //}

        private void DoSubmit()
        {
            try
            {
                if (string.IsNullOrEmpty(flow))
                {
                    throw new BusinessException("请扫描移库路线。");
                }
                if (string.IsNullOrEmpty(orderNo))
                {
                    throw new BusinessException("请扫描驾驶室生产单。");
                }
                if (string.IsNullOrEmpty(cabBarCode))
                {
                    throw new BusinessException("请扫描驾驶室条码。");
                }
                smartDeviceService.DoTransferCab(orderNo, flow, cabBarCode, user.Code);
                this.Reset();
                this.lblMessage.Text = "驾驶室移库成功";
            }
            catch (Exception ex)
            {
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
                Utility.ShowMessageBox(ex.Message);
            }
        }

        private void ScanBarCode()
        {
            this.inputBarCode = this.tbBarCode.Text.Trim();
            this.lblMessage.Text = string.Empty;
            this.tbBarCode.Text = string.Empty;

            if (this.inputBarCode.Length < 3)
            {
                throw new BusinessException("条码格式不合法");
            }
            this.op = Utility.GetBarCodeType(this.user.BarCodeTypes, this.inputBarCode);


            if (this.op == CodeMaster.BarCodeType.ORD.ToString())
            {
                #region orderMaster
                var orderMaster = this.smartDeviceService.GetOrder(this.inputBarCode, true);
                if (orderMaster.PauseStatus == PauseStatus.Paused)
                {
                    throw new BusinessException("订单已暂停");
                }
                if (orderMaster.Type == OrderType.Production)
                {

                    this.orderNo = orderMaster.OrderNo;
                    this.lblWoInfo.Text = orderMaster.OrderNo;
                    this.lblVANInfo.Text = orderMaster.TraceCode;
                }
                else
                {
                    throw new BusinessException("订单类型不正确:{0}", orderMaster.Type.ToString());
                }
                #endregion
            }
            else if (this.op == CodeMaster.BarCodeType.F.ToString())
            {
                string flowCode = inputBarCode.Substring(2, inputBarCode.Length - 2);
                var flowMaster = smartDeviceService.GetFlowMaster(flowCode, false);
                if (flowMaster.IsActive)
                {
                    this.flow = flowMaster.Code;
                    this.lbFlowInfo.Text = flowMaster.Description;
                }
                else
                    throw new BusinessException("路线:{0}不是可用的路线", flowMaster.Code);
            }
            else
            {
                var hu = smartDeviceService.GetHu(this.inputBarCode);
                this.cabBarCode = hu.HuId;
                this.lbCabBarcodeInfo.Text = hu.HuId;
                this.lbItemCodeInfo.Text = hu.Item;
                this.lbItemDescInfo.Text = hu.ItemDescription;
            }
        }

    }
}
