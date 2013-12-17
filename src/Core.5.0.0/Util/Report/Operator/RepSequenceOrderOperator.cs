using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepSequenceOrderOperator: RepTemplate1
    {
        public RepSequenceOrderOperator()
        {
            //明细部分的行数
            this.pageDetailRowCount = 30;
            //列数   1起始
            this.columnCount = 12;
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

                PrintOrderMaster sequenceMaster = (PrintOrderMaster)(list[0]);
                IList<PrintOrderDetail> sequenceDetails = (IList<PrintOrderDetail>)(list[1]);

                sequenceDetails = sequenceDetails.OrderBy(o => o.Sequence).ThenBy(o => o.Item).ToList();

                if (sequenceMaster == null
                    || sequenceDetails == null || sequenceDetails.Count == 0)
                {
                    return false;
                }


                //this.SetRowCellBarCode(0, 2, 7);
                this.barCodeFontName = this.GetBarcodeFontName(1,8);
                this.CopyPage(sequenceDetails.Count);

                this.FillHead(sequenceMaster);


                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;
                foreach (PrintOrderDetail sequenceDetail in sequenceDetails)
                {
                    // 顺序号.	
                    this.SetRowCell(pageIndex, rowIndex, 0, sequenceDetail.Sequence>0?sequenceDetail.Sequence.ToString():string.Empty);
                    
                    //工位
                    this.SetRowCell(pageIndex, rowIndex, 1, sequenceDetail.BinTo);

                    //零件号 Item Code
                    this.SetRowCell(pageIndex, rowIndex, 2, sequenceDetail.Item);

                    //旧图号
                    this.SetRowCell(pageIndex, rowIndex, 3, sequenceDetail.ReferenceItemCode);

                    //零件描述
                    this.SetRowCell(pageIndex, rowIndex, 4, sequenceDetail.ItemDescription);

                    //预计消耗时间
                    this.SetRowCell(pageIndex, rowIndex, 6, sequenceDetail.StartDateFormat);

                    ////单位Unit
                    //this.SetRowCell(pageIndex, rowIndex, 7, sequenceDetail.Uom);

                    //订单数
                    this.SetRowCell(pageIndex, rowIndex, 8, sequenceDetail.OrderedQty>0?sequenceDetail.OrderedQty.ToString("0.########"):string.Empty);

                    ////发货数
                    //this.SetRowCell(pageIndex, rowIndex, 9, sequenceDetail.ShippedQty>0?sequenceDetail.ShippedQty.ToString("0.########"):string.Empty);

                    //VAN
                    this.SetRowCell(pageIndex, rowIndex, 10, sequenceDetail.ReserveNo);
                    this.SetRowCell(pageIndex, rowIndex, 11, sequenceDetail.ZENGINE);

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
            //排序单号:
            string seqCode = Utility.BarcodeHelper.GetBarcodeStr(sequenceMaster.OrderNo, this.barCodeFontName);
            this.SetRowCell(1, 7, seqCode);
            //排序单号 No.:
            this.SetRowCell(2, 7, sequenceMaster.OrderNo);

            //来源组织
            this.SetRowCell(3, 2, sequenceMaster.PartyFrom);

            //线体
            this.SetRowCell(2, 4, sequenceMaster.SequenceGroup.Substring(sequenceMaster.SequenceGroup.Length-1));

            //来源库位
            this.SetRowCell(3, 4, sequenceMaster.LocationFrom);

            //发出时间 Create Time:
            this.SetRowCell(3, 7, sequenceMaster.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));

            ////目的组织
            //this.SetRowCell(4, 2, sequenceMaster.PartyTo);

            //排序组
            this.SetRowCell(4, 4, sequenceMaster.FlowDescription);
            //拨次
            this.SetRowCell(4, 2, sequenceMaster.TraceCode);

            //窗口时间 
            this.SetRowCell(4, 7, sequenceMaster.WindowTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /**
           * 需要拷贝的数据与合并单元格操作
           * 
           * Param pageIndex 页号
           */
        public override void CopyPageValues(int pageIndex)
        {
            this.CopyCell(pageIndex, 37, 0, "A38");
            this.CopyCell(pageIndex, 37, 7, "H38");
            this.SetMergedRegion(pageIndex, 37, 0, 37, 1);
            this.SetMergedRegion(pageIndex, 37, 7, 37, 8);


            this.SetMergedRegion(pageIndex, 7, 4, 7, 5);
            this.SetMergedRegion(pageIndex, 8, 4, 8, 5);
            this.SetMergedRegion(pageIndex, 9, 4, 9, 5);
            this.SetMergedRegion(pageIndex, 10, 4, 10, 5);
            this.SetMergedRegion(pageIndex, 11, 4, 11, 5);
            this.SetMergedRegion(pageIndex, 12, 4, 12, 5);
            this.SetMergedRegion(pageIndex, 13, 4, 13, 5);
            this.SetMergedRegion(pageIndex, 14, 4, 14, 5);
            this.SetMergedRegion(pageIndex, 15, 4, 15, 5);
            this.SetMergedRegion(pageIndex, 16, 4, 16, 5);
            this.SetMergedRegion(pageIndex, 17, 4, 17, 5);
            this.SetMergedRegion(pageIndex, 18, 4, 18, 5);
            this.SetMergedRegion(pageIndex, 19, 4, 19, 5);
            this.SetMergedRegion(pageIndex, 20, 4, 20, 5);
            this.SetMergedRegion(pageIndex, 21, 4, 21, 5);
            this.SetMergedRegion(pageIndex, 22, 4, 22, 5);
            this.SetMergedRegion(pageIndex, 23, 4, 23, 5);
            this.SetMergedRegion(pageIndex, 24, 4, 24, 5);
            this.SetMergedRegion(pageIndex, 25, 4, 25, 5);
            this.SetMergedRegion(pageIndex, 26, 4, 26, 5);
            this.SetMergedRegion(pageIndex, 27, 4, 27, 5);
            this.SetMergedRegion(pageIndex, 28, 4, 28, 5);
            this.SetMergedRegion(pageIndex, 29, 4, 29, 5);
            this.SetMergedRegion(pageIndex, 30, 4, 30, 5);
            this.SetMergedRegion(pageIndex, 31, 4, 31, 5);
            this.SetMergedRegion(pageIndex, 32, 4, 32, 5);
            this.SetMergedRegion(pageIndex, 33, 4, 33, 5);
            this.SetMergedRegion(pageIndex, 34, 4, 34, 5);
            this.SetMergedRegion(pageIndex, 35, 4, 35, 5);
            this.SetMergedRegion(pageIndex, 36, 4, 36, 5);

            this.SetMergedRegion(pageIndex, 7, 6, 7, 7);
            this.SetMergedRegion(pageIndex, 8, 6, 8, 7);
            this.SetMergedRegion(pageIndex, 9, 6, 9, 7);
            this.SetMergedRegion(pageIndex, 10, 6, 10, 7);
            this.SetMergedRegion(pageIndex, 11, 6, 11, 7);
            this.SetMergedRegion(pageIndex, 12, 6, 12, 7);
            this.SetMergedRegion(pageIndex, 13, 6, 13, 7);
            this.SetMergedRegion(pageIndex, 14, 6, 14, 7);
            this.SetMergedRegion(pageIndex, 15, 6, 15, 7);
            this.SetMergedRegion(pageIndex, 16, 6, 16, 7);
            this.SetMergedRegion(pageIndex, 17, 6, 17, 7);
            this.SetMergedRegion(pageIndex, 18, 6, 18, 7);
            this.SetMergedRegion(pageIndex, 19, 6, 19, 7);
            this.SetMergedRegion(pageIndex, 20, 6, 20, 7);
            this.SetMergedRegion(pageIndex, 21, 6, 21, 7);
            this.SetMergedRegion(pageIndex, 22, 6, 22, 7);
            this.SetMergedRegion(pageIndex, 23, 6, 23, 7);
            this.SetMergedRegion(pageIndex, 24, 6, 24, 7);
            this.SetMergedRegion(pageIndex, 25, 6, 25, 7);
            this.SetMergedRegion(pageIndex, 26, 6, 26, 7);
            this.SetMergedRegion(pageIndex, 27, 6, 27, 7);
            this.SetMergedRegion(pageIndex, 28, 6, 28, 7);
            this.SetMergedRegion(pageIndex, 29, 6, 29, 7);
            this.SetMergedRegion(pageIndex, 30, 6, 30, 7);
            this.SetMergedRegion(pageIndex, 31, 6, 31, 7);
            this.SetMergedRegion(pageIndex, 32, 6, 32, 7);
            this.SetMergedRegion(pageIndex, 33, 6, 33, 7);
            this.SetMergedRegion(pageIndex, 34, 6, 34, 7);
            this.SetMergedRegion(pageIndex, 35, 6, 35, 7);
            this.SetMergedRegion(pageIndex, 36, 6, 36, 7);
        }


    }
}
