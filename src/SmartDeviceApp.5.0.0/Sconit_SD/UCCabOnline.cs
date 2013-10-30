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

    public partial class UCCabOnline : UserControl
    {
        public event MainForm.ModuleSelectHandler ModuleSelectionEvent;
        private SmartDeviceService smartDeviceService;
        private OrderMaster orderMaster;
        private QualityBarcode qualityBarcode;
        private User user;
        private bool isMark;
        private bool isCancel;
        private string barCode;
        private string op;
        private DateTime? effDate;

        private static UCCabOnline ucCabOnline;
        private static object obj = new object();

        public UCCabOnline(User user)
        {
            this.InitializeComponent();
            this.smartDeviceService = new SmartDeviceService();
            this.user = user;

            this.Reset();
        }

        public static UCCabOnline GetUCCabOnline(User user)
        {
            if (ucCabOnline == null)
            {
                lock (obj)
                {
                    if (ucCabOnline == null)
                    {
                        ucCabOnline = new UCCabOnline(user);
                    }
                }
            }
            return ucCabOnline;
        }


        private void Reset()
        {
            lblMessage.Text = "请扫描驾驶室生产单";
            lblItemInfo.Text = string.Empty;
            lblRefItemInfo.Text = string.Empty;
            lblBarCodeInfo.Text = string.Empty;
            lblFlowInfo.Text = string.Empty;
            lblWindowTimeInfo.Text = string.Empty;
            lblStartTimeInfo.Text = string.Empty;
            lblVANInfo.Text = string.Empty;
            lblWoInfo.Text = string.Empty;
            lblSeqInfo.Text = string.Empty;
            lblItemDescInfo.Text = string.Empty;
            this.orderMaster = null;
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
                            this.btnOrder.Focus();
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
                    else if (e.KeyCode.ToString() == "199")
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

        private void DoCancel()
        {
            this.Reset();
            this.lblMessage.Text = "已全部取消";
        }

        private void DoSubmit()
        {
            try
            {
                smartDeviceService.ScanQualityBarCodeAndStartVanOrder(this.orderMaster.OrderNo, barCode, this.user.Code);
                this.Reset();
                this.lblMessage.Text = "驾驶室上线成功。";
            }
            catch (SoapException ex)
            {
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
                Utility.ShowMessageBox(ex.Message);
            }
            catch (BusinessException ex)
            {
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
                Utility.ShowMessageBox(ex);
            }
            catch (Exception ex)
            {
                Utility.ShowMessageBox(ex.Message);
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
            }
        }

        private void ScanBarCode()
        {
            this.barCode = this.tbBarCode.Text.Trim();
            this.lblMessage.Text = string.Empty;
            this.tbBarCode.Text = string.Empty;

            if (this.barCode.Length < 3)
            {
                throw new BusinessException("条码格式不合法");
            }
            op = Utility.GetBarCodeType(this.user.BarCodeTypes, this.barCode);
            #region ORD
            if (this.op == CodeMaster.BarCodeType.ORD.ToString())
            {
                #region 扫描整车生产单
                orderMaster = this.smartDeviceService.GetOrder(barCode, false);
                if (orderMaster.Type != OrderType.Production
                   || orderMaster.ProdLineType != ProdLineType.Chassis
                   || orderMaster.ProdLineType != ProdLineType.Cab
                   || orderMaster.ProdLineType != ProdLineType.Assembly
                   || orderMaster.ProdLineType != ProdLineType.Special)
                {
                    if (orderMaster.PauseStatus == PauseStatus.Paused)
                    {
                        throw new BusinessException("整车生产单已暂停不能上线");
                    }
                    else if (orderMaster.Status != OrderStatus.Submit)
                    {
                        throw new BusinessException("整车生产单已经上线");
                    }
                    else
                    {
                        if (!Utility.HasPermission(orderMaster, user))
                        {
                            throw new BusinessException("没有驾驶室上线的权限");
                        }
                        this.lblSeqInfo.Text = orderMaster.Sequence.ToString();
                        this.lblFlowInfo.Text = orderMaster.Flow;
                        this.lblWoInfo.Text = orderMaster.OrderNo;
                        this.lblVANInfo.Text = orderMaster.TraceCode;
                        this.lblStartTime.Text = orderMaster.StartTime.ToString();
                        this.lblMessage.Text = " 请扫描关键件。 ";
                    }
                }
                else
                {
                    this.Reset();
                    this.lblMessage.Text = "";
                    throw new BusinessException("扫描的不是整车生产单");
                }
                #endregion
            }
            #endregion

            #region HU
            else if (this.op == CodeMaster.BarCodeType.HU.ToString())
            {

                if (this.orderMaster == null)
                {
                    throw new BusinessException("请先扫描生产单");
                }



                if (this.orderMaster != null)
                {
                    if (this.orderMaster.Type == OrderType.Production)
                    {
                        #region 扫描关键件
                        qualityBarcode = this.smartDeviceService.GetQualityBarCode(barCode);
                        this.lblBarCodeInfo.Text = barCode;
                        this.lblItemDescInfo.Text = qualityBarcode.ItemDescription;
                        this.lblItemInfo.Text = qualityBarcode.Item;
                        this.lblRefItemInfo.Text = qualityBarcode.ReferenceItemCode;
                        this.lblMessage.Text = " 请点击确认。 ";
                        #endregion

                        //this.lblVANInfo.Text = this.orderMaster.TraceCode;
                    }
                }
                else
                {
                    this.Reset();
                    throw new BusinessException("请先扫描生产单");
                }

            }
            #endregion
            else
            {
                throw new BusinessException("条码格式不合法1");
            }
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            if (this.btnOrder == null)
            {
                this.Reset();
                this.lblMessage.Text = "请扫描整车生产单";
                this.lblMessage.ForeColor = Color.Red;
                return;
            }

            this.tbBarCode_KeyUp(sender, null);

        }
    }
}
