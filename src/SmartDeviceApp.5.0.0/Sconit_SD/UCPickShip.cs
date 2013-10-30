using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;

namespace com.Sconit.SmartDevice
{
    public partial class UCPickShip : UserControl
    {
        private DataGridTableStyle ts;
        private DataGridTextBoxColumn columnHu;

        public event MainForm.ModuleSelectHandler ModuleSelectionEvent;
        private SmartDeviceService smartDeviceService;
        private User user;
        private List<TempObject> hus;

        public UCPickShip(User user)
        {
            InitializeComponent();
            this.smartDeviceService = new SmartDeviceService();
            this.user = user;
            this.hus = new List<TempObject>();
            this.InitializeDataGrid();
            this.ResetAll();
            this.tbBarCode.Focus();
        }

        private void InitializeDataGrid()
        {
            this.columnHu = new DataGridTextBoxColumn();
            this.columnHu.Format = "";
            this.columnHu.FormatInfo = null;
            this.columnHu.HeaderText = "待发货条码";
            this.columnHu.MappingName = "A";
            this.columnHu.Width = 200;

            this.ts = new DataGridTableStyle();
            this.ts.MappingName = new TempObject[] {}.GetType().Name;
            this.ts.GridColumnStyles.Add(this.columnHu);
            this.dgList.TableStyles.Clear();
            this.dgList.TableStyles.Add(this.ts);
            this.dgList.DataSource = this.hus.ToArray<TempObject>();
            this.ResumeLayout();
        }

        private void ResetAll()
        {
            this.tbBarCode.Text = string.Empty;
            this.lblMessage.Text = string.Empty;
            this.tbVehicle.Text = string.Empty;
            this.hus = new List<TempObject>();
            this.dgList.DataSource = this.hus.ToArray<TempObject>();
        }

        private void ResetBarCode()
        {
            this.tbBarCode.Text = string.Empty;
            this.lblMessage.Text = string.Empty;
        }

        private void ResetShip()
        {
            this.lblMessage.Text = string.Empty;
            this.tbVehicle.Text = string.Empty;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {

        }

        private void DoCheck()
        {
            try
            {
                string hu = this.tbBarCode.Text.Trim();
                int isHuExist = this.hus.Where(t => t.A == hu).Count();
                if (string.IsNullOrEmpty(hu))
                    this.lblMessage.Text = "请扫描条码";
                else if (isHuExist > 0)
                    this.lblMessage.Text = "不能重复扫描条码";
                else
                {
                    smartDeviceService.CheckHuOnShip(hu, this.user.Code);
                    this.ResetBarCode();
                    this.lblMessage.Text = "添加成功";
                    TempObject to = new TempObject();
                    to.A = hu;
                    this.hus.Add(to);
                    this.dgList.DataSource = this.hus.ToArray<TempObject>();
                }
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
                        this.DoCheck();
                    }
                    else
                    {
                        if ((e.KeyData & Keys.KeyCode) == Keys.Enter)
                        {
                            this.DoCheck();
                        }
                    }
                }
                else
                {
                    if ((e.KeyData & Keys.KeyCode) == Keys.Enter)
                    {
                        this.DoCheck();
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
                this.ResetBarCode();
                Utility.ShowMessageBox(ex.Message);
            }
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            this.tbBarCode_KeyUp(sender, null);
        }

        private void btnShip_Click(object sender, EventArgs e)
        {
            string vehicle = this.tbVehicle.Text.Trim();
            if (string.IsNullOrEmpty(vehicle)) {
                this.ResetShip();
                Utility.ShowMessageBox("车牌号为空!");
            }
            else if (this.hus.Count == 0)
            {
                this.ResetShip();
                Utility.ShowMessageBox("待发货条码为空!");
            }
            else {
                try
                {
                    List<string> hulist = new List<string>();
                    foreach (TempObject to in hus) {
                        hulist.Add(to.A);
                    }
                    string shipno = smartDeviceService.Ship(hulist.ToArray<string>(), vehicle, this.user.Code);
                    this.ResetAll();
                    this.lblMessage.Text = "发货成功,送货单号" + shipno;
                }
                catch (SoapException ex)
                {
                    Utility.ShowMessageBox(ex.Message);
                }
                catch (BusinessException ex)
                {
                    Utility.ShowMessageBox(ex);
                }
                catch (Exception ex)
                {
                    Utility.ShowMessageBox(ex.Message);
                }
            }
        }
    }

    public class TempObject
    {
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public string D { get; set; }
        public string E { get; set; }
    }
}
