using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepReceiptOrderOperator : RepTemplate1
    {
        public RepReceiptOrderOperator()
        {
            //明细部分的行数
            this.pageDetailRowCount = 35;
            //列数   1起始
            this.columnCount = 10;
            //报表头的行数  1起始
            this.headRowCount = 11;
            //报表尾的行数  1起始
            this.bottomRowCount = 1;
        }

        /**
         * 填充报表
         * 
         * Param list [0]Receipt
         *            [1]ReceiptDetailList
         */
        protected override bool FillValuesImpl(String templateFileName, IList<object> list)
        {
            try
            {
                if (list == null || list.Count < 2) return false;

                PrintReceiptMaster receiptMaster = (PrintReceiptMaster)list[0];
                IList<PrintReceiptDetail> receiptDetailList = (IList<PrintReceiptDetail>)list[1];

                if (receiptMaster == null
                    || receiptDetailList == null || receiptDetailList.Count == 0)
                {
                    return false;
                }

                this.barCodeFontName = this.GetBarcodeFontName(2, 5);
                //this.SetRowCellBarCode(0, 2, 5);
                this.CopyPage(receiptDetailList.Count);

                this.FillHead(receiptMaster, receiptDetailList[0]);

                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;
                receiptDetailList = receiptDetailList.OrderBy(r => r.Sequence)
                    .ThenBy(r => r.Item).ToList();
                foreach (PrintReceiptDetail receiptDetail in receiptDetailList)
                {
                    this.SetRowCell(pageIndex, rowIndex, 0, receiptDetail.Item);
                    
                    this.SetRowCell(pageIndex, rowIndex, 1, receiptDetail.ManufactureParty);
                    
                    this.SetRowCell(pageIndex, rowIndex, 2, receiptDetail.ReferenceItemCode);
                    
                    this.SetRowCell(pageIndex, rowIndex, 3, receiptDetail.ItemDescription);

                    this.SetRowCell(pageIndex, rowIndex, 4, receiptDetail.Uom);
                    //单包装
                    this.SetRowCell(pageIndex, rowIndex, 5, receiptDetail.UnitCount.ToString("0.########"));

                    //目的库位
                    //decimal shippedQty = receiptDetail.
                    this.SetRowCell(pageIndex, rowIndex, 6, receiptDetail.LocationTo);

                    //实收数	数量
                    decimal receivedQty = receiptDetail.ReceivedQty;
                    this.SetRowCell(pageIndex, rowIndex, 8, receivedQty.ToString("0.########"));
                    //实收数  包装
                    decimal UC = receiptDetail.UnitCount > 0 ? receiptDetail.UnitCount : 1;
                    int UCs = (int)Math.Ceiling(receivedQty / UC);
                    this.SetRowCell(pageIndex, rowIndex, 9, UCs.ToString());

                    if (this.isPageBottom(rowIndex, rowTotal))//页的最后一行
                    {
                        //实际到货时间:
                        //this.SetRowCell(pageIndex, rowIndex, , "");

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
         * Param pageIndex 页号
         * Param orderHead 订单头对象
         * Param orderDetails 订单明细对象
         */
        private void FillHead(PrintReceiptMaster receiptMaster, PrintReceiptDetail receiptDetail)
        {

            //if (Receipt.ReceiptDetails[0].OrderLocationTransaction.OrderDetail.OrderHead.SubType == BusinessConstants.CODE_MASTER_ORDER_SUB_TYPE_VALUE_RTN)
            //{
            //    this.SetRowCell(2, 5, "退货");
            //}

            string receiptCode = Utility.BarcodeHelper.GetBarcodeStr(receiptMaster.ReceiptNo, this.barCodeFontName);

            //收货单号:
            this.SetRowCell(2, 5, receiptCode);

            this.SetRowCell(3, 5, receiptMaster.ReceiptNo);

            this.SetRowCell(4, 1, receiptMaster.PartyFrom);

            this.SetRowCell(4, 5, receiptMaster.PartyTo);

            this.SetRowCell(5, 1, receiptMaster.ExternalReceiptNo);

            this.SetRowCell(5, 5, receiptMaster.IpNo);

            this.SetRowCell(6, 1, receiptDetail.LocationFrom);

            this.SetRowCell(6, 5, receiptMaster.CreateDate.ToString("yyyy-MM-dd HH:mm"));
        }

        /**
           * 需要拷贝的数据与合并单元格操作
           * 
           * Param pageIndex 页号
           */
        public override void CopyPageValues(int pageIndex)
        {
            //实际收货时间: 
            this.CopyCell(pageIndex, 41, 0, "A42");
            //收货人签字:
            this.CopyCell(pageIndex, 41, 3, "D42");
            //承运商签字:	
            this.CopyCell(pageIndex, 41, 6, "G42");
        }
    }
}
