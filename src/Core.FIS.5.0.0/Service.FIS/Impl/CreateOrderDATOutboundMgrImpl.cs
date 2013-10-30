using System;
using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity.FIS;
using com.Sconit.Persistence;
using com.Sconit.Service.FIS;
using com.Sconit.Utility;

namespace com.Sconit.Service.FIS.Impl
{
    public class CreateOrderDATOutboundMgrImpl : AbstractOutboundMgr
    {
        //public IGenericMgr genericMgr { get; set; }

        public static string nowTime = string.Empty;
        public static int i = 10;
        private static object ExportOrderDATLock = new object();
        protected override void ExtractOutboundData(string systemCode, OutboundControl outboundControl)
        {
            lock (ExportOrderDATLock)
            {
                if (i == 99) i = 10;
                Dictionary<string, string[][]> result = new Dictionary<string, string[][]>();
                IList<CreateOrderDAT> createOrderDATList = this.genericMgr.FindAll<CreateOrderDAT>(@"from CreateOrderDAT as c where c.IsCreateDat=0 and c.ErrorCount<5");
                List<string> orderNos = createOrderDATList.Select(o => o.OrderNo).Distinct().ToList();
                nowTime = System.DateTime.Now.ToString("yyyyMMddHHmmss");
                //int i = 10;
                foreach (var orderNo in orderNos)
                {
                    List<string[]> extractData = new List<string[]>();
                    foreach (var createOrderDAT in createOrderDATList.Where(o => o.OrderNo == orderNo))
                    {
                        List<string> data = new List<string>();
                        data.Add(createOrderDAT.OrderNo);
                        data.Add(createOrderDAT.MATNR + "\t");
                        data.Add(createOrderDAT.LIFNR + "\t");
                        data.Add(createOrderDAT.ENMNG + "\t");
                        data.Add(createOrderDAT.CHARG + "\t");
                        data.Add(createOrderDAT.COLOR + "\t");
                        data.Add(createOrderDAT.TIME_STAMP + "\t");
                        data.Add(createOrderDAT.CY_SEQNR + "\t");
                        data.Add(nowTime + "\t");
                        data.Add(createOrderDAT.AUFNR + "\t");
                        data.Add(createOrderDAT.LGORT + "\t");
                        data.Add(createOrderDAT.UMLGO + "\t");
                        data.Add((!string.IsNullOrWhiteSpace(createOrderDAT.LGPBE)? createOrderDAT.LGPBE : createOrderDAT.UMLGO) + "\t");
                        data.Add(createOrderDAT.REQ_TIME_STAMP + "\t");
                        data.Add(createOrderDAT.FLG_SORT + "\t");
                        data.Add(createOrderDAT.PLNBEZ + "\t");
                        data.Add(createOrderDAT.KTEXT + "\t");
                        data.Add(createOrderDAT.ZPLISTNO);
                        extractData.Add(data.ToArray());
                    }
                    result.Add("SEQ1LE" + nowTime + (i++) + ".DAT", extractData.ToArray());
                }
                base.BaseOutboundData(result, outboundControl);
            }
        }

        protected override void SaveOutboundData(string orderNO, Boolean successOrError,string fileName)
        {
            if (successOrError)
            {
                this.genericMgr.Update("update CreateOrderDAT set IsCreateDat=?,TIME_STAMP1=?,FileName=? where OrderNo=?", new object[] {true, nowTime,fileName, orderNO.Replace("\t", string.Empty) });
                // IList<CreateIpDAT> createIpDATList = this.genericMgr.FindAll<CreateIpDAT>(@"from CreateIpDAT as c where c.IpNo=?",ipNo);
            }
            else {
                this.genericMgr.Update("update CreateOrderDAT set ErrorCount=ErrorCount+1 where OrderNo=?", orderNO.Replace("\t", string.Empty));
            }
        }
    }
}
