using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SAP.TRANS;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepInvTransOperator : RepTemplate1
    {
        public RepInvTransOperator()
        {
            //明细部分的行数
            this.pageDetailRowCount = 51;
            //列数   1起始
            this.columnCount = 39;
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
                IList<InvTrans> invTransList = (IList<InvTrans>)(list[0]);


                if (invTransList == null || invTransList.Count == 0)
                {
                    return false;
                }
                this.CopyPage(invTransList.Count);


                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;
                foreach (InvTrans invTrans in invTransList)
                {
                    //TCODE	BWART	BLDAT	BUDAT	EBELN	EBELP	VBELN	POSNR	
                   

                    this.SetRowCell(pageIndex, rowIndex, 0, invTrans.TCODE);

                    this.SetRowCell(pageIndex, rowIndex, 1, invTrans.BWART);


                    this.SetRowCell(pageIndex, rowIndex, 2, invTrans.BLDAT);


                    this.SetRowCell(pageIndex, rowIndex, 3, invTrans.BUDAT);


                    this.SetRowCell(pageIndex, rowIndex, 4, invTrans.EBELN);

                    //单位Unit
                    this.SetRowCell(pageIndex, rowIndex, 5, invTrans.EBELP);

                    //单包装UC
                    this.SetRowCell(pageIndex, rowIndex, 6, invTrans.VBELN);

                    this.SetRowCell(pageIndex, rowIndex, 7, invTrans.POSNR);

                    //LIFNR	WERKS	LGORT	SOBKZ	MATNR	ERFMG	ERFME	UMLGO	
                    this.SetRowCell(pageIndex, rowIndex, 8, invTrans.LIFNR);


                    this.SetRowCell(pageIndex, rowIndex, 9, invTrans.WERKS);


                    this.SetRowCell(pageIndex, rowIndex, 10, invTrans.LGORT);


                    this.SetRowCell(pageIndex, rowIndex, 11, invTrans.SOBKZ);

                    this.SetRowCell(pageIndex, rowIndex, 12, invTrans.MATNR);

                    this.SetRowCell(pageIndex, rowIndex, 13, invTrans.ERFMG.ToString("0.00"));

                    this.SetRowCell(pageIndex, rowIndex, 14, invTrans.ERFME);


                    this.SetRowCell(pageIndex, rowIndex, 15, invTrans.UMLGO);

                    //GRUND	KOSTL	XBLNR	RSNUM	RSPOS	FRBNR	
                    this.SetRowCell(pageIndex, rowIndex, 16, invTrans.GRUND);


                    this.SetRowCell(pageIndex, rowIndex, 17, invTrans.KOSTL);

                    this.SetRowCell(pageIndex, rowIndex, 18, invTrans.XBLNR);

                    this.SetRowCell(pageIndex, rowIndex, 19, invTrans.RSNUM);


                    this.SetRowCell(pageIndex, rowIndex, 20, invTrans.RSPOS);

                    this.SetRowCell(pageIndex, rowIndex, 21, invTrans.FRBNR);
                    //SGTXT	OLD	INSMK	XABLN	AUFNR	UMMAT	UMWRK	
                    this.SetRowCell(pageIndex, rowIndex, 22, invTrans.SGTXT);

                    this.SetRowCell(pageIndex, rowIndex, 23, invTrans.OLD);

                    this.SetRowCell(pageIndex, rowIndex, 24, invTrans.INSMK);

                    this.SetRowCell(pageIndex, rowIndex, 25, invTrans.XABLN);


                    this.SetRowCell(pageIndex, rowIndex, 26, invTrans.AUFNR);

                    this.SetRowCell(pageIndex, rowIndex, 27, invTrans.UMMAT);
                    this.SetRowCell(pageIndex, rowIndex, 28, invTrans.UMWRK);

                    //KZEAR	CHARG	CreateDate	LastModifyDate	StatusName	ErrorCount	ErrorId ErrorMessage OrderNo DetailId	
                    this.SetRowCell(pageIndex, rowIndex, 29, invTrans.KZEAR);

                    this.SetRowCell(pageIndex, rowIndex, 30, invTrans.CHARG);


                    this.SetRowCell(pageIndex, rowIndex, 31, invTrans.CreateDate.ToString("yyyy-MM-dd"));

                    this.SetRowCell(pageIndex, rowIndex, 32, invTrans.LastModifyDate.ToString("yyyy-MM-dd"));

                    this.SetRowCell(pageIndex, rowIndex, 33, invTrans.StatusName);


                    this.SetRowCell(pageIndex, rowIndex, 34, invTrans.ErrorCount);

                    this.SetRowCell(pageIndex, rowIndex, 35, invTrans.ErrorId.ToString());

                    this.SetRowCell(pageIndex, rowIndex, 36, invTrans.ErrorMessage);

                    this.SetRowCell(pageIndex, rowIndex, 37, invTrans.OrderNo);

                    this.SetRowCell(pageIndex, rowIndex, 38, invTrans.DetailId);
                    this.SetRowCell(pageIndex, rowIndex, 39, invTrans.POSID);

                    if (this.isPageBottom(rowIndex, rowTotal))//页的最后一行
                    {
                        this.SetRowCell(pageIndex, rowIndex + 1, 3, string.Format("当前第【{0}】页,共【{1}】页", pageIndex, invTransList.Count / 51 + 1));
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
