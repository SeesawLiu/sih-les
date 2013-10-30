using System;
using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity.FIS;
using com.Sconit.Persistence;
using com.Sconit.Service.FIS;
using com.Sconit.Utility;

namespace com.Sconit.Service.FIS.Impl
{
    public class KanBanRecOutboundMgrImpl : AbstractOutboundMgr
    {
        //public IGenericMgr genericMgr { get; set; }


        private static object ExporEdiOrderInfoLock = new object();
        protected override void ExtractOutboundData(string systemCode, OutboundControl outboundControl)
        {
            lock (ExporEdiOrderInfoLock)
            {
                Dictionary<string, string[][]> result = new Dictionary<string, string[][]>();
                IList<EdiOrderInfo> ediOrderInfoList = this.genericMgr.FindAll<EdiOrderInfo>(@"from EdiOrderInfo as ei where ei.OrderStrategy in(2,3) 
                    and ei.IsDelayOrder=0 and ei.IsCreateByPO=0 and Type=1 and Priority=0 
                    and ei.Status in (0,2)  and IsChangeOrder=0 and ei.IsCreateBySA=0  ");
                List<string> orderNos = ediOrderInfoList.Select(o => o.OrderNo).Distinct().ToList();
                foreach (var orderNo in orderNos)
                {
                    List<string[]> extractData = new List<string[]>();
                    foreach (var ediOrderInfo in ediOrderInfoList.Where(o => o.OrderNo == orderNo))
                    {
                        List<string> data = new List<string>();

                        data.Add(systemCode);
                        data.Add("SEM");
                        data.Add(ediOrderInfo.Supplier);
                        data.Add(DateTime.Now.ToString("yyyyMMddHHmm"));
                        data.Add(ediOrderInfo.Supplier);
                        data.Add(ediOrderInfo.Item);
                        data.Add(ediOrderInfo.WindowTime.ToString("yyyyMMdd"));
                        data.Add(ediOrderInfo.WindowTime.ToString("HHmm"));
                        //data.Add(SystemHelper.IsInterate(ediOrderInfo.OrderQty)?((int)ediOrderInfo.OrderQty).ToString():ediOrderInfo.OrderQty.ToString());
                        // data.Add(SystemHelper.IsInterate(ediOrderInfo.ShipQty)?((int)ediOrderInfo.ShipQty).ToString():ediOrderInfo.ShipQty.ToString());
                        // data.Add(SystemHelper.IsInterate(ediOrderInfo.ReciveQty)?((int)ediOrderInfo.ReciveQty).ToString():ediOrderInfo.ReciveQty.ToString());
                        data.Add("R07");
                        data.Add("4");
                        data.Add(ediOrderInfo.OrderNo);
                        data.Add(ediOrderInfo.Sequence.ToString());
                        data.Add("");
                        data.Add(ediOrderInfo.Bin);
                        data.Add("");
                        data.Add(ediOrderInfo.PONo);
                        data.Add(ediOrderInfo.Dock);
                        data.Add(ediOrderInfo.UCDesc);
                        // data.Add(SystemHelper.IsInterate(ediOrderInfo.UnitCount)?((int)ediOrderInfo.UnitCount).ToString():ediOrderInfo.UnitCount.ToString());
                        data.Add(ediOrderInfo.Type == 1 ? "1" : "3");
                        data.Add(ediOrderInfo.OrderStrategy == 2 ? "0" : "1");
                        data.Add("");
                        data.Add(ediOrderInfo.IsInspect ? "1" : "0");
                        extractData.Add(data.ToArray());
                    }

                    result.Add(orderNo, extractData.ToArray());
                }

                base.BaseOutboundData(result, outboundControl);
            }
        }

        protected override void SaveOutboundData(string ipNo, Boolean successOrError)
        {
            if (successOrError)
            {
                this.genericMgr.Update("update CreateIpDAT set IsCreateDat=1 where IpNo=?", ipNo);
                // IList<CreateIpDAT> createIpDATList = this.genericMgr.FindAll<CreateIpDAT>(@"from CreateIpDAT as c where c.IpNo=?",ipNo);
            }
            else
            {
                this.genericMgr.Update("update CreateIpDAT set ErrorCount=ErrorCount+1 where IpNo=?", ipNo);
            }
        }
    }
}
