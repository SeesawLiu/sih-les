using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;

namespace com.Sconit.SmartDevice
{
    public partial class UCSQReceive : UserControl
    {
        private static UCSQReceive ucSQReceive;
        private static object obj = new object();
        private DataGridTableStyle ts;
        private DataGridTextBoxColumn columnWmsId;
        private DataGridTextBoxColumn columnItem;
        private DataGridTextBoxColumn columnItemDesc;
        private DataGridTextBoxColumn columnRefItemCode;
        private DataGridTextBoxColumn columnQty;
        private DataGridTextBoxColumn columnHuId;//安吉条码

        IList<IpDetailInput> ipDetailInputList = new List<IpDetailInput>();
        public event MainForm.ModuleSelectHandler ModuleSelectionEvent;
        private SmartDeviceService smartDeviceService;
        private User user;

        private UCSQReceive(User user)
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

        public static UCSQReceive GetUCSQReceive(User user)
        {
            if (ucSQReceive == null)
            {
                lock (obj)
                {
                    if (ucSQReceive == null)
                    {
                        ucSQReceive = new UCSQReceive(user);
                    }
                }
            }
            return ucSQReceive;
        }

        private void ScanBarCode()
        {
            var ipDetailInput = smartDeviceService.GetIpDetailInputByPickHu(this.tbBarCode.Text);

            //匹配是否重复扫描
            int isExist = ipDetailInputList.Count(w => w.Id == ipDetailInput.Id);
            if (isExist > 0)
            {
                DialogResult dr = MessageBox.Show("该配送条码已经扫描，是否想取消该条码?", "重复扫描", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Yes)
                {
                    var theRepitition = ipDetailInputList.FirstOrDefault(w => w.Id == ipDetailInput.Id);
                    ipDetailInputList.Remove(theRepitition);
                }
            }
            else
                ipDetailInputList.Add(ipDetailInput);

            this.dgWMSDatFileDataBind(false);
            this.tbBarCode.Text = string.Empty;
        }

        private void DoSubmit()
        {
            if (this.ipDetailInputList == null || this.ipDetailInputList.Count == 0)
            {
                throw new BusinessException("待收货明细为空");
            }
            else
            {
                try
                {
                    foreach (IpDetailInput ipDetailInput in ipDetailInputList)
                    {
                        ipDetailInput.ReceiveQty = ipDetailInput.Qty;
                    }
                    smartDeviceService.DoReceiveIp(ipDetailInputList.ToArray<IpDetailInput>(), DateTime.Now, user.Code);
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
            this.ts.MappingName = new IpDetailInput[] { }.GetType().Name;
            this.ts.GridColumnStyles.Add(this.columnWmsId);
            this.ts.GridColumnStyles.Add(this.columnItem);
            this.ts.GridColumnStyles.Add(this.columnItemDesc);
            this.ts.GridColumnStyles.Add(this.columnRefItemCode);
            this.ts.GridColumnStyles.Add(this.columnQty);
            this.ts.GridColumnStyles.Add(this.columnHuId);

            this.dgWMSDatFile.TableStyles.Clear();
            this.dgWMSDatFile.TableStyles.Add(this.ts);
            

            if (isReset)
            {
                this.ipDetailInputList = new List<IpDetailInput>();   
            }
            this.dgWMSDatFile.DataSource = ipDetailInputList.ToArray<IpDetailInput>();
            this.ResumeLayout();
        }

        private void InitializeDataGrid()
        {
            this.columnWmsId = new DataGridTextBoxColumn();
            this.columnWmsId.Format = "";
            this.columnWmsId.FormatInfo = null;
            this.columnWmsId.HeaderText = "Id";
            this.columnWmsId.MappingName = "Id";
            this.columnWmsId.Width = 100;

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

            this.columnHuId = new DataGridTextBoxColumn();
            this.columnHuId.Format = "";
            this.columnHuId.FormatInfo = null;
            this.columnHuId.HeaderText = "配送条码";
            this.columnHuId.MappingName = "WMSSeq";
            this.columnHuId.NullText = "";
            this.columnHuId.Width = 100;
        }
    }
}
