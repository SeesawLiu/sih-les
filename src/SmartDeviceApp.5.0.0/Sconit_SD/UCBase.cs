using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;
using System.Drawing;
using System.Data;

namespace com.Sconit.SmartDevice
{
    public partial class UCBase : UserControl
    {
        public new event MainForm.ModuleSelectHandler ModuleSelectionEvent;

        protected DataGridTableStyle ts;
        protected DataGridTextBoxColumn columnHuId;
        protected DataGridTextBoxColumn columnCurrentQty;
        protected DataGridTextBoxColumn columnItemDescription;
        protected DataGridTextBoxColumn columnItemCode;
        protected DataGridTextBoxColumn columnReferenceItemCode;
        protected DataGridTextBoxColumn columnManufactureParty;
        protected DataGridTextBoxColumn columnUnitCount;
        protected DataGridTextBoxColumn columnUom;
        protected DataGridTextBoxColumn columnCarton;
        protected DataGridTextBoxColumn columnIsOdd;
        protected DataGridTextBoxColumn columnLotNo;
        protected DataGridTextBoxColumn columnSupplierLotNo;
        protected DataGridTextBoxColumn columnOrderedQty;

        protected User user;
        protected List<Hu> hus;
        protected bool isMark;
        protected bool isMasterBind;
        protected string barCode;
        protected bool isCancel;
        protected string op;

        protected SmartDeviceService smartDeviceService;

        public UCBase()
        {
        }

        public UCBase(User user)
        {
            this.smartDeviceService = new SmartDeviceService();
            this.user = user;
            this.InitializeComponent();
            this.InitializeDataGrid();
            this.Reset();
        }

