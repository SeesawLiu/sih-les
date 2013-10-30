using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepKitOrderOperator: RepTemplate1
    {
        public RepKitOrderOperator()
        {
            //明细部分的行数
            this.pageDetailRowCount = 30;
            //列数   1起始
            this.columnCount = 13;
            //报表头的行数  1起始
            this.headRowCount = 7;
            //报表尾的行数  1起始
            this.bottomRowCount = 1;
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

                if (orderMaster == null
                    || orderDetails == null || orderDetails.Count == 0)
                {
                    return false;
                }


                //this.SetRowCellBarCode(0, 2, 8);
                this.barCodeFontName = this.GetBarcodeFontName(1,4);
                this.CopyPage(orderDetails.Count);

                this.FillHead(orderMaster);


                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;
                string reserveNo = string.Empty;
                foreach (PrintOrderDetail orderDetail in orderDetails)
                {
                    if (orderDetail.ReserveNo == reserveNo)
                    {
                        orderDetail.ReserveNo = string.Empty;
                    }
                    else
                    {
                        reserveNo = orderDetail.ReserveNo;
                    }

                    this.SetRowCell(pageIndex, rowIndex, 0, orderDetail.ReserveNo);

                    this.SetRowCell(pageIndex, rowIndex, 1, orderDetail.ZOPID);
                    
                    this.SetRowCell(pageIndex, rowIndex, 2, orderDetail.ZOPDS);

                    this.SetRowCell(pageIndex, rowIndex, 3, orderDetail.Item);

                    this.SetRowCell(pageIndex, rowIndex, 5, orderDetail.ReferenceItemCode);

                    this.SetRowCell(pageIndex, rowIndex, 6, orderDetail.ItemDescription);

                    this.SetRowCell(pageIndex, rowIndex, 9, orderDetail.ManufactureParty);

                    this.SetRowCell(pageIndex, rowIndex, 11,orderDetail.OrderedQty>0? orderDetail.OrderedQty.ToString("0.########"):string.Empty);

                    this.SetRowCell(pageIndex, rowIndex, 12, orderDetail.IsScanHu ? "√" : string.Empty);

                    //if (orderDetail.IsScanHu == true)
                    //{
                    //    this.SetRowCell(pageIndex, rowIndex, 10, "√");
                    //}

                    //批号/备注
                   // this.SetRowCell(pageIndex, rowIndex, 11, "");

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
            //顺序号:
            this.SetRowCell(0, 6, orderMaster.Sequence.ToString());
            //订单号:
            if (!string.IsNullOrEmpty(orderMaster.TraceCode))
            {
                string vanCode = Utility.BarcodeHelper.GetBarcodeStr(orderMaster.TraceCode, this.barCodeFontName);
                this.SetRowCell(1, 4, vanCode);
            }
            //Order No.:
            this.SetRowCell(2, 4, orderMaster.TraceCode);

            //订单号:
            string orderCode = Utility.BarcodeHelper.GetBarcodeStr(orderMaster.OrderNo, this.barCodeFontName);
            this.SetRowCell(1, 8, orderCode);
            //Order No.:
            this.SetRowCell(2, 8, orderMaster.OrderNo);

            this.SetRowCell(3, 3, orderMaster.Flow+"["+orderMaster.FlowDescription+"]");

            //线体
            this.SetRowCell(3, 6, orderMaster.SequenceGroup.Substring(orderMaster.SequenceGroup.Length - 1));
  
            //来源库位
           // this.SetRowCell(3, 5, orderMaster.OrderDetails[0].LocationFrom);

            //发出时间 Create Time:
            this.SetRowCell(3, 9, orderMaster.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));


            //目的库位
            //this.SetRowCell(4, 5, orderMaster.OrderDetails[0].LocationTo);

            //窗口时间 
            this.SetRowCell(4, 9, orderMaster.WindowTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /**
           * 需要拷贝的数据与合并单元格操作
           * 
           * Param pageIndex 页号
           */
        public override void CopyPageValues(int pageIndex)
        {
            this.SetMergedRegion(pageIndex, 7, 3, 7, 4);
            this.SetMergedRegion(pageIndex, 8, 3, 8, 4);
            this.SetMergedRegion(pageIndex, 9, 3, 9, 4);
            this.SetMergedRegion(pageIndex, 10, 3, 10, 4);
            this.SetMergedRegion(pageIndex, 11, 3, 11, 4);
            this.SetMergedRegion(pageIndex, 12, 3, 12, 4);
            this.SetMergedRegion(pageIndex, 13, 3, 13, 4);
            this.SetMergedRegion(pageIndex, 14, 3, 14, 4);
            this.SetMergedRegion(pageIndex, 15, 3, 15, 4);
            this.SetMergedRegion(pageIndex, 16, 3, 16, 4);
            this.SetMergedRegion(pageIndex, 17, 3, 17, 4);
            this.SetMergedRegion(pageIndex, 18, 3, 18, 4);
            this.SetMergedRegion(pageIndex, 19, 3, 19, 4);
            this.SetMergedRegion(pageIndex, 20, 3, 20, 4);
            this.SetMergedRegion(pageIndex, 21, 3, 21, 4);
            this.SetMergedRegion(pageIndex, 22, 3, 22, 4);
            this.SetMergedRegion(pageIndex, 23, 3, 23, 4);
            this.SetMergedRegion(pageIndex, 24, 3, 24, 4);
            this.SetMergedRegion(pageIndex, 25, 3, 25, 4);
            this.SetMergedRegion(pageIndex, 26, 3, 26, 4);
            this.SetMergedRegion(pageIndex, 27, 3, 27, 4);
            this.SetMergedRegion(pageIndex, 28, 3, 28, 4);
            this.SetMergedRegion(pageIndex, 29, 3, 29, 4);
            this.SetMergedRegion(pageIndex, 30, 3, 30, 4);
            this.SetMergedRegion(pageIndex, 31, 3, 31, 4);
            this.SetMergedRegion(pageIndex, 32, 3, 32, 4);
            this.SetMergedRegion(pageIndex, 33, 3, 33, 4);
            this.SetMergedRegion(pageIndex, 34, 3, 34, 4);
            this.SetMergedRegion(pageIndex, 35, 3, 35, 4);
            this.SetMergedRegion(pageIndex, 36, 3, 36, 4);


            this.SetMergedRegion(pageIndex, 7, 6, 7, 8);
            this.SetMergedRegion(pageIndex, 8, 6, 8, 8);
            this.SetMergedRegion(pageIndex, 9, 6, 9, 8);
            this.SetMergedRegion(pageIndex, 10, 6, 10, 8);
            this.SetMergedRegion(pageIndex, 11, 6, 11, 8);
            this.SetMergedRegion(pageIndex, 12, 6, 12, 8);
            this.SetMergedRegion(pageIndex, 13, 6, 13, 8);
            this.SetMergedRegion(pageIndex, 14, 6, 14, 8);
            this.SetMergedRegion(pageIndex, 15, 6, 15, 8);
            this.SetMergedRegion(pageIndex, 16, 6, 16, 8);
            this.SetMergedRegion(pageIndex, 17, 6, 17, 8);
            this.SetMergedRegion(pageIndex, 18, 6, 18, 8);
            this.SetMergedRegion(pageIndex, 19, 6, 19, 8);
            this.SetMergedRegion(pageIndex, 20, 6, 20, 8);
            this.SetMergedRegion(pageIndex, 21, 6, 21, 8);
            this.SetMergedRegion(pageIndex, 22, 6, 22, 8);
            this.SetMergedRegion(pageIndex, 23, 6, 23, 8);
            this.SetMergedRegion(pageIndex, 24, 6, 24, 8);
            this.SetMergedRegion(pageIndex, 25, 6, 25, 8);
            this.SetMergedRegion(pageIndex, 26, 6, 26, 8);
            this.SetMergedRegion(pageIndex, 27, 6, 27, 8);
            this.SetMergedRegion(pageIndex, 28, 6, 28, 8);
            this.SetMergedRegion(pageIndex, 29, 6, 29, 8);
            this.SetMergedRegion(pageIndex, 30, 6, 30, 8);
            this.SetMergedRegion(pageIndex, 31, 6, 31, 8);
            this.SetMergedRegion(pageIndex, 32, 6, 32, 8);
            this.SetMergedRegion(pageIndex, 33, 6, 33, 8);
            this.SetMergedRegion(pageIndex, 34, 6, 34, 8);
            this.SetMergedRegion(pageIndex, 35, 6, 35, 8);
            this.SetMergedRegion(pageIndex, 36, 6, 36, 8);
            this.CopyCell(pageIndex, 37, 0, "A38");
            this.CopyCell(pageIndex, 37, 5, "F38");
            this.CopyCell(pageIndex, 37, 9, "J38");
            
            //this.CopyCell(pageIndex, 50, 1, "B51");
            //this.CopyCell(pageIndex, 50, 5, "F51");
            //this.CopyCell(pageIndex, 50, 9, "J51");
            //this.CopyCell(pageIndex, 51, 0, "A52");
           // this.SetMergedRegion(pageIndex,7, 4, 35, 6);
        }


    }
}
