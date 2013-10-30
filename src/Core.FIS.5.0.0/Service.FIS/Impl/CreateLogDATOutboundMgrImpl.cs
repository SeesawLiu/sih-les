using System;
using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity.FIS;
using com.Sconit.Persistence;
using com.Sconit.Service.FIS;
using com.Sconit.Utility;

namespace com.Sconit.Service.FIS.Impl
{
    public class CreateLogDATOutboundMgrImpl : AbstractOutboundMgr
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
                IList<LesINLog> lesINLogList = this.genericMgr.FindAll<LesINLog>(@"from LesINLog as l where l.IsCreateDat=0");
                List<string> fileNames = lesINLogList.Select(o => o.FileName).Distinct().ToList();
                nowTime = System.DateTime.Now.ToString("yyyyMMddHHmmss");
                //int i = 10;
                foreach (var fileName in fileNames)
                {
                    List<string[]> extractData = new List<string[]>();
                    var ids = string.Join( "','",lesINLogList.Where(o => o.FileName == fileName).Select(o => o.Id).ToArray());
                    foreach (var lesINLog in lesINLogList.Where(o => o.FileName == fileName))
                    {
                        List<string> data = new List<string>();
                        data.Add(ids);
                        data.Add(lesINLog.Type + "\t");
                        data.Add(lesINLog.MoveType + "\t");
                        data.Add(lesINLog.Id + "\t");
                        data.Add(lesINLog.PO + "\t");
                        data.Add(lesINLog.POLine + "\t");
                        data.Add(lesINLog.WMSNo + "\t");
                        data.Add(lesINLog.WMSLine + "\t");
                        data.Add(lesINLog.HandTime.ToString("yyMMddHHmmss") + "\t");
                        data.Add(lesINLog.Item + "\t");
                        data.Add(lesINLog.HandResult + "\t");
                        data.Add(lesINLog.ErrorCause + "\t");
                        data.Add(lesINLog.Qty + "\t");
                        data.Add(lesINLog.QtyMark + "\t");
                        extractData.Add(data.ToArray());
                    }
                    //string fileName = lesINLogList.Where(o => o.FileName == fileName).First().HandTime;
                    //result.Add(fileName + (i++) + ".DAT", extractData.ToArray());
                    result.Add("LOGLE" + nowTime + (i++) + ".DAT", extractData.ToArray());
                }
                base.BaseOutboundData(result, outboundControl);
            }
        }

        protected override void SaveOutboundData(string ids, Boolean successOrError,string fileName)
        {
            if (successOrError)
            {
                this.genericMgr.UpdateWithNativeQuery(string.Format("update FIS_LesINLog set IsCreateDat=1,UploadDate='{0}',LesFileName='{1}' where Id in('{2}')",System.DateTime.Now, fileName, ids ));

            }
        }
    }
}
