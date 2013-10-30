using System;
using System.Drawing;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using com.Sconit.SmartDevice.SmartDeviceRef;

namespace com.Sconit.SmartDevice
{
    public partial class UCProductOrderOnline : UserControl
    {
        SmartDeviceService smartDeviceService;
        public event MainForm.ModuleSelectHandler ModuleSelectionEvent;
        private User user;
        private string vanOrderNo;
        private bool isMark;

        private static UCProductOrderOnline ucProductOrderOnline;
        private static object obj = new object();

        private UCProductOrderOnline(User user)
        {
            InitializeComponent();
            this.smartDeviceService = new SmartDeviceService();
            this.user = user;
            this.Reset();
        }

        public static UCProductOrderOnline GetUCProductOrderOnline(User user)
        {
            if (ucProductOrderOnline == null)
            {
                lock (obj)
                {
                    if (ucProductOrderOnline == null)
                    {
                        ucProductOrderOnline = new UCProductOrderOnline(user);
                    }
                }
            }

            return ucProductOrderOnline;
        }

        private void btnBack_Click(object sender, System.EventArgs e)
        {
            this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
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
                        if (this.tbBarCode.Text.Trim() != string.Empty)
                        {
                            this.ScanBarCode();
                        }
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
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
                Utility.ShowMessageBox(ex);
            }
            catch (SoapException ex)
            {
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
                Utility.ShowMessageBox(ex.Message);
            }
            catch (Exception ex)
            {
                this.isMark = true;
                this.Reset();
                Utility.ShowMessageBox(ex.Message);
            }
        }

        private void DoSubmit()
        {
            try
            {
                smartDeviceService.StartVanOrder(this.vanOrderNo, this.user.Code);
                this.Reset();
                this.lblMessage.Text = "整车生产单上线成功";
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
            string barCode = this.tbBarCode.Text.Trim();
            this.tbBarCode.Text = string.Empty;
            string op = Utility.GetBarCodeType(this.user.BarCodeTypes, barCode);
            if (barCode.Length < 3)
            {
                throw new BusinessException("条码格式不合法");
            }

            if (op == CodeMaster.BarCodeType.ORD.ToString())
            {
                var vanOrder = this.smartDeviceService.GetOrder(barCode, false);

                if (vanOrder.Type != OrderType.Production
                    || vanOrder.ProdLineType != ProdLineType.Chassis
                    || vanOrder.ProdLineType != ProdLineType.Cab
                    || vanOrder.ProdLineType != ProdLineType.Assembly
                    || vanOrder.ProdLineType != ProdLineType.Special)
                {
                    if (vanOrder.PauseStatus == PauseStatus.Paused)
                    {
                        throw new BusinessException("整车生产单已暂停不能上线");
                    }
                    else if (vanOrder.Status != OrderStatus.Submit)
                    {
                        throw new BusinessException("整车生产单已经上线");
                    }
                    else
                    {
                        if (!Utility.HasPermission(vanOrder, user))
                        {
                            throw new BusinessException("没有整车生产单上线的权限");
                        }

                        this.lblMessage.Text = "请按确定键确认上线";

                        this.lblStartTimeInfo.Text = vanOrder.StartTime.ToString("yyyy-MM-dd");
                        this.lblFlowInfo.Text = vanOrder.Flow;
                        this.lblVANInfo.Text = vanOrder.TraceCode;
                        this.lblWoInfo.Text = vanOrder.OrderNo;
                        this.vanOrderNo = vanOrder.OrderNo;
                    }
                }
                else
                {
                    this.Reset();
                    this.lblMessage.Text = "";
                    throw new BusinessException("扫描的不是整车生产单");
                }
            }
            else
            {
                this.Reset();
                this.lblMessage.Text = "";
                throw new BusinessException("扫描的不是整车生产单");
            }
        }

        private void Reset()
        {
            this.vanOrderNo = null;
            this.lblFlowInfo.Text = string.Empty;
            this.lblStartTimeInfo.Text = string.Empty;
            this.lblVANInfo.Text = string.Empty;
            this.lblWoInfo.Text = string.Empty;
            this.tbBarCode.Text = string.Empty;
            this.lblMessage.Text = string.Empty;
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            if (this.vanOrderNo == null)
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
