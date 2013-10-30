using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.PrintModel.ORD;

namespace com.Sconit.Utility.Report.Operator
{
    public class RepASNTransferOperator : RepTemplate1
    {
        public RepASNTransferOperator()
        {

            //明细部分的行数
            this.pageDetailRowCount = 35;
            //列数   1起始
            this.columnCount = 11;
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

                PrintIpMaster ipMaster = (PrintIpMaster)list[0];
                IList<PrintIpDetail> ipDetailList = (IList<PrintIpDetail>)list[1];

                if (ipMaster == null
                    || ipDetailList == null || ipDetailList.Count == 0)
                {
                    return false;
                }
                //ASN号:
                //List<Transformer> transformerList = Utility.TransformerHelper.ConvertInProcessLocationDetailsToTransformers(ipDetailList);
                this.barCodeFontName = this.GetBarcodeFontName(2, 8);
                this.CopyPage(ipDetailList.Count);

                this.FillHead(ipMaster);

                int pageIndex = 1;
                int rowIndex = 0;
                int rowTotal = 0;

                foreach (PrintIpDetail ipDetail in ipDetailList)
                {
                    this.SetRowCell(pageIndex, rowIndex, 0, ipDetail.WindowTime.HasValue?ipDetail.WindowTime.Value.ToString("yyyy-MM-dd"):string.Empty);

                    //物料号
                    this.SetRowCell(pageIndex, rowIndex, 1, ipDetail.Item);

                    //旧图号
                    this.SetRowCell(pageIndex, rowIndex, 2, ipDetail.ReferenceItemCode);

                    //物料描述
                    this.SetRowCell(pageIndex, rowIndex, 3, ipDetail.ItemDescription);

                    //制造商
                    this.SetRowCell(pageIndex, rowIndex, 4, ipDetail.ManufactureParty);

                    //单位
                    this.SetRowCell(pageIndex, rowIndex, 5, ipDetail.Uom);

                    //单包装
                    this.SetRowCell(pageIndex, rowIndex, 6, ipDetail.UnitCount.ToString("0.########"));

                    //箱数
                    this.SetRowCell(pageIndex, rowIndex, 7, Math.Round((ipDetail.Qty / ipDetail.UnitCount),0).ToString("0.########"));

                    //实送数量
                    this.SetRowCell(pageIndex, rowIndex, 8, ipDetail.Qty.ToString("0.########"));

                    
                    
                    //来源库位
                    this.SetRowCell(pageIndex, rowIndex, 10, string.IsNullOrEmpty(ipDetail.LocationFrom)?string.Empty:ipDetail.LocationFrom);

                    //目的库位
                    if (string.IsNullOrEmpty(ipDetail.BinTo))
                    {
                        this.SetRowCell(pageIndex, rowIndex, 11, ipDetail.LocationTo);
                    }
                    else
                    {
                        this.SetRowCell(pageIndex, rowIndex, 11, ipDetail.LocationTo + "[" + ipDetail.BinTo + "]");
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

                    //    pageIndex++;
                    //    rowIndex = 0;
                    //}
                    //else
                    //{
                    //    rowIndex++;
                    //}
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
            this.SetRowCell(2, 8, orderCode);
            //Order No.:
            this.SetRowCell(3, 8, ipMaster.IpNo);

            //制单时间 Create Time:
            this.SetRowCell(4, 7, ipMaster.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));

            //供应商代码 Supplier Code:	
            this.SetRowCell(6, 3, ipMaster.PartyFrom);
            //目的区域
            this.SetRowCell(6, 8, ipMaster.PartyTo);


            //供应商名称 Supplier Name:		
            this.SetRowCell(7, 3, ipMaster.PartyFromName);
            //目的区域名称
            this.SetRowCell(7, 8, ipMaster.PartyToName);

            //供应商地址 Address:	
            this.SetRowCell(8, 3, ipMaster.ShipFromAddress);
            //目的区域地址
            this.SetRowCell(8, 8, ipMaster.ShipToAddress);

            //供应商联系人 Contact:	
            this.SetRowCell(9, 3, ipMaster.ShipFromContact);
            //目的库位
            //this.SetRowCell(9, 8, ipMaster.IpDetails[0].LocationTo);

            //供应商电话 Telephone:		
            //this.SetRowCell(10, 3, ipMaster.ShipFromTel);
            //收货道口
            this.SetRowCell(9, 8, ipMaster.Dock);

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
            this.CopyCell(pageIndex, 49, 1, "B50");
            //开单人
            this.CopyCell(pageIndex, 49, 3, "E50");
            //开单日期
            this.CopyCell(pageIndex, 49, 8, "I50");
        }

    }
}
