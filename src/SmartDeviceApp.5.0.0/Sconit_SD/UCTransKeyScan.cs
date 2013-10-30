using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using com.Sconit.SmartDevice.SmartDeviceRef;
using System.Web.Services.Protocols;

namespace com.Sconit.SmartDevice
{
    public partial class UCTransKeyScan : UCBase
    {
        //public event MainForm.ModuleSelectHandler ModuleSelectionEvent;

        private static object obj = new object();
        private List<OrderMaster> orderMasters;
        private OrderMaster orderMaster;
        private DateTime? effDate;

        public UCTransKeyScan(User user)
            : base(user)
        {
            this.Reset();
            this.InitializeComponent();
            base.btnOrder.Text = "确认";
            this.lblMessage.Text = "请扫描KIT单";
        }

        protected override void ScanBarCode()
        {
            base.ScanBarCode();

            if (this.isCancel == false)
            {
                if (this.orderMaster == null)
                {
                    var orderMaster = new OrderMaster();
                    if (base.op == CodeMaster.BarCodeType.ORD.ToString())
                    {
                        orderMaster = this.smartDeviceService.GetOrder(base.barCode, true);
                        //if (orderMaster.OrderStrategy != FlowStrategy.KIT)
                        //{
                        //    throw new BusinessException("该订单不是KIT单，请扫描KIT");
                        //}
                        if (orderMaster.Status != OrderStatus.Close)
                        {
                            throw new BusinessException("KIT单{0}未关闭", base.barCode);
                        }
                        //orderMaster.OrderDetails = orderMaster.OrderDetails.Where(o => o.IsScanHu == true).ToArray();
                        this.lblMessage.Text = "请扫描关键件条码";
                        this.orderMaster = orderMaster;
                        this.gvListDataBind();
                    }
                    else
                    {
                        throw new BusinessException("请先扫描KIT单");
                    }
                }
                else
                {

                    if (base.barCode.Length == 17 && Utility.IsValidateLotNo(base.barCode.Substring(9, 4)) == true)
                    {
                        base.op = CodeMaster.BarCodeType.HU.ToString();
                    }
                    if (base.op == CodeMaster.BarCodeType.HU.ToString())
                    {
                        if (this.orderMaster == null)
                        {
                            throw new BusinessException("请先扫描KIT单");
                        }
                        Hu hu = new Hu();
                        try
                        {
                            hu = this.smartDeviceService.GetHu(this.barCode);
                        }
                        catch
                        {
                            if (this.barCode.Length == 17)
                            {
                                hu = this.smartDeviceService.ResolveHu(this.barCode, this.user.Code);
                            }
                        }
                        if (string.IsNullOrEmpty(hu.HuId))
                        {
                            throw new BusinessException("条码没有找到。");
                        }
                        
                        if (this.orderMaster.OrderDetails.Where(od => od.Item != hu.Item && od.IsScanHu == true).Count()>0)
                        {
                            throw new BusinessException("条码{0}不是KIT单{1}所需要的关键件", hu.HuId, this.orderMaster.OrderNo);
                        }
                        
                        OrderDetail orderDetail = this.orderMaster.OrderDetails.FirstOrDefault(o => o.Item == hu.Item);
                        if (orderDetail.QualityType != hu.QualityType)
                        {
                            throw new BusinessException("条码的质量不满足需求");
                        }

                        if (hu.Status == HuStatus.Location)
                        {
                            throw new BusinessException("条码{0}已经在库位中", base.barCode);
                        }
                        orderDetail.IsScanHu = false;
                        this.hus.Add(hu);
                        this.gvListDataBind();
                        //this.MatchHu(hu);
                    }
                    else
                    {
                        throw new BusinessException("条码格式不合法");
                    }
                }
            }
            else
            {
                if (base.op == CodeMaster.BarCodeType.HU.ToString())
                {
                    if(this.hus.All(h=>h.HuId != base.barCode))
                    {
                        throw new BusinessException("该条码未扫入不需要取消");
                    }
                    Hu hu = this.hus.FirstOrDefault(h => h.HuId == base.barCode);

                    OrderDetail orderDetail = this.orderMaster.OrderDetails.FirstOrDefault(o => o.Item == hu.Item);

                    orderDetail.IsScanHu = true;
                    this.hus.Add(hu);
                    this.gvListDataBind();
                }
                else
                {
                    throw new BusinessException("条码格式不合法");
                }
            }
        }

        protected override void gvListDataBind()
        {
            this.tbBarCode.Text = string.Empty;
            //this.tbBarCode.Focus();
            this.ts = new DataGridTableStyle();

            this.ts.GridColumnStyles.Add(base.columnItemCode);
            this.ts.GridColumnStyles.Add(base.columnOrderedQty);
            this.ts.GridColumnStyles.Add(base.columnLotNo);
            this.ts.GridColumnStyles.Add(base.columnIsOdd);
            this.ts.GridColumnStyles.Add(base.columnUnitCount);
            this.ts.GridColumnStyles.Add(base.columnUom);
            this.ts.GridColumnStyles.Add(base.columnReferenceItemCode);
            this.ts.GridColumnStyles.Add(base.columnItemDescription);

            this.dgList.TableStyles.Clear();
            this.dgList.TableStyles.Add(this.ts);

            this.ResumeLayout();
            this.isMasterBind = true;

            List<OrderDetail> orderDetailList = new List<OrderDetail>();
            if (this.orderMaster != null)
            {
                orderDetailList = this.orderMaster.OrderDetails.Where(od => od.IsScanHu == true).ToList();
            }
            base.dgList.DataSource = orderDetailList;
            base.ts.MappingName = orderDetailList.GetType().Name;
        }

        protected override void Reset()
        {
            this.orderMaster = null;
            //this.orderMasters = new List<OrderMaster>();
            base.Reset();
            //this.effDate = null;
        }

        protected override void DoSubmit()
        {
            if (this.orderMaster == null)
            {
                throw new BusinessException("请先扫描KIT单。");
            }

            if (this.orderMaster.OrderDetails.Any(od => od.IsScanHu == true))
            {
                throw new BusinessException("请扫描全部关键件");
            }

            string[][] huDetails = new string[hus.Count][];
            for (int i = 0; i < hus.Count; i++)
            {
                huDetails[i] = new string[2];
                huDetails[i][0] = hus[i].HuId;
                huDetails[i][1] = hus[i].Item;
            }

            try
            {
                //this.smartDeviceService.DoKitOrderScanKeyPart(huDetails, this.orderMaster.OrderNo, this.user.Code);
                this.Reset();
                this.lblMessage.Text = "关键件匹配成功";
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
            Hu firstHu = base.DoCancel();
            this.CancelHu(firstHu);
            return firstHu;
        }

        private void CancelHu(Hu hu)
        {
            if (this.hus == null)
            {
                this.Reset();
            }
            if (hu != null)
            {
                base.hus = base.hus.Where(h => !h.HuId.Equals(hu.HuId, StringComparison.OrdinalIgnoreCase)).ToList();
                this.gvHuListDataBind();
            }
        }
    }
}
