using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;
using com.Sconit.PrintModel.INP;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepInspectOrderOperator : RepTemplate2
    {
        public RepInspectOrderOperator()
        {

            //明细部分的行数
            this.pageDetailRowCount = 15;
            //列数   1起始
            this.columnCount = 10;

            this.rowCount = 15;
            //报表头的行数  1起始
            this.leftColumnHeadCount = 0;
            //报表尾的行数  1起始
            this.bottomRowCount = 0;

            this.headRowCount = 0;

        }

        public override void CopyPageValues(int pageIndex)
        {
            this.SetMergedRegionColumn(pageIndex, 0, 0, 0, 8);
            this.SetMergedRegionColumn(pageIndex, 1, 0, 1, 8);
            this.SetMergedRegionColumn(pageIndex, 3, 1, 3, 4);
            this.SetMergedRegionColumn(pageIndex, 4, 1, 4, 4);
            this.SetMergedRegionColumn(pageIndex, 4, 6, 4, 8);
            this.SetMergedRegionColumn(pageIndex, 5, 1, 5, 4);
            this.SetMergedRegionColumn(pageIndex, 6, 1, 6, 4);
            this.SetMergedRegionColumn(pageIndex, 7, 1, 7, 2);
            this.SetMergedRegionColumn(pageIndex, 7, 3, 7, 4);
            this.SetMergedRegionColumn(pageIndex, 7, 6, 7, 8);
            this.SetMergedRegionColumn(pageIndex, 8, 1, 8, 2);
            this.SetMergedRegionColumn(pageIndex, 8, 3, 8, 4);
            this.SetMergedRegionColumn(pageIndex, 8, 6, 8, 8);
            this.SetMergedRegionColumn(pageIndex, 9, 1, 9, 4);
            this.SetMergedRegionColumn(pageIndex, 9, 6, 9, 8);
            this.SetMergedRegionColumn(pageIndex, 10, 1, 10, 4);
            this.SetMergedRegionColumn(pageIndex, 10, 6, 10, 8);
            this.SetMergedRegionColumn(pageIndex, 11, 1, 11, 4);
            this.SetMergedRegionColumn(pageIndex, 11, 6, 11, 8);
            this.SetMergedRegionColumn(pageIndex, 14, 0, 14, 8);
            this.SetMergedRegionColumn(pageIndex, 7, 0, 8, 0);
            this.SetMergedRegionColumn(pageIndex, 12, 1, 12, 4);

            this.CopyCellColumn(pageIndex, 0, 0, "A1");
            this.CopyCellColumn(pageIndex, 1, 0, "A2");
            this.CopyCellColumn(pageIndex, 2, 3, "D3");
            this.CopyCellColumn(pageIndex, 2, 7, "H3");
            this.CopyCellColumn(pageIndex, 3, 0, "A4");
            this.CopyCellColumn(pageIndex, 3, 5, "F4");
            this.CopyCellColumn(pageIndex, 3, 7, "H4");
            this.CopyCellColumn(pageIndex, 4, 0, "A5");
            this.CopyCellColumn(pageIndex, 4, 5, "F5");
            this.CopyCellColumn(pageIndex, 5, 0, "A6");
            this.CopyCellColumn(pageIndex, 5, 5, "F6");
            this.CopyCellColumn(pageIndex, 5, 7, "H6");
            this.CopyCellColumn(pageIndex, 6, 0, "A7");
            this.CopyCellColumn(pageIndex, 6, 5, "F7");
            this.CopyCellColumn(pageIndex, 6, 7, "H7");
            this.CopyCellColumn(pageIndex, 7, 0, "A8");
            this.CopyCellColumn(pageIndex, 7, 1, "B8");
            this.CopyCellColumn(pageIndex, 7, 3, "D8");
            this.CopyCellColumn(pageIndex, 7, 5, "F8");
            this.CopyCellColumn(pageIndex, 8, 5, "F9");
            this.CopyCellColumn(pageIndex, 8, 6, "G9");
            this.CopyCellColumn(pageIndex, 9, 0, "A10");
            this.CopyCellColumn(pageIndex, 9, 5, "F10");
            this.CopyCellColumn(pageIndex, 10, 0, "A11");
            this.CopyCellColumn(pageIndex, 10, 5, "F11");
            this.CopyCellColumn(pageIndex, 11, 0, "A12");
            this.CopyCellColumn(pageIndex, 11, 5, "F12");
            this.CopyCellColumn(pageIndex, 12, 0, "A13");
            //this.CopyCellColumn(pageIndex, 12, 1, "B13");
            //this.CopyCellColumn(pageIndex, 12, 3, "D13");
            this.CopyCellColumn(pageIndex, 12, 5, "F13");
            this.CopyCellColumn(pageIndex, 12, 7, "H13");
            this.CopyCellColumn(pageIndex, 13, 0, "A14");
            this.CopyCellColumn(pageIndex, 13, 4, "E14");
            this.CopyCellColumn(pageIndex, 13, 6, "G14");
            this.CopyCellColumn(pageIndex, 14, 0, "A15");

        }

        /**
         * 填充报表
         * 
         * Param list [0]huDetailList
         */
        protected override bool FillValuesImpl(String templateFileName, IList<object> list)
        {
            try
            {
                if (list == null) { return false; }
                IList<PrintInspectMaster> masterList = (IList<PrintInspectMaster>)list[0];

                string userName = "";
                if (list.Count == 2)
                {
                    userName = (string)list[1];
                }

                this.sheet.DisplayGridlines = false;
                this.sheet.IsPrintGridlines = false;

                //this.sheet.DisplayGuts = false;

                int count = masterList.Count;

                if (count == 0) return false;

                //this.barCodeFontName = this.GetBarcodeFontName(7, 1);

                //加页删页
                //纵向打印
                this.CopyPageCloumn(count, columnCount, 1);

                int pageIndex = 1;

                foreach (PrintInspectMaster inspectMaster in masterList)
                {
                    PrintInspectDetail printDet=inspectMaster.InspectDetails.First();
                    // 检验编号
                    //this.SetColumnCell(pageIndex, 0, 1, hu.ItemDescription);

                    // 编号
                    this.SetColumnCell(pageIndex, 2, 8, inspectMaster.InspectNo);

                    // 生产单位
                    this.SetColumnCell(pageIndex, 3, 1, inspectMaster.PartyFromName);

                    // 单位代码
                    this.SetColumnCell(pageIndex, 3, 6, inspectMaster.PartyFrom);

                    //结算方式
                    this.SetColumnCell(pageIndex, 3, 8, inspectMaster.ConsignmentType);
                   
                    //产品图号
                    this.SetColumnCell(pageIndex, 4, 1, printDet.Item+"("+printDet.ReferenceItemCode+")");

                    //产品名称
                    this.SetColumnCell(pageIndex, 5, 1, printDet.ItemDescription);

                    //供货状态
                    //this.SetColumnCell(pageIndex, 4, 6, printDet.Item);

                    //送检数量
                    this.SetColumnCell(pageIndex, 5, 6, printDet.InspectQty.ToString("0.########"));

                    //送检日期
                    this.SetColumnCell(pageIndex, 5, 8, inspectMaster.CreateDate.ToShortDateString());

                    //批次号
                    //this.SetColumnCell(pageIndex, 6, 1, "");

                    ////抽检数量
                    //this.SetColumnCell(pageIndex, 6, 6, "");

                    ////送检类型
                    //this.SetColumnCell(pageIndex, 6, 8, "");

                    ////签字刻章
                    //this.SetColumnCell(pageIndex, 7, 6, "");

                    ////合格
                    //this.SetColumnCell(pageIndex, 8, 1, "");

                    ////冻结
                    //this.SetColumnCell(pageIndex, 8, 3, "");

                    //收货单 //ASN号
                    this.SetColumnCell(pageIndex, 12, 1, "收货单:" + inspectMaster.ReceiptNo + "     ASN号:" + inspectMaster.IpNo);

                    //ASN号
                    //this.SetColumnCell(pageIndex, 12, 3, inspectMaster.IpNo);
                    //制单人
                    this.SetColumnCell(pageIndex, 13, 7, userName);
                    pageIndex++;

                }

            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

    }
}
