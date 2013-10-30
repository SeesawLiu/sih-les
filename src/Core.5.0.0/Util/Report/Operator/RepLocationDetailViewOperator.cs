using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;
using com.Sconit.Entity.VIEW;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepLocationDetailViewOperator : RepTemplate1
    {
        public RepLocationDetailViewOperator()
        {
            //明细部分的行数
            this.pageDetailRowCount = 46;
            //列数   1起始
            this.columnCount = 11;
            //报表头的行数  1起始
            this.headRowCount = 2;
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
               // if (list == null || list.Count < 2) return false;

              //  PrintOrderMaster orderMaster = (PrintOrderMaster)(list[0]);
                IList<LocationDetailView> LocationDetailViewList = (IList<LocationDetailView>)(list[0]);

               // orderDetails = orderDetails.OrderBy(o => o.Sequence).ThenBy(o => o.Item).ToList();

                if (LocationDetailViewList == null || LocationDetailViewList.Count == 0)
                {
                    return false;
                }


                //this.SetRowCellBarCode(0, 2, 8);
               // this.barCodeFontName = this.GetBarcodeFontName(2, 8);
                this.CopyPage(LocationDetailViewList.Count);

                //this.FillHead(orderMaster);


                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;
                foreach (LocationDetailView LocationDetailView in LocationDetailViewList)
                {
                    //零件号 Item Code
                    this.SetRowCell(pageIndex, rowIndex, 0, LocationDetailView.Item);

                    //描述Description
                    this.SetRowCell(pageIndex, rowIndex, 1, LocationDetailView.ItemDescription);

                    //单位Unit
                    this.SetRowCell(pageIndex, rowIndex, 2, LocationDetailView.Uom);

                    //库位 ToString("0.########")
                    this.SetRowCell(pageIndex, rowIndex, 3, LocationDetailView.Location);
                    //供应商 
                    this.SetRowCell(pageIndex, rowIndex, 4, LocationDetailView.ManufactureParty);
                    //批号
                    this.SetRowCell(pageIndex, rowIndex, 5, LocationDetailView.LotNo);
                    //是否寄售
                    this.SetRowCell(pageIndex, rowIndex, 6, LocationDetailView.IsCS?"是":"否");
                    //数量 
                    this.SetRowCell(pageIndex, rowIndex, 7, LocationDetailView.Qty.ToString("0.00"));
                    //合格数
                    this.SetRowCell(pageIndex, rowIndex, 8, LocationDetailView.QualifyQty.ToString("0.00"));
                    //待验数
                    this.SetRowCell(pageIndex, rowIndex, 9, LocationDetailView.InspectQty.ToString("0.00"));
                    //不合格数 
                    this.SetRowCell(pageIndex, rowIndex, 10, LocationDetailView.RejectQty.ToString("0.00"));

                   

                    if (this.isPageBottom(rowIndex, rowTotal))//页的最后一行
                    {
                        this.SetRowCell(pageIndex, rowIndex + 1, 3, string.Format("当前第【{0}】页,共【{1}】页", pageIndex, LocationDetailViewList.Count / 46 + 1));
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


        /**
           * 需要拷贝的数据与合并单元格操作
           * 
           * Param pageIndex 页号
           */
        public override void CopyPageValues(int pageIndex)
        {
            //this.CopyCell(pageIndex, 50, 1, "B51");
            //this.CopyCell(pageIndex, 50, 5, "F51");
            //this.CopyCell(pageIndex, 50, 9, "J51");
            //this.CopyCell(pageIndex, 51, 0, "A52");
        }


    }
}
