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
    public partial class UCSQShip : UserControl
    {
        public new event MainForm.ModuleSelectHandler ModuleSelectionEvent;
        private SmartDeviceService smartDeviceService;

        private DataGridTableStyle ts;

        private DataGridTextBoxColumn columnHuId;
        private DataGridTextBoxColumn columnQty;
        private DataGridTextBoxColumn columnItemDescription;
        private DataGridTextBoxColumn columnItemCode;
        private DataGridTextBoxColumn columnReferenceItemCode;
        private DataGridTextBoxColumn columnManufactureParty;
        private DataGridTextBoxColumn columnUnitCount;
        private DataGridTextBoxColumn columnUom;
        private DataGridTextBoxColumn columnIsOdd;
        private DataGridTextBoxColumn columnLotNo;
        private DataGridTextBoxColumn columnOrderNo;

        private User user;
        private List<Hu> hus;
        private bool isMark;
        private bool isCancel;
        private DateTime? effDate;
        private string region;

        private static UCSQShip ucSQShip;
        private static object obj = new object();

        private UCSQShip(User user)
        {
            this.InitializeComponent();
            this.smartDeviceService = new SmartDeviceService();
            this.user = user;
            this.btnOrder.Text = "发货";
            this.InitializeDataGrid();
            this.Reset();
        }

        public static UCSQShip GetUCSQShip(User user)
        {
            if (ucSQShip == null)
            {
                lock (obj)
                {
                    if (ucSQShip == null)
                    {
                        ucSQShip = new UCSQShip(user);
                    }
                }
            }
            ucSQShip.Reset();
            ucSQShip.lblMessage.Text = "请扫描配送标签发货。";
            return ucSQShip;
        }

        private void Reset()
        {
            this.lblMessage.Text = "请扫描配送标签发货。";
            this.hus = new List<Hu>();
            this.region = string.Empty;
            //this.cardNos = new List<string>();
            this.lblMessage.Text = "";
            this.dgDetailDataBind();
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
                    //else if ((e.KeyData & Keys.KeyCode) == Keys.F2 || (e.KeyData & Keys.KeyCode) == Keys.F5)
                    else if (e.KeyCode.ToString() == "197")
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
                    //else if ((e.KeyData & Keys.KeyCode) == Keys.F3)
                    else if (e.KeyCode.ToString() == "198")
                    {
                        
                    }
                    //else if ((e.KeyData & Keys.KeyCode) == Keys.F1)
                    else if (e.KeyCode.ToString() == "196")
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


        private void ScanBarCode()
        {
            string barCode = this.tbBarCode.Text.Trim();
            //this.tbBarCode.Focus();
            this.tbBarCode.Text = string.Empty;
            string op = Utility.GetBarCodeType(this.user.BarCodeTypes, barCode);

            if (barCode.Length < 3)
            {
                throw new BusinessException("条码格式不合法");
            }

            if (op == CodeMaster.BarCodeType.HU.ToString())
            {
                Hu hu = this.smartDeviceService.GetDistHu(barCode);
                if (this.isCancel == false)
                {
                    if (this.hus.Where(h=>h.HuId == hu.HuId).Count()>0)
                    {
                        throw new BusinessException("条码扫描重复。");
                    }
                    if (string.IsNullOrEmpty(hu.OrderNo))
                    {
                        throw new BusinessException("该条码不是配送条码，请扫描配送条码。");
                    }
                    if (hu.Status != HuStatus.NA || hu.IsEffective == true)
                    {
                        throw new BusinessException("该条码已经发货，请确认");
                    }
                    var orderMaster = this.smartDeviceService.GetOrder(hu.OrderNo,false);
                    if (string.IsNullOrEmpty(this.region))
                    {
                        this.region = orderMaster.PartyTo;
                    }
                    else
                    {
                        if (this.region != orderMaster.PartyTo)
                        {
                            throw new BusinessException("目的区域的不同不能一起发货。");
                        }
                    }

                    hus.Insert(0, hu);
                    this.dgDetailDataBind();
                }
                else
                {
                    this.CancelHu(hu);
                }
            }
            else
            {
                throw new BusinessException("请扫描配送标签上的条码");
            }
            
        }

        private void DoSubmit()
        {
            try
            {
                this.smartDeviceService.DoRepackAndShipOrder(hus.ToArray(), this.effDate, this.user.Code);
                this.Reset();
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
                Utility.ShowMessageBox(ex.Message);
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
                //this.tbBarCode.Focus();
            }
        }



        protected Hu DoCancel()
        {
            if (this.hus == null || this.hus.Count == 0)
            {
                this.Reset();
                return null;
            }
            else
            {
                Hu firstHu = this.hus.FirstOrDefault();
                this.CancelHu(firstHu);
                return firstHu;
            }
        }

        private void CancelHu(Hu hu)
        {
            //if (this.ipMaster == null && (this.orderMasters == null || this.orderMasters.Count() == 0))
            if (this.hus == null || this.hus.Count == 0)
            {
                //this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
                this.Reset();
                return;
            }

            if (hu != null)
            {
                if (this.hus.Any(h => h.HuId == hu.HuId))
                {
                    var removehu = this.hus.Where(h => h.HuId == hu.HuId).FirstOrDefault();
                    this.hus.Remove(removehu);
                    this.dgDetailDataBind();
                }
                else
                {
                    throw new BusinessException("未扫入的条码，不可以取消。");
                }
            }
        }
    

        private void btnOrder_Click(object sender, EventArgs e)
        {
            this.tbBarCode_KeyUp(sender, null);
        }

        private void btnOrder_KeyUp(object sender, KeyEventArgs e)
        {
            this.tbBarCode_KeyUp(sender, e);
        }

        private void dgDetailDataBind()
        {

            this.ts = new DataGridTableStyle();
            this.ts.MappingName = this.hus.GetType().Name;

            this.ts.GridColumnStyles.Add(columnHuId);
            this.ts.GridColumnStyles.Add(columnItemCode);
            this.ts.GridColumnStyles.Add(columnReferenceItemCode);
            this.ts.GridColumnStyles.Add(columnOrderNo);
            this.ts.GridColumnStyles.Add(columnLotNo);
            this.ts.GridColumnStyles.Add(columnManufactureParty);
            this.ts.GridColumnStyles.Add(columnItemDescription);
            this.ts.GridColumnStyles.Add(columnUnitCount);
            this.ts.GridColumnStyles.Add(columnUom);

            this.dgDetail.TableStyles.Clear();
            this.dgDetail.TableStyles.Add(this.ts);
            this.dgDetail.DataSource = this.hus;

            this.tbBarCode.Text = string.Empty;
            this.ResumeLayout();
        }

        private void InitializeDataGrid()
        {


            this.columnHuId = new DataGridTextBoxColumn();
            this.columnHuId.Format = "";
            this.columnHuId.FormatInfo = null;
            this.columnHuId.HeaderText = "条码";
            this.columnHuId.MappingName = "HuId";
            this.columnHuId.NullText = "";
            this.columnHuId.Width = 100;

            // 
            // columnItemCode
            // 
            this.columnItemCode = new DataGridTextBoxColumn();
            this.columnItemCode.Format = "";
            this.columnItemCode.FormatInfo = null;
            this.columnItemCode.HeaderText = "物料";
            this.columnItemCode.MappingName = "Item";
            this.columnItemCode.Width = 100;

            this.columnOrderNo = new DataGridTextBoxColumn();
            this.columnOrderNo.Format = "";
            this.columnOrderNo.FormatInfo = null;
            this.columnOrderNo.HeaderText = "订单号";
            this.columnOrderNo.MappingName = "OrderNo";
            this.columnOrderNo.NullText = "";
            this.columnOrderNo.Width = 100;

            this.columnLotNo = new DataGridTextBoxColumn();
            this.columnLotNo.Format = "";
            this.columnLotNo.FormatInfo = null;
            this.columnLotNo.HeaderText = "批号";
            this.columnLotNo.MappingName = "LotNo";
            this.columnLotNo.Width = 40;
            // 
            // columnCurrentQty
            // 
            this.columnQty = new DataGridTextBoxColumn();
            this.columnQty.Format = "0.##";
            this.columnQty.FormatInfo = null;
            this.columnQty.HeaderText = "数量";
            this.columnQty.MappingName = "Qty";
            this.columnQty.Width = 40;

            this.columnUnitCount = new DataGridTextBoxColumn();
            this.columnUnitCount.Format = "0.##";
            this.columnUnitCount.FormatInfo = null;
            this.columnUnitCount.HeaderText = "包装";
            this.columnUnitCount.MappingName = "UnitCount";
            this.columnUnitCount.Width = 40;

            this.columnUom = new DataGridTextBoxColumn();
            this.columnUom.Format = "";
            this.columnUom.FormatInfo = null;
            this.columnUom.HeaderText = "单位";
            this.columnUom.MappingName = "Uom";
 
            this.columnIsOdd = new DataGridTextBoxColumn();
            this.columnIsOdd.Format = "";
            this.columnIsOdd.FormatInfo = null;
            this.columnIsOdd.HeaderText = "零头";
            this.columnIsOdd.MappingName = "IsOdd";
            this.columnIsOdd.Width = 40;

            this.columnReferenceItemCode = new DataGridTextBoxColumn();
            this.columnReferenceItemCode.Format = "";
            this.columnReferenceItemCode.FormatInfo = null;
            this.columnReferenceItemCode.HeaderText = "旧图号";
            this.columnReferenceItemCode.MappingName = "ReferenceItemCode";
            this.columnReferenceItemCode.Width = 100;

            this.columnItemDescription = new DataGridTextBoxColumn();
            this.columnItemDescription.Format = "";
            this.columnItemDescription.FormatInfo = null;
            this.columnItemDescription.HeaderText = "描述";
            this.columnItemDescription.MappingName = "ItemDescription";
            this.columnItemDescription.Width = 150;

            this.columnManufactureParty = new DataGridTextBoxColumn();
            this.columnManufactureParty.Format = "";
            this.columnManufactureParty.FormatInfo = null;
            this.columnManufactureParty.HeaderText = "制造商";
            this.columnManufactureParty.MappingName = "ManufactureParty";
            this.columnManufactureParty.NullText = "";
            this.columnManufactureParty.Width = 200;


        }
    }
}
