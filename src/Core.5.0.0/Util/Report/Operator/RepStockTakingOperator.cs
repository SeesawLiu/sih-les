using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.INV;
using com.Sconit.Entity.INV;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepStockTakingOperator : RepTemplate1
    {
        public RepStockTakingOperator()
        {
            //明细部分的行数
            this.pageDetailRowCount = 35;
            //列数   1起始
            this.columnCount = 9;
            //报表头的行数  1起始
            this.headRowCount = 11;
            //报表尾的行数  1起始
            this.bottomRowCount = 1;
        }

        /**
         * 填充报表
         * 
         *
         */
        protected override bool FillValuesImpl(String templateFileName, IList<object> list)
        {
            try
            {
                if (list == null || list.Count < 2) return false;

                PrintStockTakeMaster printStockTakeMaster = (PrintStockTakeMaster)list[0];
                IList<PrintStockTakeDetail> printStockTakeDetails = (IList<PrintStockTakeDetail>)list[1];
                if (printStockTakeMaster == null)
                {
                    return false;
                }

                //this.SetRowCellBarCode(0, 2, 7);
                this.barCodeFontName = this.GetBarcodeFontName(2, 5);
                this.CopyPage(printStockTakeDetails.Count);
                this.FillHead(printStockTakeMaster);


                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;
                int seq = 1;
                foreach (PrintStockTakeDetail printDetail in printStockTakeDetails)
                {
                    //序号	库位	物料代码	旧图号	物料描述	质量类型	寄售	寄售供应商	盘点数
                    //序号
                    this.SetRowCell(pageIndex, rowIndex, 0, seq++);
                    //库位
                    this.SetRowCell(pageIndex, rowIndex, 1, printDetail.Location);
                    //物料代码
                    this.SetRowCell(pageIndex, rowIndex, 2, printDetail.Item);
                    //旧图号
                    this.SetRowCell(pageIndex, rowIndex, 3, printDetail.RefItemCode);
                    //物料描述
                    this.SetRowCell(pageIndex, rowIndex, 4, printDetail.ItemDescription);
                    //质量类型
                    if (printDetail.QualityType == 0)
                    {
                        this.SetRowCell(pageIndex, rowIndex, 5, "正常");
                    }
                    else if (printDetail.QualityType == 1)
                    {
                        this.SetRowCell(pageIndex, rowIndex, 5, "待验");
                    }
                    else if (printDetail.QualityType == 2)
                    {
                        this.SetRowCell(pageIndex, rowIndex, 5,"不合格");
                    }

                    //寄售
                    if (printDetail.IsConsigement)
                    {
                        this.SetRowCell(pageIndex, rowIndex, 6, "√");
                        //寄售供应商
                        this.SetRowCell(pageIndex, rowIndex, 7, printDetail.CSSupplier);
                    }
                    
                    //盘点数
                    //this.SetRowCell(pageIndex, rowIndex, 3, printDetail.Uom);

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
                    seq++;
                }
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
        private void FillHead(PrintStockTakeMaster stockTakeMaster)
        {
            //盘点单号:
            string seqCode = Utility.BarcodeHelper.GetBarcodeStr(stockTakeMaster.StNo, this.barCodeFontName);
            this.SetRowCell(2, 6, seqCode);
            //盘点单号 No.:
            this.SetRowCell(4, 6, stockTakeMaster.StNo);

            //区域
            this.SetRowCell(6, 2, stockTakeMaster.Region);

            //创建用户
            this.SetRowCell(6, 6, stockTakeMaster.CreateUserName);

            //创建时间 
            this.SetRowCell(8, 6, stockTakeMaster.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));

            
        }

        /**
           * 需要拷贝的数据与合并单元格操作
           * 
           * Param pageIndex 页号
           */
        public override void CopyPageValues(int pageIndex)
        {
            //盘点人:
            this.CopyCell(pageIndex, 46, 1, "B47");
            //完成时间:
            this.CopyCell(pageIndex, 46, 6, "G47");
        }
    }
}
