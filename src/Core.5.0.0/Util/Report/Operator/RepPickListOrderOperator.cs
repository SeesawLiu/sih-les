using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepPickListOrderOperator : RepTemplate1
    {
        public RepPickListOrderOperator()
        {
            //明细部分的行数
            this.pageDetailRowCount = 40;
            //列数   1起始
            this.columnCount = 11;
            //报表头的行数  1起始
            this.headRowCount = 7;
            //报表尾的行数  1起始
            this.bottomRowCount = 0;
        }

        /**
         * 填充报表
         * 
         * Param list [0]PickList
         *            
         */
        protected override bool FillValuesImpl(String templateFileName, IList<object> list)
        {
            try
            {
                PrintPickListMaster pickListMaster = (PrintPickListMaster)list[0];
                IList<PrintPickListDetail> pickListDetails = pickListMaster.PickListDetails.OrderByDescending(p => p.IsInventory).ThenBy(p => p.Bin).ThenBy(p => p.Item).ToList();

                if (pickListMaster == null
                    || pickListDetails == null || pickListDetails.Count == 0)
                {
                    return false;
                }

                //this.barCodeFontName = this.GetBarcodeFontName(0, 7);
                //this.SetRowCellBarCode(0, 0, 7);
                this.CopyPage(pickListDetails.Count);
              //  this.barCodeFontName = this.GetBarcodeFontName(2, 8);
                this.barCodeFontName = "Code 128";
                this.FillHead(pickListMaster);


                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;
                foreach (PrintPickListDetail pickListDetail in pickListDetails)
                {
                    this.SetRowCell(pageIndex, rowIndex, 0, pickListDetail.Sequence);

                    this.SetRowCell(pageIndex, rowIndex, 1, pickListDetail.Item);

                    this.SetRowCell(pageIndex, rowIndex, 2, pickListDetail.ReferenceItemCode);

                    this.SetRowCell(pageIndex, rowIndex, 3, pickListDetail.ItemDescription);

                    this.SetRowCell(pageIndex, rowIndex, 4, pickListDetail.ManufactureParty);

                    this.SetRowCell(pageIndex, rowIndex, 5, pickListDetail.ConsignmentSupplier);

                    this.SetRowCell(pageIndex, rowIndex, 6, pickListDetail.Qty.ToString("0.00"));

                    this.SetRowCell(pageIndex, rowIndex, 7, pickListDetail.LocationTo);

                    this.SetRowCell(pageIndex, rowIndex, 8, pickListDetail.Bin);

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
         * Param pickList 订单头对象
         */
        private void FillHead(PrintPickListMaster pickListMaster)
        {
            

            //订单号:
            string orderCode = Utility.BarcodeHelper.GetBarcodeStr(pickListMaster.PickListNo, this.barCodeFontName);
            this.SetRowCell(2, 7, orderCode);
            //Order No.:
            this.SetRowCell(3, 7, pickListMaster.PickListNo);

            //if (pickListMaster.PrintPickListDetails == null
            //        || pickListMaster.PrintPickListDetails[0] == null
            //        || pickListMaster.PrintPickListDetails[0]. == null
            //        || pickListMaster.PrintPickListDetails[0].o.OrderDetail == null
            //        || pickListMaster.PrintPickListDetails[0].OrderLocationTransaction.OrderDetail.OrderHead == null
            //        || "Normal".Equals(pickListMaster.PrintPickListDetails[0].OrderLocationTransaction.OrderDetail.OrderHead.Priority)) 
            //{
            //    this.SetRowCell(2, 4, "");
            //}
            //else
            //{
            //    this.SetRowCell(1, 4, "");
            //}

            //源超市：
            //if (pickListMaster != null )//&& pickListMaster.Flow.Trim() != string.Empty)
            //{
            //    //Flow flow = this.flowMgrE.LoadFlow(pickList.Flow);
            //    this.SetRowCell(2, 2, flow.LocationFrom == null ? string.Empty : flow.LocationFrom.Code);
            //    //目的超市：
            //    this.SetRowCell(3, 2, pickList.Flow);
            //    //领料地点：
            //    this.SetRowCell(4, 2, flow.LocationFrom == null ? string.Empty : flow.LocationFrom.Region.Code);
            //}

            //窗口时间
            //    this.SetRowCell(2, 8, pickListMaster.WindowTime.ToString("yyyy-MM-dd HH:mm"));
            //订单时间
                this.SetRowCell(4, 7, pickListMaster.CreateDate.ToString("yyyy-MM-dd HH:mm"));
        }

        /**
           * 需要拷贝的数据与合并单元格操作
           * 
           * Param pageIndex 页号
           */
        public override void CopyPageValues(int pageIndex)
        {
            this.SetMergedRegion(pageIndex, 47, 3, 47, 4);
            this.SetMergedRegion(pageIndex, 47, 7, 47, 8);

            this.CopyCell(pageIndex, 47, 2, "C48");
            this.CopyCell(pageIndex, 47, 7, "H48");
            
        }

    }
}
