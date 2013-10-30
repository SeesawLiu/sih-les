using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;

namespace com.Sconit.SmartDevice
{
    public partial class UCReceiveAnJiHu : UserControl
    {
        private static UCReceiveAnJiHu ucReceiveAnJiHu;
        private static object obj = new object();
        private DataGridTableStyle ts;
        private DataGridTextBoxColumn columnWMSId;
        private DataGridTextBoxColumn columnItem;
        private DataGridTextBoxColumn columnItemDesc;
        private DataGridTextBoxColumn columnRefItemCode;
        private DataGridTextBoxColumn columnQty;
        private DataGridTextBoxColumn columnUom;
        private DataGridTextBoxColumn columnLGORT;//来源库位
        private DataGridTextBoxColumn columnUMLGO;//入库库位
        private DataGridTextBoxColumn columnWmsNo;//安吉送货单号
        private DataGridTextBoxColumn columnWBS;
        private DataGridTextBoxColumn columnHuId;//安吉条码

        List<WMSDatFile> wmsDatFileList = new List<WMSDatFile>();
        public event MainForm.ModuleSelectHandler ModuleSelectionEvent;
        private SmartDeviceService smartDeviceService;
        private User user;

        private UCReceiveAnJiHu(User user)
        {
            this.InitializeComponent();
            this.InitializeDataGrid();
            this.user = user;
            this.Reset();
            this.smartDeviceService = new SmartDeviceService();
            dgWMSDatFileDataBind(false);
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            this.tbBarCode_KeyUp(sender, null);
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

        private void Reset()
        {
            this.tbBarCode.Text = string.Empty;
            this.lblMessage.Text = string.Empty;
            this.dgWMSDatFileDataBind(true);
        }

        public static UCReceiveAnJiHu GetUCReceiveAnJiHu(User user)
        {
            if (ucReceiveAnJiHu == null)
            {
                lock (obj)
                {
                    if (ucReceiveAnJiHu == null)
                    {
                        ucReceiveAnJiHu = new UCReceiveAnJiHu(user);
                    }
                }
            }
            return ucReceiveAnJiHu;
        }

        private void ScanBarCode()
        {
            var wmsDatFiles = smartDeviceService.GetWMSDatFileByAnJiHuId(this.tbBarCode.Text);

            //匹配是否重复扫描
            int isExist = wmsDatFileList.Count(w => w.HuId == this.tbBarCode.Text);
            if (isExist > 0)
            {
                DialogResult dr = MessageBox.Show("该安吉条码已经扫描，是否想取消该条码?", "重复扫描", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Yes)
                {
                    wmsDatFileList.RemoveAll(w => w.HuId == this.tbBarCode.Text);
                }
            }
            else
                wmsDatFileList.AddRange(wmsDatFiles.ToList<WMSDatFile>());
            this.dgWMSDatFileDataBind(false);
            this.tbBarCode.Text = string.Empty;
        }

        private void DoSubmit()
        {
            if (this.wmsDatFileList == null || this.wmsDatFileList.Count == 0)
            {
                throw new BusinessException("待收货明细为空");
            }
            else
            {
                try
                {
                    foreach (WMSDatFile wmsDatFile in wmsDatFileList)
                    {
                        wmsDatFile.CurrentReceiveQty = wmsDatFile.Qty - wmsDatFile.ReceiveTotal + wmsDatFile.CancelQty;
                    }
                    smartDeviceService.ReceiveWMSDatFile(wmsDatFileList.ToArray<WMSDatFile>(), user.Code);
                    this.Reset();
                    this.lblMessage.Text = "收货成功";
                    dgWMSDatFileDataBind(false);
                }
                catch (SoapException ex)
                {
                    this.Reset();
                    Utility.ShowMessageBox(ex.Message);
                }
                catch (BusinessException ex)
                {
                    this.Reset();
                    Utility.ShowMessageBox(ex);
                }
                catch (Exception ex)
                {
                    this.Reset();
                    Utility.ShowMessageBox(ex.Message);
                }
            }
        }

        private void dgWMSDatFileDataBind(bool isReset)
        {
            this.ts = new DataGridTableStyle();
            this.ts.MappingName = new WMSDatFile[] { }.GetType().Name;
            this.ts.GridColumnStyles.Add(this.columnWMSId);
            this.ts.GridColumnStyles.Add(this.columnItem);
            this.ts.GridColumnStyles.Add(this.columnItemDesc);
            this.ts.GridColumnStyles.Add(this.columnRefItemCode);
            this.ts.GridColumnStyles.Add(this.columnQty);
            this.ts.GridColumnStyles.Add(this.columnUom);
            this.ts.GridColumnStyles.Add(this.columnLGORT);
            this.ts.GridColumnStyles.Add(this.columnUMLGO);
            this.ts.GridColumnStyles.Add(this.columnWmsNo);
            this.ts.GridColumnStyles.Add(this.columnWBS);
            this.ts.GridColumnStyles.Add(this.columnHuId);

            this.dgWMSDatFile.TableStyles.Clear();
            this.dgWMSDatFile.TableStyles.Add(this.ts);
            

            if (isReset)
            {
                this.wmsDatFileList = new List<WMSDatFile>();   
            }
            this.dgWMSDatFile.DataSource = wmsDatFileList.ToArray<WMSDatFile>();
            this.ResumeLayout();
        }

        private void InitializeDataGrid()
        {
            this.columnWMSId = new DataGridTextBoxColumn();
            this.columnWMSId.Format = "";
            this.columnWMSId.FormatInfo = null;
            this.columnWMSId.HeaderText = "WMSId";
            this.columnWMSId.MappingName = "WMSId";
            this.columnWMSId.Width = 100;

            this.columnItem = new DataGridTextBoxColumn();
            this.columnItem.Format = "";
            this.columnItem.FormatInfo = null;
            this.columnItem.HeaderText = "物料PRP号";
            this.columnItem.MappingName = "Item";
            this.columnItem.Width = 100;

            this.columnItemDesc = new DataGridTextBoxColumn();
            this.columnItemDesc.Format = "";
            this.columnItemDesc.FormatInfo = null;
            this.columnItemDesc.HeaderText = "物料描述";
            this.columnItemDesc.MappingName = "ItemDescription";
            this.columnItemDesc.Width = 100;

            this.columnRefItemCode = new DataGridTextBoxColumn();
            this.columnRefItemCode.Format = "";
            this.columnRefItemCode.FormatInfo = null;
            this.columnRefItemCode.HeaderText = "物料老图号";
            this.columnRefItemCode.MappingName = "ReferenceItemCode";
            this.columnRefItemCode.Width = 100;

            this.columnQty = new DataGridTextBoxColumn();
            this.columnQty.Format = "";
            this.columnQty.FormatInfo = null;
            this.columnQty.HeaderText = "数量";
            this.columnQty.MappingName = "Qty";
            this.columnQty.NullText = "";
            this.columnQty.Width = 100;

            this.columnUom = new DataGridTextBoxColumn();
            this.columnUom.Format = "";
            this.columnUom.FormatInfo = null;
            this.columnUom.HeaderText = "单位";
            this.columnUom.MappingName = "Uom";
            this.columnUom.Width = 40;

            this.columnLGORT = new DataGridTextBoxColumn();
            this.columnLGORT.Format = "";
            this.columnLGORT.FormatInfo = null;
            this.columnLGORT.HeaderText = "来源库位";
            this.columnLGORT.MappingName = "LGORT";
            this.columnLGORT.Width = 40;

            this.columnUMLGO = new DataGridTextBoxColumn();
            this.columnUMLGO.Format = "";
            this.columnUMLGO.FormatInfo = null;
            this.columnUMLGO.HeaderText = "目的库位";
            this.columnUMLGO.MappingName = "UMLGO";
            this.columnUMLGO.Width = 100;

            this.columnWmsNo = new DataGridTextBoxColumn();
            this.columnWmsNo.Format = "";
            this.columnWmsNo.FormatInfo = null;
            this.columnWmsNo.HeaderText = "安吉单据号";
            this.columnWmsNo.MappingName = "WmsNo";
            this.columnWmsNo.Width = 100;

            this.columnWBS = new DataGridTextBoxColumn();
            this.columnWBS.Format = "";
            this.columnWBS.FormatInfo = null;
            this.columnWBS.HeaderText = "WBS";
            this.columnWBS.MappingName = "WBS";
            this.columnWBS.NullText = "";
            this.columnWBS.Width = 100;

            this.columnHuId = new DataGridTextBoxColumn();
            this.columnHuId.Format = "";
            this.columnHuId.FormatInfo = null;
            this.columnHuId.HeaderText = "安吉条码";
            this.columnHuId.MappingName = "HuId";
            this.columnHuId.NullText = "";
            this.columnHuId.Width = 100;
        }
    }
}
