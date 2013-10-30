using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;
using com.Sconit.Entity.ORD;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepProductOrderDetViewOperator : RepTemplate1
    {
        public RepProductOrderDetViewOperator()
        {
            //明细部分的行数
            this.pageDetailRowCount = 51;
            //列数   1起始
            this.columnCount = 21;
            //报表头的行数  1起始
            this.headRowCount = 1;
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
                IList<OrderDetail> orderDetailList = (IList<OrderDetail>)(list[0]);

               // orderDetails = orderDetails.OrderBy(o => o.Sequence).ThenBy(o => o.Item).ToList();

                if (orderDetailList == null || orderDetailList.Count == 0)
                {
                    return false;
                }


                //this.SetRowCellBarCode(0, 2, 8);
               // this.barCodeFontName = this.GetBarcodeFontName(2, 8);
                this.CopyPage(orderDetailList.Count);

                //this.FillHead(orderMaster);


                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;
                foreach (OrderDetail printOrderDetail in orderDetailList)
                {

                    this.SetRowCell(pageIndex, rowIndex, 0, printOrderDetail.Id);
                    // No.	
                    this.SetRowCell(pageIndex, rowIndex, 1, printOrderDetail.OrderNo);

                    //零件号 Item Code
                    this.SetRowCell(pageIndex, rowIndex, 2, printOrderDetail.MastExtOrderNo);

                    //参考号 Ref No.
                    this.SetRowCell(pageIndex, rowIndex, 3, printOrderDetail.MastRefOrderNo);

                    //描述Description
                    this.SetRowCell(pageIndex, rowIndex, 4, printOrderDetail.Item);

                    //单位Unit
                    this.SetRowCell(pageIndex, rowIndex, 5, printOrderDetail.ReferenceItemCode);

                    //单包装UC
                    this.SetRowCell(pageIndex, rowIndex, 6, printOrderDetail.ItemDescription);

                    this.SetRowCell(pageIndex, rowIndex, 7, printOrderDetail.Uom);

                   
                    this.SetRowCell(pageIndex, rowIndex, 8, printOrderDetail.UnitCount.ToString("0.00"));

                    
                    this.SetRowCell(pageIndex, rowIndex, 9, printOrderDetail.LocationTo);

                    
                    this.SetRowCell(pageIndex, rowIndex, 10, printOrderDetail.SAPLocation);

                    
                    this.SetRowCell(pageIndex, rowIndex, 11, printOrderDetail.OrderedQty.ToString("0.00"));

                    this.SetRowCell(pageIndex, rowIndex, 12, printOrderDetail.ShippedQty.ToString("0.00"));

                    this.SetRowCell(pageIndex, rowIndex, 13, printOrderDetail.ReceivedQty.ToString("0.00"));

                    this.SetRowCell(pageIndex, rowIndex, 14, printOrderDetail.ManufactureParty);

                    
                    this.SetRowCell(pageIndex, rowIndex, 15, printOrderDetail.MastPartyFrom);

                  
                    this.SetRowCell(pageIndex, rowIndex,16, printOrderDetail.MastPartyTo);

                  
                    this.SetRowCell(pageIndex, rowIndex, 17, printOrderDetail.MastFlow);

                    this.SetRowCell(pageIndex, rowIndex, 18, printOrderDetail.MastType);

                   
                    this.SetRowCell(pageIndex, rowIndex, 19, printOrderDetail.MastStatus);

                   
                    this.SetRowCell(pageIndex, rowIndex, 20, printOrderDetail.MastCreateDate.ToString("yyyy-MM-dd"));

                    if (this.isPageBottom(rowIndex, rowTotal))//页的最后一行
                    {
                        this.SetRowCell(pageIndex, rowIndex+1, 3, string.Format("当前第【{0}】页,共【{1}】页", pageIndex, orderDetailList.Count / 51 + 1));
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
