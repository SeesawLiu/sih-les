using System;
using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity.FIS;
using com.Sconit.Persistence;
using com.Sconit.Service.FIS;
using com.Sconit.Utility;

namespace com.Sconit.Service.FIS.Impl
{
    public class CreateIpDATOutboundMgrImpl : AbstractOutboundMgr
    {
        //public IGenericMgr genericMgr { get; set; }

        public static string nowTime = string.Empty;
        public static int i = 10;
        private static object ExportIpDATLock = new object();
        protected override void ExtractOutboundData(string systemCode, OutboundControl outboundControl)
        {
            lock (ExportIpDATLock)
            {
                if (i == 99) i = 10;
                Dictionary<string, string[][]> result = new Dictionary<string, string[][]>();
                IList<CreateIpDAT> createIpDATList = this.genericMgr.FindAll<CreateIpDAT>(@"from CreateIpDAT as c where c.IsCreateDat=0 and c.ErrorCount<5");
                List<string> ipNos = createIpDATList.Select(o => o.ASN_NO).Distinct().ToList();
                nowTime = System.DateTime.Now.ToString("yyyyMMddHHmmss");

                foreach (var ipNo in ipNos)
                {
                    List<string[]> extractData = new List<string[]>();
                    foreach (var createIpDAT in createIpDATList.Where(o => o.ASN_NO == ipNo).OrderBy(o => o.Id))
                    {
                        List<string> data = new List<string>();
                        data.Add(createIpDAT.ASN_NO);
                        data.Add(createIpDAT.ASN_NO + "\t");
                        data.Add(createIpDAT.ASN_ITEM + "\t");
                        data.Add(createIpDAT.WH_CODE + "\t");
                        data.Add(createIpDAT.WH_DOCK + "\t");
                        data.Add(createIpDAT.WH_LOCATION + "\t");
                        data.Add(createIpDAT.ITEM_CODE + "\t");
                        data.Add(createIpDAT.SUPPLIER_CODE + "\t");
                        data.Add(createIpDAT.QTY + "\t");
                        data.Add(createIpDAT.UOM + "\t");
                        data.Add(createIpDAT.BASE_UNIT_QTY + "\t");
                        data.Add(createIpDAT.BASE_UNIT_UOM + "\t");
                        data.Add(createIpDAT.QC_FLAG + "\t");
                        data.Add(createIpDAT.DELIVERY_DATE.ToString("yyyyMMddHHmmss") + "\t");
                        data.Add(createIpDAT.TIME_WINDOW + "\t");
                        data.Add(createIpDAT.PO + "\t");
                        data.Add(createIpDAT.FINANCE_FLAG + "\t");
                        data.Add(createIpDAT.COMPONENT_FLAG + "\t");
                        data.Add(createIpDAT.TRACKID + "\t");
                        data.Add(createIpDAT.PO_LINE + "\t");
                        data.Add(createIpDAT.FactoryInfo + "\t");
                        data.Add(createIpDAT.F80XBJ + "\t");
                        data.Add(createIpDAT.F80X_LOCATION);
                        extractData.Add(data.ToArray());
                    }
                    //string fileName = createIpDATList.Where(o => o.ASN_NO == ipNo).First().FileName;
                    //result.Add(fileName+(i++)+".DAT", extractData.ToArray());
                    result.Add("ASNLE" + nowTime + (i++) + ".DAT", extractData.ToArray());
                }
                base.BaseOutboundData(result, outboundControl);
            }
        }

        protected override void SaveOutboundData(string ipNo, Boolean successOrError,string fileName)
        {
            if (successOrError)
            {
                this.genericMgr.Update("update CreateIpDAT set IsCreateDat=?,TIME_STAMP1=?,FileName=? where ASN_NO=?", new object[] {true, System.DateTime.Now,fileName, ipNo.Replace("\t", string.Empty) });
               // this.genericMgr.Update("update CreateIpDAT set IsCreateDat=1 where ASN_NO=?", ipNo.Replace("\t",string.Empty));
                // IList<CreateIpDAT> createIpDATList = this.genericMgr.FindAll<CreateIpDAT>(@"from CreateIpDAT as c where c.IpNo=?",ipNo);
            }
            else {
                this.genericMgr.Update("update CreateIpDAT set ErrorCount=ErrorCount+1 where ASN_NO=?", ipNo);
            }
        }
    }
}