        protected virtual void tbBarCode_KeyUp(object sender, KeyEventArgs e)
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
                        //MessageBox.Show("199");
                        this.ModuleSelectionEvent(CodeMaster.TerminalPermission.M_Switch);
                    }
                    //else if ((e.KeyData & Keys.KeyCode) == Keys.F3)
                    else if (e.KeyCode.ToString() == "198")
                    {
                        if (this.isMasterBind)
                        {
                            this.gvHuListDataBind();
                        }
                        else
                        {
                            this.gvListDataBind();
                        }
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

        protected virtual void ScanBarCode()
        {
            this.barCode = this.tbBarCode.Text.Trim();
            //this.lblMessage.Text = string.Empty;
            if (this.barCode.Length < 3)
            {
                throw new BusinessException("条码格式不合法");
            }
            op = Utility.GetBarCodeType(this.user.BarCodeTypes, this.barCode);
            this.tbBarCode.Text = string.Empty;
        }

        protected virtual void gvListDataBind()
        {
            this.tbBarCode.Text = string.Empty;
            ////this.tbBarCode.Focus();
            //this.dgList.DataSource = details;
            //ts.MappingName = details.GetType().Name;
            this.ts = new DataGridTableStyle();

            this.ts.GridColumnStyles.Add(columnItemCode);
            this.ts.GridColumnStyles.Add(columnCurrentQty);
            this.ts.GridColumnStyles.Add(columnCarton);
            this.ts.GridColumnStyles.Add(columnReferenceItemCode);
            this.ts.GridColumnStyles.Add(columnManufactureParty);
            this.ts.GridColumnStyles.Add(columnLotNo);
            this.ts.GridColumnStyles.Add(columnIsOdd);
            this.ts.GridColumnStyles.Add(columnUnitCount);
            this.ts.GridColumnStyles.Add(columnUom);
            this.ts.GridColumnStyles.Add(columnItemDescription);

            this.dgList.TableStyles.Clear();
            this.dgList.TableStyles.Add(this.ts);

            this.ResumeLayout();
            this.isMasterBind = true;
        }

        protected virtual void gvHuListDataBind()
        {
            List<Hu> hus = new List<Hu>();
            if (this.hus != null)
            {
                hus = this.hus;
            }
            this.tbBarCode.Text = string.Empty;
            ////this.tbBarCode.Focus();

            this.ts = new DataGridTableStyle();
            this.ts.MappingName = hus.GetType().Name;

            this.ts.GridColumnStyles.Add(columnHuId);
            this.ts.GridColumnStyles.Add(columnItemCode);
            this.ts.GridColumnStyles.Add(columnReferenceItemCode);
            this.ts.GridColumnStyles.Add(columnCurrentQty);
            this.ts.GridColumnStyles.Add(columnLotNo);
            this.ts.GridColumnStyles.Add(columnManufactureParty);
            this.ts.GridColumnStyles.Add(columnItemDescription);
            this.ts.GridColumnStyles.Add(columnSupplierLotNo);
            this.ts.GridColumnStyles.Add(columnUnitCount);
            this.ts.GridColumnStyles.Add(columnUom);

            this.dgList.TableStyles.Clear();
            this.dgList.TableStyles.Add(this.ts);

            this.dgList.DataSource = hus;

            this.tbBarCode.Text = string.Empty;
            this.ResumeLayout();
            this.isMasterBind = false;
        }

        protected virtual void Reset()
        {
            this.hus = new List<Hu>();
            this.gvListDataBind();
            this.lblMessage.Text = string.Empty;
            this.tbBarCode.Text = string.Empty;
            this.isCancel = false;
            this.lblBarCode.ForeColor = Color.Black;
            ////this.tbBarCode.Focus();
        }

        protected virtual void DoSubmit()
        {

        }

        protected virtual Hu DoCancel()
        {
            if (this.hus == null)
            {
                this.Reset();
                return null;
            }
            if (this.hus.Count == 0)
            {
                this.gvListDataBind();
                this.hus = null;
                return null;
            }
            if (this.isMasterBind)
            {
                this.gvHuListDataBind();
                return null;
            }

            Hu firstHu = hus.First();

            this.lblMessage.Text = "已取消条码:" + firstHu.HuId;

            return firstHu;
        }

        protected void CheckHu(Hu hu)
        {
            if (hu == null)
            {
                throw new BusinessException("条码不存在");
            }
            hu.CurrentQty = hu.Qty;

            var matchHu = this.hus.Where(h => h.HuId.Equals(hu.HuId, StringComparison.OrdinalIgnoreCase));

            if (this.isCancel)
            {
                if (matchHu == null || matchHu.Count() == 0)
                {
                    throw new BusinessException("没有需要取消匹配条码:{0}", hu.HuId);
                }
                else if (matchHu.Count() == 1)
                {
                    //var _hu = _hus.Single();
                    //this.hus.Remove(_hu);
                }
                else
                {
                    throw new Exception("匹配了多个条码");
                }
            }
            else
            {
                if (matchHu != null && matchHu.Count() > 0)
                {
                    throw new BusinessException("条码重复扫描!");
                }
            }

            if (this.hus == null)
            {
                this.hus = new List<Hu>();
            }
            if (hu.IsFreeze)
            {
                throw new BusinessException("条码被冻结!");
            }
            if (hu.OccupyType != OccupyType.None)
            {
                throw new BusinessException("条码被{0}占用!", hu.OccupyReferenceNo);
            }

            if (!Utility.HasPermission(user.Permissions, null, false, true, hu.Region, null))
            {
                throw new BusinessException("没有此条码的权限");
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

        private void InitializeDataGrid()
        {
            // 
            // columnItemCode
            // 
            this.columnItemCode = new DataGridTextBoxColumn();
            this.columnItemCode.Format = "";
            this.columnItemCode.FormatInfo = null;
            this.columnItemCode.HeaderText = "物料";
            this.columnItemCode.MappingName = "Item";
            this.columnItemCode.Width = 100;
            // 
            // columnLotNo
            // 
            this.columnLotNo = new DataGridTextBoxColumn();
            this.columnLotNo.Format = "";
            this.columnLotNo.FormatInfo = null;
            this.columnLotNo.HeaderText = "批号";
            this.columnLotNo.MappingName = "LotNo";
            this.columnLotNo.Width = 40;
            // 
            // columnCurrentQty
            // 
            this.columnCurrentQty = new DataGridTextBoxColumn();
            this.columnCurrentQty.Format = "0.##";
            this.columnCurrentQty.FormatInfo = null;
            this.columnCurrentQty.HeaderText = "数量";
            this.columnCurrentQty.MappingName = "CurrentQty";
            this.columnCurrentQty.Width = 40;
            // 
            // columnCarton
            // 
            this.columnCarton = new DataGridTextBoxColumn();
            this.columnCarton.Format = "";
            this.columnCarton.FormatInfo = null;
            this.columnCarton.HeaderText = "箱数";
            this.columnCarton.MappingName = "Carton";
            this.columnCarton.Width = 40;
            // 
            // columnUnitCount
            // 
            this.columnUnitCount = new DataGridTextBoxColumn();
            this.columnUnitCount.Format = "0.##";
            this.columnUnitCount.FormatInfo = null;
            this.columnUnitCount.HeaderText = "包装";
            this.columnUnitCount.MappingName = "UnitCount";
            this.columnUnitCount.Width = 40;
            // 
            // columnUom
            // 
            this.columnUom = new DataGridTextBoxColumn();
            this.columnUom.Format = "";
            this.columnUom.FormatInfo = null;
            this.columnUom.HeaderText = "单位";
            this.columnUom.MappingName = "Uom";
            this.columnUom.Width = 40;
            // 
            // columnIsOdd
            // 
            this.columnIsOdd = new DataGridTextBoxColumn();
            this.columnIsOdd.Format = "";
            this.columnIsOdd.FormatInfo = null;
            this.columnIsOdd.HeaderText = "零头";
            this.columnIsOdd.MappingName = "IsOdd";
            this.columnIsOdd.Width = 40;
            // 
            // columnReferenceItemCode
            // 
            this.columnReferenceItemCode = new DataGridTextBoxColumn();
            this.columnReferenceItemCode.Format = "";
            this.columnReferenceItemCode.FormatInfo = null;
            this.columnReferenceItemCode.HeaderText = "旧图号";
            this.columnReferenceItemCode.MappingName = "ReferenceItemCode";
            this.columnReferenceItemCode.Width = 100;
            // 
            // columnItemDescription
            // 
            this.columnItemDescription = new DataGridTextBoxColumn();
            this.columnItemDescription.Format = "";
            this.columnItemDescription.FormatInfo = null;
            this.columnItemDescription.HeaderText = "描述";
            this.columnItemDescription.MappingName = "ItemDescription";
            this.columnItemDescription.Width = 150;
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

            this.columnSupplierLotNo = new DataGridTextBoxColumn();
            this.columnSupplierLotNo.Format = "";
            this.columnSupplierLotNo.FormatInfo = null;
            this.columnSupplierLotNo.HeaderText = "制造商批号";
            this.columnSupplierLotNo.MappingName = "SupplierLotNo";
            this.columnSupplierLotNo.NullText = "";
            this.columnSupplierLotNo.Width = 200;

            this.columnManufactureParty = new DataGridTextBoxColumn();
            this.columnManufactureParty.Format = "";
            this.columnManufactureParty.FormatInfo = null;
            this.columnManufactureParty.HeaderText = "制造商";
            this.columnManufactureParty.MappingName = "ManufactureParty";
            this.columnManufactureParty.NullText = "";
            this.columnManufactureParty.Width = 200;

            this.columnOrderedQty = new DataGridTextBoxColumn();
            this.columnOrderedQty.Format = "";
            this.columnOrderedQty.FormatInfo = null;
            this.columnOrderedQty.HeaderText = "订单数";
            this.columnOrderedQty.MappingName = "OrderedQty";
            this.columnOrderedQty.NullText = "";
            this.columnOrderedQty.Width = 100;
        }

        //private void tbBarCode_LostFocus(object sender, EventArgs e)
        //{
        //    if (!this.btnOrder.Focused)
        //    {
        //        //this.tbBarCode.Focus();
        //    }
        //}

        //MMddHH =>yyyy-MM-dd HH
        private DateTime GetEffDate(string date)
        {
            try
            {
                DateTime effDate = DateTime.Now;
                if (date.Length == 6)
                {
                    string newStr = effDate.Year.ToString() + "-";//年
                    newStr += date.Substring(0, 2) + "-";//月
                    newStr += date.Substring(2, 2) + " ";//日
                    newStr += date.Substring(4, 2) + ":00:00";//时:分:秒
                    effDate = DateTime.Parse(newStr);
                    if (effDate > DateTime.Now)
                    {
                        if (effDate.Month == 1)
                        {
                            effDate.AddYears(-1);
                        }
                        else
                        {
                            throw new BusinessException("输入的时间不能大于当前时间");
                        }
                    }
                }
                else
                {
                    throw new BusinessException("输入正确的格式,2月3日14时输入..020314");
                }
                return effDate;
            }
            catch (Exception ex)
            {
                throw new BusinessException(ex.Message);
            }
        }
    }

    public class GroupHu
    {
        public string Item { get; set; }
        public decimal CurrentQty { get; set; }
        public int Carton { get; set; }
        public string LotNo { get; set; }
        //public bool IsOdd { get; set; }
        public decimal UnitCount { get; set; }
        public string Uom { get; set; }
        public string ReferenceItemCode { get; set; }
        public string ItemDescription { get; set; }
    }
}
