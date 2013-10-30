using System;
using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity.FIS;
using com.Sconit.Persistence;
using com.Sconit.Service.FIS;
using com.Sconit.Utility;

namespace com.Sconit.Service.FIS.Impl
{
    public class CreateSeqOrderDATOutboundMgrImpl : AbstractOutboundMgr
    {
        //public IGenericMgr genericMgr { get; set; }

        public static string nowTime = string.Empty;
        public static int i = 10;
        private static object ExportSeqOrderDATLock = new object();
        protected override void ExtractOutboundData(string systemCode, OutboundControl outboundControl)
        {
            lock (ExportSeqOrderDATLock)
            {
                if (i == 99) i = 10;
                Dictionary<string, string[][]> result = new Dictionary<string, string[][]>();
                IList<CreateSeqOrderDAT> createSeqOrderDATList = this.genericMgr.FindAll<CreateSeqOrderDAT>(@"from CreateSeqOrderDAT as c where c.IsCreateDat=0 and c.ErrorCount<5 and Qty>0");
                List<string> seqNos = createSeqOrderDATList.Select(o => o.SeqNo).Distinct().ToList();
                nowTime = System.DateTime.Now.ToString("yyyyMMddHHmmss");
                //int i = 10;
                foreach (var seqNo in seqNos)
                {
                    List<string[]> extractData = new List<string[]>();
                    foreach (var createSeqOrderDAT in createSeqOrderDATList.Where(o => o.SeqNo == seqNo))
                    {
                        List<string> data = new List<string>();
                        data.Add(createSeqOrderDAT.SeqNo);
                        data.Add(createSeqOrderDAT.SeqNo + "\t");
                        data.Add(createSeqOrderDAT.Seq + "\t");
                        data.Add(createSeqOrderDAT.Flow + "\t");
                        data.Add(createSeqOrderDAT.StartTime.ToString("yyyy/MM/dd HH:mm:ss") + "\t");
                        data.Add(createSeqOrderDAT.WindowTime.ToString("yyyy/MM/dd HH:mm:ss") + "\t");
                        data.Add(createSeqOrderDAT.PartyFrom + "\t");
                        data.Add(createSeqOrderDAT.PartyTo + "\t");
                        data.Add(createSeqOrderDAT.LocationTo + "\t");
                        data.Add(createSeqOrderDAT.Container + "\t");
                        data.Add(createSeqOrderDAT.CreateDate.ToString("yyyy/MM/dd HH:mm:ss") + "\t");
                        data.Add(createSeqOrderDAT.Item + "\t");
                        data.Add(createSeqOrderDAT.ManufactureParty + "\t");
                        data.Add(createSeqOrderDAT.Qty + "\t");
                        data.Add(createSeqOrderDAT.SequenceNumber + "\t");
                        data.Add(createSeqOrderDAT.Van + "\t");
                        data.Add(createSeqOrderDAT.Line + "\t");
                        data.Add(createSeqOrderDAT.Station + "\t");
                        data.Add(createSeqOrderDAT.OrderDetId.ToString() + "\t");
                        extractData.Add(data.ToArray());
                    }
                    result.Add("SEQ" + nowTime + (i++) + ".DAT", extractData.ToArray());
                }
                base.BaseOutboundData(result, outboundControl);
            }
        }

        protected override void SaveOutboundData(string seqNo, Boolean successOrError,string fileName)
        {
            if (successOrError)
            {
                this.genericMgr.Update("update CreateSeqOrderDAT set IsCreateDat=?,UploadDate=?,FileName=? where SeqNo=?", new object[] { true, System.DateTime.Now,fileName, seqNo.Replace("\t", string.Empty) });
            }
            else
            {
                this.genericMgr.Update("update CreateSeqOrderDAT set ErrorCount=ErrorCount+1 where SeqNo=?", seqNo.Replace("\t", string.Empty));
            }
        }
    }
}
