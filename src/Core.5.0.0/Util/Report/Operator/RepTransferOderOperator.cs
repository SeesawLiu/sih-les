using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;
using com.Sconit.Entity;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepTransferOderOperator : RepTemplate1
    {
        public RepTransferOderOperator()
        {
            //明细部分的行数
            this.pageDetailRowCount = 35;
            //列数   1起始
            this.columnCount = 12;
            //报表头的行数  1起始
            this.headRowCount = 15;
            //报表尾的行数  1起始
            this.bottomRowCount = 2;
        }

        /**
         * 填充报表
         * 
         * Param list [0]OrderHead
         * Param list [0]IList<OrderDetail>           
         */
        protected override bool FillValuesImpl(String templateFileName, IList<object> list)
        {
            try
            {
                if (list == null || list.Count < 2) return false;

                PrintOrderMaster orderMaster = (PrintOrderMaster)(list[0]);
                IList<PrintOrderDetail> orderDetails = (IList<PrintOrderDetail>)(list[1]);

                orderDetails = orderDetails.OrderBy(o => o.Sequence).ThenBy(o => o.Item).ToList();

                if (orderMaster == null
                    || orderDetails == null || orderDetails.Count == 0)
                {
                    return false;
                }

                this.barCodeFontName = this.GetBarcodeFontName(2, 8);
                this.CopyPage(orderDetails.Count);

                this.FillHead(orderMaster);


                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;
                foreach (PrintOrderDetail orderDetail in orderDetails)
                {
                    // No.	
                    this.SetRowCell(pageIndex, rowIndex, 0, "" + orderDetail.Sequence);

                    //零件号 Item Code
                    this.SetRowCell(pageIndex, rowIndex, 1, orderDetail.Item);

                    //参考号 Ref No.
                    this.SetRowCell(pageIndex, rowIndex, 2, orderDetail.ReferenceItemCode);

                    //描述Description
                    this.SetRowCell(pageIndex, rowIndex, 3, orderDetail.ItemDescription);

                    //单位Unit
                    this.SetRowCell(pageIndex, rowIndex, 4, orderDetail.Uom);

                    //单包装UC
                    this.SetRowCell(pageIndex, rowIndex, 5, orderDetail.UnitCount.ToString("0.########"));

                    //需求 Request	包装
                    if (orderDetail.UnitCount > 0)
                    {
                        int UCs = (int)Math.Ceiling(orderDetail.OrderedQty / orderDetail.UnitCount);
                        this.SetRowCell(pageIndex, rowIndex, 6, UCs.ToString());
                    }
                    else
                    {
                        this.SetRowCell(pageIndex, rowIndex, 6, 0);
                    }

                    //需求 Request	零件数
                    this.SetRowCell(pageIndex, rowIndex, 7, orderDetail.OrderedQty.ToString("0.########"));

                    //发货数
                    this.SetRowCell(pageIndex, rowIndex, 8,  orderDetail.ShippedQty.ToString("0.########") );

                    //实收 Received	包装
                    //this.SetRowCell(pageIndex, rowIndex, 9, "");

                    //实收 Received	零件数
                    this.SetRowCell(pageIndex, rowIndex, 10,  orderDetail.ReceivedQty.ToString("0.########") );

                    //批号/备注
                    //this.SetRowCell(pageIndex, rowIndex, 11, orderDetail.Remark);

                    if (this.isPageBottom(rowIndex, rowTotal))//页的最后一行
                    {
                        pageIndex++;
                        rowIndex = 0;
                    }
                    else
                    {
                        rowIndex++;
                    }
                    rowTotal++;
                }

                this.sheet.DisplayGridlines = false;
                this.sheet.IsPrintGridlines = false;

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        /*
         * 填充报表头
         * 
         * Param repack 报验单头对象
         */
        private void FillHead(PrintOrderMaster orderMaster)
        {
            //订单号:
            string orderCode = Utility.BarcodeHelper.GetBarcodeStr(orderMaster.OrderNo, this.barCodeFontName);
            this.SetRowCell(2, 8, orderCode);
            //Order No.:
            this.SetRowCell(3, 8, orderMaster.OrderNo);

            if (orderMaster.SubType == (short)CodeMaster.OrderSubType.Return)
            {
                this.SetRowCell(0, 3, "退货");
            }

            if (orderMaster.Priority == (short)CodeMaster.OrderPriority.Normal)
            {
                this.SetRowCell(4, 5, "");
            }
            else
            {
                this.SetRowCell(3, 5, "");
            }

            //制单时间 Create Time:
            this.SetRowCell(4, 8, orderMaster.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));

            //供应商代码 Supplier Code:	
            this.SetRowCell(6, 3, orderMaster.PartyFrom != null ? orderMaster.PartyFrom : String.Empty);
            //开始时间 Start Time:
            this.SetRowCell(6, 8, orderMaster.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));


            //供应商名称 Supplier Name:		
            this.SetRowCell(7, 3, orderMaster.PartyFrom != null ? orderMaster.PartyFromName : String.Empty);
            //窗口时间 Window Time:
            this.SetRowCell(7, 8, orderMaster.WindowTime.ToString("yyyy-MM-dd HH:mm:ss"));

            //供应商地址 Address:	
            this.SetRowCell(8, 3, orderMaster.ShipFrom != null ? orderMaster.ShipFromAddr : String.Empty);
            //交货道口 Delivery Dock:
            this.SetRowCell(8, 8, orderMaster.Dock != null ? orderMaster.Dock : string.Empty);

            //供应商联系人 Contact:	
            this.SetRowCell(9, 3, orderMaster.ShipFrom != null ? orderMaster.ShipFromContact : String.Empty);
            //物流协调员 Follow Up:
            this.SetRowCell(9, 8, orderMaster.ShipTo != null ? orderMaster.ShipToContact : String.Empty);

            //供应商电话 Telephone:		
            this.SetRowCell(10, 3, orderMaster.ShipFrom != null ? orderMaster.ShipFromTel : String.Empty);
            //YFV电话 Telephone:
            this.SetRowCell(10, 8, orderMaster.ShipTo != null ? orderMaster.ShipToTel : String.Empty);

            //来源库位:	
            this.SetRowCell(11, 3, orderMaster.PartyFrom != null ? orderMaster.PartyFrom + "@" + orderMaster.PartyFromName : String.Empty);
            //目的库位:
            this.SetRowCell(11, 8, orderMaster.PartyTo != null ? orderMaster.PartyTo + "@" + orderMaster.PartyToName : String.Empty);


            //系统号 SysCode:
            //this.SetRowCell(++rowNum, 3, "");
            //版本号 Version:
            //this.SetRowCell(rowNum, 8, "");
        }

        /**
           * 需要拷贝的数据与合并单元格操作
           * 
           * Param pageIndex 页号
           */
        public override void CopyPageValues(int pageIndex)
        {
            this.CopyCell(pageIndex, 50, 1, "B51");
            this.CopyCell(pageIndex, 50, 5, "F51");
            this.CopyCell(pageIndex, 50, 9, "J51");
            this.CopyCell(pageIndex, 51, 0, "A52");
        }

    }
}
