using System;
using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity.FIS;

namespace com.Sconit.Service.FIS.Impl
{
    public class YieldReturnDATMgrImpl : AbstractOutboundMgr
    {
        public static string nowTime = string.Empty;
        public static int i = 10;
        private static object ExporYieldReturnLock = new object();
        protected override void ExtractOutboundData(string systemCode, OutboundControl outboundControl)
        {
            lock (ExporYieldReturnLock)
            {
                if (i == 99) i = 10;
                var result = new Dictionary<string, string[][]>();
                var list = this.genericMgr.FindAll<YieldReturn>(@"from YieldReturn as c where c.IsCreateDat=0 order by Id asc");
                List<string> ipNos = list.Select(o => o.IpNo).Distinct().ToList();
                nowTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                foreach (var ipNo in ipNos)
                {
                    List<string[]> extractData = new List<string[]>();
                    foreach (var m in list.Where(c => c.IpNo == ipNo))
                    {
                        var data = new List<string>
                                        {
                                            m.IpNo,
                                            m.IpNo + "\t",
                                            m.ArriveTime + "\t",
                                            m.PartyFrom + "\t",
                                            m.PartyTo + "\t",
                                            m.Dock + "\t",
                                            m.CreateDate.ToString("yyyy/MM/dd HH:mm:ss") + "\t",
                                            m.Seq + "\t",
                                            m.Item + "\t",
                                            m.ManufactureParty + "\t",
                                            m.Qty + "\t",
                                            m.IsConsignment + "\t"
                                        };
                        extractData.Add(data.ToArray());
                    }
                    var fileName = "RETU" + nowTime + (i++) + ".DAT";
                    result.Add(fileName, extractData.ToArray());
                }
                base.BaseOutboundData(result, outboundControl);
            }
        }

       
        protected override void SaveOutboundData(string ipNo, Boolean successOrError, string fileName)
        {
            if (successOrError)
            {
                this.genericMgr.Update("update YieldReturn set IsCreateDat=?,DATFileName=?,CreateDATDate=? where IpNo=?", new object[] { true, fileName, System.DateTime.Now, ipNo.Replace("\t", string.Empty) });
            }
        }
    }
}
