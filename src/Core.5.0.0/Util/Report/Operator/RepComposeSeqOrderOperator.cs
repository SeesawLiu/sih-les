using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepComposeSeqOrderOperator: RepTemplate1
    {
        public RepComposeSeqOrderOperator()
        {
            //明细部分的行数
            this.pageDetailRowCount = 30;
            //列数   1起始
            this.columnCount = 7;
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
            this.SetRowCell(1, 2, string.Format("共有Van号{0}个。",sequenceMaster.VanNoCount));
            //排序单号:
            string seqCode = Utility.BarcodeHelper.GetBarcodeStr(sequenceMaster.OrderNo, this.barCodeFontName);
            this.SetRowCell(1, 5, seqCode);
            //排序单号 No.:
            this.SetRowCell(2, 5, sequenceMaster.OrderNo);

            //线体
            this.SetRowCell(2, 3, sequenceMaster.SequenceGroup.Substring(sequenceMaster.SequenceGroup.Length-1));

            //来源组织
            this.SetRowCell(3, 1, sequenceMaster.PartyFrom);

            //来源库位
            this.SetRowCell(3, 3, sequenceMaster.LocationFrom);

            //发出时间 Create Time:
            this.SetRowCell(3, 5, sequenceMaster.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));

            ////目的组织
            //this.SetRowCell(4, 2, sequenceMaster.PartyTo);

            //排序组
            this.SetRowCell(4, 3, sequenceMaster.FlowDescription);
            //拨次
            this.SetRowCell(4, 1, sequenceMaster.TraceCode);

            //窗口时间 
            this.SetRowCell(4, 5, sequenceMaster.WindowTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /**
           * 需要拷贝的数据与合并单元格操作
           * 
           * Param pageIndex 页号
           */
        public override void CopyPageValues(int pageIndex)
        {
            this.CopyCell(pageIndex, 37, 0, "A38");
            this.CopyCell(pageIndex, 37, 4, "E38");
            this.CopyCell(pageIndex, 37, 8, "I38");
        }


    }
}
