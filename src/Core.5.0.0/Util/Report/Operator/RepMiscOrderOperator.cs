using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepMiscOrderOperator : RepTemplate1
    {
        public RepMiscOrderOperator()
        {

            //明细部分的行数
            this.pageDetailRowCount = 35;
            //列数   1起始
            this.columnCount = 7;
            //报表头的行数  1起始
            this.headRowCount = 14;
            //报表尾的行数  1起始
            this.bottomRowCount = 2;

        }

        /**
         * 填充报表
         * 
         * Param list [0]InProcessLocation
         *            [1]inProcessLocationDetailList
         */
        protected override bool FillValuesImpl(String templateFileName, IList<object> list)
        {
            try
            {

                if (list == null || list.Count < 2) return false;

                PrintMiscOrderMaster miscOrderMaster = (PrintMiscOrderMaster)list[0];
                IList<PrintMiscOrderDetail> miscOrderDetailList = (IList<PrintMiscOrderDetail>)list[1];

                if (miscOrderMaster == null
                    || miscOrderDetailList == null || miscOrderDetailList.Count == 0)
                {
                    return false;
                }
                //ASN号:
                //List<Transformer> transformerList = Utility.TransformerHelper.ConvertInProcessLocationDetailsToTransformers(ipDetailList);

                this.CopyPage(miscOrderDetailList.Count);

                this.FillHead(miscOrderMaster);

                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;

                foreach (PrintMiscOrderDetail miscOrderDetail in miscOrderDetailList)
                {
                    //this.SetRowCell(pageIndex, rowIndex, 0, ipDetail.WindowTime.Value.ToString("yyyy-MM-dd"));

                    //序号
                    this.SetRowCell(pageIndex, rowIndex, 0, miscOrderDetail.Sequence);

                    //库位
                    this.SetRowCell(pageIndex, rowIndex, 1, miscOrderDetail.Location);

                    //零件号
                    this.SetRowCell(pageIndex, rowIndex, 2, miscOrderDetail.Item);

                    //老图号
                    this.SetRowCell(pageIndex, rowIndex, 3, miscOrderDetail.ReferenceItemCode);

                    //零件描述
                    this.SetRowCell(pageIndex, rowIndex, 4, miscOrderDetail.ItemDescription);

                    //单位
                    this.SetRowCell(pageIndex, rowIndex, 5, miscOrderDetail.Uom);

                    //数量
                    this.SetRowCell(pageIndex, rowIndex, 6, miscOrderDetail.Qty.ToString("0.########"));


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
         * Param pageIndex 页号
         * Param orderHead 订单头对象
         * Param orderDetails 订单明细对象
         */
        protected void FillHead(PrintMiscOrderMaster miscOrderMaster)
        {
            //计划外出入库单号:
            string orderCode = Utility.BarcodeHelper.GetBarcodeStr(miscOrderMaster.MiscOrderNo, this.barCodeFontName);
            this.SetRowCell(1, 5, orderCode);

            //Order No.:
            this.SetRowCell(2, 5, miscOrderMaster.MiscOrderNo);

            //区域:
            this.SetRowCell(4, 2, miscOrderMaster.Region);

            //移库类型:
            this.SetRowCell(5, 2, miscOrderMaster.MoveType);

            //科目代码:	
            this.SetRowCell(6, 2, "");

            //分账号代码:	
            this.SetRowCell(7, 2, "");

            //质量类型:
            this.SetRowCell(8, 2, miscOrderMaster.QualityType.ToString());

            //创建日期:	
            this.SetRowCell(9, 2, miscOrderMaster.EffectiveDate.ToString("yyyy-MM-dd hh:nn:ss"));

            //生效日期:	
            this.SetRowCell(10, 2, miscOrderMaster.EffectiveDate.ToString("yyyy-MM-dd"));

            //参考订单号:
            this.SetRowCell(4, 5, miscOrderMaster.ReferenceNo);

            //移动原因:
            this.SetRowCell(5, 5, miscOrderMaster.Note);
            
            //成本中心:
            this.SetRowCell(6, 5, miscOrderMaster.CostCenter);

            //项目代码:
            this.SetRowCell(7, 5, "");

            //WBS:
            this.SetRowCell(8, 5, miscOrderMaster.WBS);
            
            //创建者: 
            this.SetRowCell(9, 5, miscOrderMaster.CreateUserName);
          
        }

        /**
           * 需要拷贝的数据与合并单元格操作
           * 
           * Param pageIndex 页号
           */
        public override void CopyPageValues(int pageIndex)
        {
            //签字
            this.CopyCell(pageIndex, 49, 3, "D50");

        }

    }
}
