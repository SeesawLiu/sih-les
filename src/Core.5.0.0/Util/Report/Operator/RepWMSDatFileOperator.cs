using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;
using com.Sconit.Entity.CUST;
using com.Sconit.Entity.FIS;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepWMSDatFileOperator : RepTemplate1
    {
        public RepWMSDatFileOperator()
        {
            //明细部分的行数
            this.pageDetailRowCount = 46;
            //列数   1起始
            this.columnCount = 13;
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
                IList<WMSDatFile> WMSDatFileList = (IList<WMSDatFile>)(list[0]);

               // orderDetails = orderDetails.OrderBy(o => o.Sequence).ThenBy(o => o.Item).ToList();

                if (WMSDatFileList == null || WMSDatFileList.Count == 0)
                {
                    return false;
                }


                //this.SetRowCellBarCode(0, 2, 8);
               // this.barCodeFontName = this.GetBarcodeFontName(2, 8);
                this.CopyPage(WMSDatFileList.Count);

                //this.FillHead(orderMaster);


                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;
                foreach (WMSDatFile wMSDatFile in WMSDatFileList)
                {
                    // No.	
                    this.SetRowCell(pageIndex, rowIndex, 0, wMSDatFile.WmsNo);
                    // No.	
                    this.SetRowCell(pageIndex, rowIndex, 1, wMSDatFile.MoveType);
                    // No.	
                    this.SetRowCell(pageIndex, rowIndex, 2, wMSDatFile.WMSId);

                    //零件号 Item Code
                    this.SetRowCell(pageIndex, rowIndex, 3, wMSDatFile.Item);

                    //参考号 Ref No.
                    this.SetRowCell(pageIndex, rowIndex, 4, wMSDatFile.ItemDescription);

                    //描述Description
                    //this.SetRowCell(pageIndex, rowIndex, 5, wMSDatFile.ReferenceItemCode);

                    //单位Unit
                    this.SetRowCell(pageIndex, rowIndex, 5, wMSDatFile.Uom);

                    //单包装UC
                    this.SetRowCell(pageIndex, rowIndex, 6, wMSDatFile.LGORT);
                    this.SetRowCell(pageIndex, rowIndex, 7, wMSDatFile.UMLGO);

                    this.SetRowCell(pageIndex, rowIndex, 8, wMSDatFile.OrderQty.ToString("0.00"));

                    this.SetRowCell(pageIndex, rowIndex, 9, wMSDatFile.Qty.ToString("0.00"));

                    this.SetRowCell(pageIndex, rowIndex, 10, wMSDatFile.ReceivedQty.ToString("0.00"));

                    this.SetRowCell(pageIndex, rowIndex, 11, wMSDatFile.IsHand?"是":"否");

                    this.SetRowCell(pageIndex, rowIndex, 12,wMSDatFile.HandResult);

                    this.SetRowCell(pageIndex, rowIndex, 13, wMSDatFile.ErrorCause);

                   

                    if (this.isPageBottom(rowIndex, rowTotal))//页的最后一行
                    {
                        this.SetRowCell(pageIndex, rowIndex + 1, 3, string.Format("当前第【{0}】页,共【{1}】页", pageIndex, WMSDatFileList.Count / 46 + 1));
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
            this.SetRowCell(4, 9, orderMaster.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));

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

            //内部移库显示库位信息
            if (orderMaster.Type == (short)CodeMaster.OrderType.Transfer)
            {
                //来源库位:	
                this.SetRowCell(11, 3, orderMaster.PartyFrom != null ? orderMaster.PartyFrom + "@" + orderMaster.PartyFromName : String.Empty);
                //目的库位:
                this.SetRowCell(11, 8, orderMaster.PartyTo != null ? orderMaster.PartyTo + "@" + orderMaster.PartyToName : String.Empty);
            }
            else
            {
                //供应商传真 Fax:	
                this.SetRowCell(11, 3, orderMaster.ShipFrom != null ? orderMaster.ShipFromFax : String.Empty);
                //YFV传真 Fax:
                this.SetRowCell(11, 8, orderMaster.ShipTo != null ? orderMaster.ShipToFax : String.Empty);
            }
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
