using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;
using System.Collections;
using System.Drawing;

namespace com.Sconit.SmartDevice
{
    public partial class UCStockTaking : UCBase
    {
        public new event MainForm.ModuleSelectHandler ModuleSelectionEvent;
        private StockTakeMaster stockTakeMaster;

        private static UCStockTaking ucStockTaking;
        private static object obj = new object();
        private string binCode;
        private string locationCode;
        //private ArrayList detailStringArray = new ArrayList();

        private UCStockTaking(User user)
            : base(user)
        {
            this.InitializeComponent();
        }

        public static UCStockTaking GetUCStockTaking(User user)
        {
            if (ucStockTaking == null)
            {
                lock (obj)
                {
                    if (ucStockTaking == null)
                    {
                        ucStockTaking = new UCStockTaking(user);
                    }
                }
            }
            ucStockTaking.Reset();
            return ucStockTaking;
        }

        protected override void tbBarCode_KeyUp(object sender, KeyEventArgs e)
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
                    //else if ((e.KeyData & Keys.KeyCode) == Keys.F3)
                    else if (e.KeyData.ToString() == "198")
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
                            this.lblMessage.Text = this.binCode != null ? "当前库格:" + this.binCode : "当前库位:" + this.locationCode;
                        }
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


        protected override void ScanBarCode()
        {
            string barCode = this.tbBarCode.Text.Trim();
            //this.tbBarCode.Focus();
            this.tbBarCode.Text = string.Empty;
            string op = Utility.GetBarCodeType(this.user.BarCodeTypes, barCode);

            if (barCode.Length < 3)
            {
                throw new BusinessException("条码格式不合法");
            }

            if (this.stockTakeMaster == null && !this.isCancel)
            {
                if (op == CodeMaster.BarCodeType.STT.ToString())
                {
                    var stockTakeMaster = this.smartDeviceService.GetStockTake(barCode);
                    if (stockTakeMaster == null)
                    {
                        throw new BusinessException("该盘点单{0}不存在", barCode);
                    }
                    else if(stockTakeMaster.IsScan)
                    {
                        throw new BusinessException("该盘点单{0}不可以扫描条码", barCode);
                    }
                    else if (stockTakeMaster.Status != StockTakeStatus.InProcess && stockTakeMaster.Status != StockTakeStatus.Submit)
                    {
                        throw new BusinessException("盘点单{0}状态不为执行中或者提交", stockTakeMaster.StNo);
                    }
                    this.lblMessage.Text = string.Format("盘点单号{0};", stockTakeMaster.StNo);
                    this.stockTakeMaster = stockTakeMaster;
                }
                else
                {
                    throw new BusinessException("请先扫描盘点单");
                }
            }
            else if (!this.isCancel)
            {
                if (op == CodeMaster.BarCodeType.B.ToString())
                {
                    barCode = barCode.Substring(2, barCode.Length - 2);
                    Bin bin = this.smartDeviceService.GetBin(barCode);
                    if (bin == null)
                    {
                        throw new BusinessException("当前扫描的库格不存在");
                    }
                    else if (!this.stockTakeMaster.Location.Contains(bin.Location))
                    {
                        throw new BusinessException("当前库格不在该盘点单的库位");
                    }
                    else
                    {
                        this.lblMessage.Text = "当前库格: " + bin.Code;

                        //if (Utility.HasPermission(user.Permissions, null, true, false, bin.Region, null))
                        //{
                        //    throw new BusinessException("没有操作此区域{0}的权限", bin.Region);
                        //}
                        this.binCode = bin.Code;
                        this.locationCode = bin.Location;
                    }
                }
                else if (op == CodeMaster.BarCodeType.L.ToString())
                {
                    barCode = barCode.Substring(2, barCode.Length - 2);
                    Location location = this.smartDeviceService.GetLocation(barCode);
                    if (location == null)
                    {
                        throw new BusinessException("当前扫描的库位不存在");
                    }
                    else if (!this.stockTakeMaster.Location.Contains(location.Code))
                    {
                        throw new BusinessException("当前库位不是该盘点单的库位");
                    }
                    else
                    {
                        this.lblMessage.Text = "当前库位: " + location.Code;

                        //if (Utility.HasPermission(user.Permissions, null, true, false, location.Region, null))
                        //{
                        //    throw new BusinessException("没有操作此区域{0}的权限", location.Region);
                        //}
                        this.locationCode = location.Code;
                        this.binCode = null;
                    }
                }

                else if (string.IsNullOrEmpty(this.binCode) && string.IsNullOrEmpty(this.locationCode))
                {
                    throw new BusinessException("请扫描库格或者库位");
                }
                else if (op == CodeMaster.BarCodeType.HU.ToString())
                {
                    if (hus.Where(h => h.HuId == barCode).ToList().Count > 0)
                    {
                        throw new BusinessException("请不要重复扫描条码");
                    }
                    Hu hu = this.smartDeviceService.GetHu(barCode);
                    if (hu == null)
                    {
                        throw new BusinessException("此条码不存在");
                    }
                    hu.Region = this.binCode;
                    hu.Location = this.locationCode != null ? this.locationCode : string.Empty;
                    this.hus.Insert(0, hu);
                    //detailStringArray.Add(new string[] { hu.HuId, this.binCode, this.locationCode != null ? this.locationCode : string.Empty });
                    base.gvHuListDataBind();

                    this.isCancel = false;
                }
                else
                {
                    throw new BusinessException("条码格式不合法");
                }
            }
            else
            {
                if (op == CodeMaster.BarCodeType.HU.ToString())
                {
                    if (hus.Where(h => h.HuId == barCode).ToList().Count > 0)
                    {
                        this.hus = this.hus.Where(h => h.HuId != barCode).ToList();
                        base.gvHuListDataBind();
                    }
                    else
                    {
                        throw new BusinessException("条码{0}未扫入不需取消", barCode);
                    }
                }
            }
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            this.tbBarCode_KeyUp(null, null);
        }

        protected override void DoSubmit()
        {
            try
            {
                if (this.stockTakeMaster == null)
                {
                    throw new BusinessException("请先扫描盘点单");
                }
                if (hus == null)
                {
                    throw new BusinessException("未扫入物料条码,不可以提交");
                }

                string[][] details = new string[hus.Count][];
                for (int i = 0; i < hus.Count; i++)
                {
                    details[i] = new string[] { hus[i].HuId, hus[i].Region, hus[i].Location };
                }

                smartDeviceService.DoStockTake(this.stockTakeMaster.StNo, details, user.Code);
                this.Reset();
            }
            catch (Exception ex)
            {
                this.isMark = true;
                this.tbBarCode.Text = string.Empty;
                //this.tbBarCode.Focus();
                Utility.ShowMessageBox(ex.Message);
            }
        }

        protected override Hu DoCancel()
        {
            if (this.hus == null || this.hus.Count==0)
            {
                this.Reset();
                return null;
            }

            Hu firstHu = hus.First();
            hus.Remove(firstHu);
            this.lblMessage.ForeColor = Color.Red;
            this.lblMessage.Text = string.Format("当前取消条码{0}",firstHu.HuId);
            this.gvHuListDataBind();
            return firstHu;
        }

        protected override void Reset()
        {
            this.hus = new List<Hu>();
            this.stockTakeMaster = null;
            this.lblMessage.Text = "请扫描盘点单号";
            this.tbBarCode.Text = string.Empty;
            this.isCancel = false;
            this.isMasterBind = true;
            this.gvHuListDataBind();
        }


    }
}
