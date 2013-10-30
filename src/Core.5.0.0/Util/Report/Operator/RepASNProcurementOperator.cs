using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepASNProcurementOperator : RepTemplate1
    {
        public RepASNProcurementOperator()
        {

            //明细部分的行数
            this.pageDetailRowCount = 35;
            //列数   1起始
            this.columnCount = 11;
            //报表头的行数  1起始
            this.headRowCount = 15;
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

                PrintIpMaster ipMaster = (PrintIpMaster)list[0];
                IList<PrintIpDetail> ipDetailList = (IList<PrintIpDetail>)list[1];

                if (ipMaster == null
                    || ipDetailList == null || ipDetailList.Count == 0)
                {
                    return false;
                }
                //ASN号:
                //List<Transformer> transformerList = Utility.TransformerHelper.ConvertInProcessLocationDetailsToTransformers(ipDetailList);
                this.barCodeFontName = this.GetBarcodeFontName(2, 7);

                this.CopyPage(ipDetailList.Count);

                this.FillHead(ipMaster);

                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;

                foreach (PrintIpDetail ipDetail in ipDetailList)
                {
                    //this.SetRowCell(pageIndex, rowIndex, 0, ipDetail.WindowTime.HasValue? ipDetail.WindowTime.Value.ToString("yyyy-MM-dd"):string.Empty);
                    //入库库位
                    this.SetRowCell(pageIndex, rowIndex, 0, ipDetail.LocationToName);

                    // 合同号/行号
                    this.SetRowCell(pageIndex, rowIndex, 1, ipDetail.ExternalOrderNo + "/" + ipDetail.ExternalSequence);

                    //零件号
                    this.SetRowCell(pageIndex, rowIndex, 2, ipDetail.Item);

                    //旧零件号
                    this.SetRowCell(pageIndex, rowIndex, 3, ipDetail.ReferenceItemCode);

                    //零件描述
                    this.SetRowCell(pageIndex, rowIndex, 4, ipDetail.ItemDescription);

                    //单位
                    this.SetRowCell(pageIndex, rowIndex, 5, ipDetail.Uom);

                    //单包装
                    this.SetRowCell(pageIndex, rowIndex, 7, ipDetail.UnitCount.ToString("0.########"));

                    //实送数量 包装数
                    this.SetRowCell(pageIndex, rowIndex, 8, (ipDetail.Qty / ipDetail.UnitCount).ToString("0.########"));

                    //实送数量
                    this.SetRowCell(pageIndex, rowIndex, 6, ipDetail.Qty.ToString("0.########"));


                    //是否免检
                    if (ipDetail.IsInspect == false)
                    {
                        this.SetRowCell(pageIndex, rowIndex, 9, "Y");
                    }
                    else
                        this.SetRowCell(pageIndex, rowIndex, 9, "N");

                    //采购类型（寄售/非寄售/委外/转储）
                    //0：标准，2：寄售，3：委外，7：转储                     
                    if (ipDetail.PSTYP == "0")
                    {
                        this.SetRowCell(pageIndex, rowIndex, 10, "标准");
                    }
                    else if (ipDetail.PSTYP == "2")
                    {
                        this.SetRowCell(pageIndex, rowIndex, 10, "寄售");
                    }
                    else if (ipDetail.PSTYP == "3")
                    {
                        this.SetRowCell(pageIndex, rowIndex, 10, "委外");
                    }
                    else if (ipDetail.PSTYP == "7")
                    {
                        this.SetRowCell(pageIndex, rowIndex, 10, "转储");
                    }

                    if ("ZC1".Equals(ipDetail.Tax, StringComparison.OrdinalIgnoreCase))
                    {
                        this.SetRowCell(pageIndex, rowIndex, 11, "军车");
                    }
                    else if ("ZC2".Equals(ipDetail.Tax, StringComparison.OrdinalIgnoreCase))
                    {
                        this.SetRowCell(pageIndex, rowIndex, 11, "出口车");
                    }
                    else if ("ZC3".Equals(ipDetail.Tax, StringComparison.OrdinalIgnoreCase))
                    {
                        this.SetRowCell(pageIndex, rowIndex, 11, "特殊车");
                    }
                    else if ("ZC5".Equals(ipDetail.Tax, StringComparison.OrdinalIgnoreCase))
                    {
                        this.SetRowCell(pageIndex, rowIndex, 11, "CKD");
                    }
                    //if (this.isPageBottom(rowIndex, rowTotal))//页的最后一行
                    //{
                    //    //客户代码
                    //    this.SetRowCell(pageIndex, this.pageDetailRowCount + 4, 10, ipMaster.PartyTo == null ? string.Empty : ipMaster.PartyTo);
                    //    //开单人
                    //    this.SetRowCell(pageIndex, this.pageDetailRowCount + 6, 1, ipMaster.CreateUserName);
                    //    //开单日期
                    //    this.SetRowCell(pageIndex, this.pageDetailRowCount + 6, 3, ipMaster.CreateDate.ToString("yyyy    MM    dd"));

                    //    this.sheet.SetRowBreak(this.GetRowIndexAbsolute(pageIndex, this.pageDetailRowCount + this.bottomRowCount - 1));

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
        protected void FillHead(PrintIpMaster ipMaster)
        {
            //订单号:
            string orderCode = Utility.BarcodeHelper.GetBarcodeStr(ipMaster.IpNo, this.barCodeFontName);
            this.SetRowCell(2, 7, orderCode);
            //Order No.:
            this.SetRowCell(3, 7, ipMaster.IpNo);

            //制单时间 Create Time:
            this.SetRowCell(4, 4, ipMaster.ArriveTime.ToString("yyyy-MM-dd HH:mm:ss"));

            //窗口时间 ArriveTime Time:
            this.SetRowCell(4, 7, ipMaster.ArriveTime.ToString("yyyy-MM-dd HH:mm:ss"));

            //供应商代码 Supplier Code:	
            this.SetRowCell(6, 2, ipMaster.PartyFrom);
            //目的区域
            this.SetRowCell(6, 8, ipMaster.PartyTo);


            //供应商名称 Supplier Name:		
            this.SetRowCell(7, 2, ipMaster.PartyFromName);
            //目的区域名称
            this.SetRowCell(7, 8, ipMaster.PartyToName);

            //供应商地址 Address:	
            this.SetRowCell(8, 2, ipMaster.ShipFromAddress);
            //目的区域地址
            this.SetRowCell(8, 8, ipMaster.ShipToAddress);

            //供应商联系人 Contact:	
            this.SetRowCell(9, 2, ipMaster.ShipFromContact);
            //目的库位
            this.SetRowCell(9, 8, ipMaster.IpDetails[0].LocationTo);

            //车辆号:		
            this.SetRowCell(10, 2, "");
            //收货道口
            this.SetRowCell(10, 8, ipMaster.Dock);

            ////供应商传真 Fax:	
            //this.SetRowCell(11, 3, orderHead.ShipFromFax);
            ////YFV传真 Fax:
            //this.SetRowCell(11, 8, orderHead.ShipToFax);

            //系统号 SysCode:
            //this.SetRowCell(++rowNum, 3, "");
            //版本号 Version:
            //this.SetRowCell(rowNum, 8, "");
        }

        /**
           * 需要拷贝的数据与合并单元格操作
           * 
           * Param pageIndex 页号
           */
        public override void CopyPageValues(int pageIndex)
        {
            //客户代码
            this.CopyCell(pageIndex, 50, 2, "C51");
            //开单人
            this.CopyCell(pageIndex, 50, 4, "F51");
            //开单日期
            this.CopyCell(pageIndex, 50, 9, "J51");
        }

    }
}
