using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepComposeVanNoOperator: RepTemplate1
    {
        public RepComposeVanNoOperator()
        {
            //明细部分的行数
            this.pageDetailRowCount = 0;
            //列数   1起始
            this.columnCount = 6;
            //报表头的行数  1起始
            this.headRowCount = 2;
            //报表尾的行数  1起始
            this.bottomRowCount = 0;
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

                PrintOrderMaster sequenceMaster = (PrintOrderMaster)(list[0]);
                IList<PrintOrderDetail> sequenceDetails = (IList<PrintOrderDetail>)(list[1]);

                //sequenceDetails = sequenceDetails.OrderBy(o =>o.Item).ToList();

                if (sequenceMaster == null
                    || sequenceDetails == null || sequenceDetails.Count == 0)
                {
                    return false;
                }


                //this.SetRowCellBarCode(0, 2, 8);
                this.barCodeFontName = this.GetBarcodeFontName(1,8);
                this.CopyPage(sequenceDetails.Count);

                this.FillHead(sequenceMaster);


                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;
                foreach (PrintOrderDetail sequenceDetail in sequenceDetails)
                {

                    //零件号 Item Code
                    this.SetRowCell(pageIndex, rowIndex, 0, sequenceDetail.Item);

                    //旧图号
                    this.SetRowCell(pageIndex, rowIndex, 1, sequenceDetail.ReferenceItemCode);

                    //零件描述
                    this.SetRowCell(pageIndex, rowIndex, 2, sequenceDetail.ItemDescription);

                    //制造商
                    this.SetRowCell(pageIndex, rowIndex, 4, sequenceDetail.ManufactureParty);

                    //寄售供应商
                    this.SetRowCell(pageIndex, rowIndex, 5, sequenceDetail.ICHARG);


                    //订单数
                    this.SetRowCell(pageIndex, rowIndex, 6, sequenceDetail.OrderedQty>0?sequenceDetail.OrderedQty.ToString("0.########"):string.Empty);

                    ////发货数
                    //this.SetRowCell(pageIndex, rowIndex, 6, sequenceDetail.ShippedQty>0?sequenceDetail.ShippedQty.ToString("0.########"):string.Empty);


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
        private void FillHead(PrintOrderMaster sequenceMaster)
        {
            //排序单号 No.:
            this.SetRowCell(2, 5, sequenceMaster.OrderNo);
        }

        /**
           * 需要拷贝的数据与合并单元格操作
           * 
           * Param pageIndex 页号
           */
        public override void CopyPageValues(int pageIndex)
        {
            //this.CopyCell(pageIndex, 37, 0, "A38");
            //this.CopyCell(pageIndex, 37, 4, "E38");
            //this.CopyCell(pageIndex, 37, 8, "I38");
        }


    }
}
