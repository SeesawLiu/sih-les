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

    public partial class UCScanEngine : UserControl
    {
        public event MainForm.ModuleSelectHandler ModuleSelectionEvent;
        private SmartDeviceService smartDeviceService;
        private User user;
        private bool isMark;
        private bool isCancel;
        private string traceCode;
        private string engineCode;

        private static UCScanEngine ucScanEngine;
        private static object obj = new object();

        public UCScanEngine(User user)
        {
            this.InitializeComponent();
            this.smartDeviceService = new SmartDeviceService();
            this.user = user;

            this.Reset();
        }

        public static UCScanEngine GetUCScanEngine(User user)
        {
            if (ucScanEngine == null)
            {
                lock (obj)
                {
                    if (ucScanEngine == null)
                    {
                        ucScanEngine = new UCScanEngine(user);
                    }
                }
            }
            return ucScanEngine;
        }


        private void Reset()
        {
            lblMessage.Text = "已全部取消,请扫描Van号。";
            tbBarCode.Text = string.Empty;
            tbBarCode.Focus();
            lblTraceCodeInfo.Text = string.Empty;
            lblEngineInfo.Text = string.Empty;
            this.traceCode = string.Empty;
            this.engineCode = string.Empty;
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
                string currentCode = this.tbBarCode.Text.Trim();
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
                            if (currentCode != string.Empty)
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
                        if (currentCode != string.Empty)
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
                        this.DoCancel();
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
                this.DoCancel();
                Utility.ShowMessageBox(ex.Message);
            }
            catch (BusinessException ex)
            {
                this.isMark = true;
                this.DoCancel();
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
                this.DoCancel();
            }
        }

        private void DoCancel()
        {
            this.Reset();
            this.lblMessage.Text = "已全部取消,请扫描Van号。";
        }

        private void DoSubmit()
        {
            try
            {
                smartDeviceService.ScanEngineTraceBarCode(this.engineCode, this.traceCode, this.user.Code);
                this.Reset();
                this.lblMessage.Text = "记录成功,请扫描下个Van号。";
            }
            catch (SoapException ex)
            {
                this.isMark = true;
                this.DoCancel();
                Utility.ShowMessageBox(ex.Message);
            }
            catch (BusinessException ex)
            {
                this.isMark = true;
                this.DoCancel();
                Utility.ShowMessageBox(ex);
            }
            catch (Exception ex)
            {
                Utility.ShowMessageBox(ex.Message);
                this.DoCancel();
                this.tbBarCode.Text = string.Empty;
            }
        }

        private void ScanBarCode()
        {
            string currentBarCode = this.tbBarCode.Text.Trim();
            this.lblMessage.Text = string.Empty;
            this.tbBarCode.Text = string.Empty;

            if (string.IsNullOrEmpty(this.traceCode))
            {
                this.traceCode = currentBarCode;
                this.lblTraceCodeInfo.Text = currentBarCode;
                this.lblMessage.Text = "请扫描发动机条码";
            }
            else if (string.IsNullOrEmpty(this.engineCode))
            {
                this.engineCode = currentBarCode;
                this.lblEngineInfo.Text = currentBarCode;
                DoSubmit();
            }
           
        }

 
    }
}
