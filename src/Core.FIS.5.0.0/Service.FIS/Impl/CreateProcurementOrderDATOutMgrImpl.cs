using System;
using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity.FIS;
using com.Sconit.Persistence;
using com.Sconit.Service.FIS;
using com.Sconit.Utility;

namespace com.Sconit.Service.FIS.Impl
{
    public class CreateProcurementOrderDATOutMgrImpl : AbstractOutboundMgr
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
                IList<CreateProcurementOrderDAT> createOrderDATList = this.genericMgr.FindAll<CreateProcurementOrderDAT>(@"from CreateProcurementOrderDAT as c where c.IsCreateDat=?", false);
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
                        data.Add(createOrderDAT.OrderNo + "\t");
                        data.Add(createOrderDAT.Van + "\t");
                        data.Add(createOrderDAT.OrderStrategy + "\t");
                        data.Add(createOrderDAT.StartTime.ToString("yyyy/MM/dd HH:mm:ss") + "\t");
                        data.Add(createOrderDAT.WindowTime.ToString("yyyy/MM/dd HH:mm:ss") + "\t");
                        data.Add(createOrderDAT.Priority + "\t");
                        data.Add(createOrderDAT.Sequence + "\t");
                        data.Add(createOrderDAT.PartyFrom + "\t");
                        data.Add(createOrderDAT.PartyTo + "\t");
                        data.Add(createOrderDAT.Dock + "\t");
                        data.Add(nowTime + "\t");
                        data.Add(createOrderDAT.Flow + "\t");
                        data.Add(createOrderDAT.LineSeq + "\t");
                        data.Add(createOrderDAT.Item + "\t");
                        data.Add(createOrderDAT.ManufactureParty + "\t");
                        data.Add(createOrderDAT.LocationTo + "\t");
                        data.Add(createOrderDAT.Bin + "\t");
                        data.Add(createOrderDAT.OrderedQty + "\t");
                        data.Add(createOrderDAT.IsShipExceed + "\t");
                        extractData.Add(data.ToArray());
                    }
                    result.Add("SHIP" + nowTime + (i++) + ".DAT", extractData.ToArray());
                }
                base.BaseOutboundData(result, outboundControl);
            }
        }

        protected override void SaveOutboundData(string orderNo, Boolean successOrError,string fileName)
        {
            if (successOrError)
            {
                this.genericMgr.Update("update CreateProcurementOrderDAT set IsCreateDat=?,CreateDate=?,FileName=? where OrderNo=?", new object[] { true, System.DateTime.Now, fileName, orderNo.Replace("\t", string.Empty) });
            }
            //else {
            //    this.genericMgr.Update("update CreateOrderDAT set ErrorCount=ErrorCount+1 where OrderNo=?", orderNo.Replace("\t", string.Empty));
            //}
        }
    }
}
