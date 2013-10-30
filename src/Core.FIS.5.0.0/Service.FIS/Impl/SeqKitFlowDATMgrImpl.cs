using System;
using System.Collections.Generic;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.FIS;

namespace com.Sconit.Service.FIS.Impl
{
    public class SeqKitFlowDATMgrImpl : AbstractOutboundMgr
    {
        public static string nowTime = string.Empty;


        private static object ExporSeqKitFlowLock = new object();
        protected override void ExtractOutboundData(string systemCode, OutboundControl outboundControl)
        {
            lock (ExporSeqKitFlowLock)
            {
                var result = new Dictionary<string, string[][]>();
                var list = this.genericMgr.FindAll<FlowMaster>(@"select f from FlowMaster as f where f.PartyFrom=? and f.FlowStrategy=? ", new object[] { "LOC", (int)com.Sconit.CodeMaster.FlowStrategy.SEQ });
                nowTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                if (list.Count > 0)
                {
                    List<string[]> extractData = new List<string[]>();
                    foreach (var flowMaster in list)
                    {
                        var data = new List<string>
                                   {
                                       flowMaster.Code,
                                       flowMaster.Code + "\t",
                                       flowMaster.Description + "\t",
                                       "0084\t",
                                       flowMaster.Type + "\t",
                                   };
                        extractData.Add(data.ToArray());
                    }
                    result.Add("FLOW" + nowTime + ".DAT", extractData.ToArray());
                }
                base.BaseOutboundData(result, outboundControl);
            }
        }

        protected override void SaveOutboundData(string systemcode, bool SuccessOrError, string fileName)
        {
            
        }
    }
}
