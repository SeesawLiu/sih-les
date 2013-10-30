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
    public partial class UCScanForceMaterialIn : UserControl
    {
        public event MainForm.ModuleSelectHandler ModuleSelectionEvent;

        protected DataGridTableStyle ts;
        protected DataGridTextBoxColumn columnHuId;

        private SmartDeviceService smartDeviceService;
        private User user;
        private OrderMaster orderMaster;
        private List<string> huIds;
        private bool isMark;
        private bool isCancel;
        private string barCode;
        private string op;
        private string traceCode;

        private static UCScanForceMaterialIn usScanForceMaterialIn;
        private static object obj = new object();

        protected UCScanForceMaterialIn(User user)
        {
            InitializeComponent();
            this.smartDeviceService = new SmartDeviceService();
            this.user = user;
            this.InitializeDataGrid();
            this.Reset();
        }

        public static UCScanForceMaterialIn UCScanForceMaterialIns(User user)
        {
            if (usScanForceMaterialIn == null)
            {
                lock (obj)
                {
                    if (usScanForceMaterialIn == null)
                    {
                        usScanForceMaterialIn = new UCScanForceMaterialIn(user);
                    }
                }
            }

            usScanForceMaterialIn.lblMessage.Text = "请扫描Van号";
            return usScanForceMaterialIn;
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

                    //else if ((e.KeyData & Keys.KeyCode) == Keys.F2 || (e.KeyData & Keys.KeyCode) == Keys.F5)
                    else if (e.KeyData.ToString() == "197")
                    {
                        if (!this.isCancel)
                        {
                            this.isCancel = true;
                            this.lblBarCode.ForeColor = Color.Red;
                            this.lblMessage.Text = "取消模式.";
                        }
                        else
                        {
                            this.isCancel = false;
                            this.lblBarCode.ForeColor = Color.Black;
                            this.lblMessage.Text = "正常模式.";
                        }
                    }
                    else if ((e.KeyData & Keys.KeyCode) == Keys.F1)
                    {
                        MessageBox.Show("没有任何帮助");
                        //todo Help
                    }
                }
            }
            catch (SoapException ex)
            {
                this.isMark = true;
                lblMessage.Text = ex.Message;
                Utility.ShowMessageBox(ex.Message);
            }
            catch (BusinessException ex)
            {
                this.isMark = true;
                lblMessage.Text = ex.Message;
                Utility.ShowMessageBox(ex);
            }
            catch (Exception ex)
            {
                if ((ex is System.Net.WebException) || (ex is SoapException))
                {
                    lblMessage.Text = ex.Message;
                    Utility.ShowMessageBox(ex);
                }
                else if (ex is BusinessException)
                {
                    lblMessage.Text = ex.Message;
                    Utility.ShowMessageBox(ex.Message);
                }
                else
                {
                    this.Reset();
                    lblMessage.Text = ex.Message;
                    Utility.ShowMessageBox(ex.Message);
                }
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
            this.op = Utility.GetBarCodeType(this.user.BarCodeTypes, this.barCode);


            if (string.IsNullOrEmpty(this.traceCode))
            {
                #region Van号
                this.Reset();

                this.traceCode = this.barCode;
                this.lblVANInfo.Text = this.traceCode;
               

                this.lblVAN.Visible = true;
                lblMessage.Text = "扫描成功，请扫描关键件";
                this.lblVAN.Text = "VAN:";
                #endregion
            }

            else 
            {
                #region HU

                this.smartDeviceService.ScanQualityBarCode(orderMaster.OrderNo, barCode, null, true,false, this.user.Code);
                this.huIds.Add(barCode);
                LBScanhuListDataBind();
                this.lblVAN.Visible = true;
                lblMessage.Text = "请扫Van号";
                this.lblVAN.Text = "VAN:";
                this.traceCode = string.Empty;
               
                #endregion
            }
        }

        private void LBScanhuListDataBind()
        {
            this.LBScanhuList.Items.Clear();
            for (int i = huIds.Count - 1; i >= 0; i--)
            {
                this.LBScanhuList.Items.Add((i + 1).ToString() + ". " + this.huIds[i]);
            }
            //this.LBScanhuList.Visible = true;
        }

        private void DoCancel()
        {
            this.Reset();
            this.lblMessage.Text = "已全部取消,请扫描Van号";
        }

        private void DoSubmit()
        {
            try
            {
                this.Reset();
            }
            catch (Exception ex)
            {
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
                Utility.ShowMessageBox(ex.Message);
            }
        }

        
        private void Reset()
        {
            this.traceCode = string.Empty;
            this.orderMaster = null;
            this.huIds = new List<string>();
            this.lblVANInfo.Text = "";
            this.LBScanhuList.Items.Clear();
        }

        private void InitializeDataGrid()
        {
            // 
            // columnHuId
            // 
            this.columnHuId = new DataGridTextBoxColumn();
            this.columnHuId.Format = "";
            this.columnHuId.FormatInfo = null;
            this.columnHuId.HeaderText = "条码";
            this.columnHuId.MappingName = "HuId";
            this.columnHuId.NullText = "";
            this.columnHuId.Width = 150;

        }

    }
}
