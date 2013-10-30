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
    public partial class UCMaterialIn : UserControl
    {
        public event MainForm.ModuleSelectHandler ModuleSelectionEvent;

        protected DataGridTableStyle ts;
        protected DataGridTextBoxColumn columnHuId;

        private User user;
        private OrderMaster orderMaster;
        private List<string> huIds;
        private bool isMark;
        private bool isCancel;
        private string barCode;
        private string op;
        private string traceCode;

        protected bool isForceFeed;

        private SmartDeviceService smartDeviceService;


        private static UCMaterialIn ucMaterialIn;
        private static object obj = new object();

        protected UCMaterialIn(User user)
        {
            InitializeComponent();
            this.smartDeviceService = new SmartDeviceService();
            this.user = user;
            this.InitializeDataGrid();
            this.Reset();
        }

        public static UCMaterialIn GetUCMaterialIn(User user)
        {
            if (ucMaterialIn == null)
            {
                lock (obj)
                {
                    if (ucMaterialIn == null)
                    {
                        ucMaterialIn = new UCMaterialIn(user);
                    }
                }
            }
            ucMaterialIn.Reset();
            ucMaterialIn.lblMessage.Text = "请扫描Van号";
            return ucMaterialIn;
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

        private void DoCancel()
        {
            this.Reset();
            this.lblMessage.Text = "已全部取消";
        }

        private void DoSubmit()
        {
            try
            {
                //if (this.orderMaster == null)
                //{
                //    throw new BusinessException("请扫描生产单。");
                //}
                this.Reset();
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
            this.barCode = this.tbBarCode.Text.Trim();
            this.lblMessage.Text = string.Empty;
            this.tbBarCode.Text = string.Empty;

            if (this.barCode.Length < 3)
            {
                throw new BusinessException("条码格式不合法");
            }
            //this.op = Utility.GetBarCodeType(this.user.BarCodeTypes, this.barCode);


            if (string.IsNullOrEmpty(this.traceCode))
            {
                #region Van号
                this.Reset();
                this.lblVAN.Text = "VAN号:";

                this.lblVANInfo.Text = this.barCode;
                this.traceCode = this.barCode;
                
                this.lblVAN.Visible = true;
                lblMessage.Text = "请扫描关键件";
                #endregion
            }
          
            else
            {
                
               
                this.smartDeviceService.ScanQualityBarCode(this.traceCode, barCode, null, false,false, this.user.Code);
                this.huIds.Add(barCode);
                LBScanhuListDataBind();
                this.lblVANInfo.Text = string.Empty;
                this.traceCode = string.Empty;
                this.lblVAN.Visible = true;
                lblMessage.Text = "扫描成功，请扫描VAN号";
            }
        }

        private void LBScanhuListDataBind()
        {
            this.LBScanhuList.Items.Clear();
            for (int i = huIds.Count-1; i >=0; i--)
            {
                this.LBScanhuList.Items.Add((i+1).ToString()+". "+this.huIds[i]);
            }
            //this.LBScanhuList.Visible = true;
        }
           


        private void Reset()
        {
            this.orderMaster = null;
            this.huIds = new List<string>();
            this.tbBarCode.Text = string.Empty;
            //this.tbBarCode.Focus();

            this.isCancel = false;

            this.lblVANInfo.Text = string.Empty;


            this.lblVAN.Visible = false;

            this.LBScanhuList.Items.Clear();
            this.traceCode = string.Empty;
            this.lblMessage.Text ="已全部取消,请扫描Van号";
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
