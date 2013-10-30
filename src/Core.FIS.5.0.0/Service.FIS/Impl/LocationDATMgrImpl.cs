using System;
using System.Collections.Generic;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.FIS;

namespace com.Sconit.Service.FIS.Impl
{
    public class LocationDATMgrImpl : AbstractOutboundMgr
    {
        public static string nowTime = string.Empty;

        /// <summary>
        /// 每天执行一次
        /// </summary>
        /// <param name="huId"></param>
        /// <returns></returns>
        private static object ExporLocationLock = new object();
        protected override void ExtractOutboundData(string systemCode, OutboundControl outboundControl)
        {
            lock (ExporLocationLock)
            {
                var result = new Dictionary<string, string[][]>();
                var list = this.genericMgr.FindAll<Location>(@"from Location as c where (DATEDIFF(d,c.LastModifyDate,GETDATE())=0) and IsActive=? ", true);
                nowTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                if (list.Count > 0)
                {
                    List<string[]> extractData = new List<string[]>();
                    foreach (var barCodeDat in list)
                    {
                        var data = new List<string>
                                   {
                                       barCodeDat.Code,
                                       barCodeDat.Code + "\t",
                                       barCodeDat.Name + "\t",
                                       "0084\t"
                                   };
                        extractData.Add(data.ToArray());
                    }
                    result.Add("WARE" + nowTime + ".DAT", extractData.ToArray());
                }
                base.BaseOutboundData(result, outboundControl);
            }
        }

        protected override void SaveOutboundData(string systemcode, bool SuccessOrError, string fileName)
        {
            
        }
    }
}
