using System;
using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity.FIS;

namespace com.Sconit.Service.FIS.Impl
{
    public class CreateBarCodeDATMgrImpl : AbstractOutboundMgr
    {
        public static string nowTime = string.Empty;
        public static string CreateBarCodeIds = string.Empty;
        private static object ExportBarCodeLock = new object();
        protected override void ExtractOutboundData(string huId, OutboundControl outboundControl)
        {
            lock (ExportBarCodeLock)
            {
                var result = new Dictionary<string, string[][]>();
                var list = this.genericMgr.FindAll<CreateBarCode>(@"from CreateBarCode as c where c.IsCreateDat =0 order by Id asc ");
                nowTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                list = list.OrderBy(l => l.Id).ToList();
                int count = list.Count % 2000 == 0 ? list.Count / 2000 : list.Count / 2000 + 1;
                for (int i = 0; i < count; i++)
                {
                    List<string[]> extractData = new List<string[]>();
                    var currentList = list.Skip(i * 2000).Take(2000);
                    var maxId = currentList.Max(c => c.Id);
                    var minId = currentList.Min(c => c.Id);
                    foreach (var barCodeDat in currentList)
                    {
                        var data = new List<string>
                                        {
                                            maxId.ToString()+","+minId.ToString(),
                                            barCodeDat.HuId + "\t",
                                            barCodeDat.LotNo + "\t",
                                            barCodeDat.Item + "\t",
                                            barCodeDat.Qty + "\t",
                                            barCodeDat.ManufactureParty + "\t",
                                            barCodeDat.ASN + "\t"
                                        };
                        extractData.Add(data.ToArray());
                    }
                    result.Add("HUID" + nowTime + i + ".DAT", extractData.ToArray());
                }
                base.BaseOutboundData(result, outboundControl);
            }
        }

        protected override void SaveOutboundData(string betId, bool SuccessOrError, string fileName)
        {
            if (SuccessOrError)
            {
                var ids = betId.Split(',');
                int maxId = Convert.ToInt32(ids[0]);
                int minId = Convert.ToInt32(ids[1]);
                this.genericMgr.Update("update CreateBarCode set DATFileName=?,CreateDATDate=?,IsCreateDat=? where Id between ? and ?", new object[] { fileName, DateTime.Now, true, minId, maxId });
                //this.genericMgr.Update(string.Format("update CreateBarCode set DATFileName='{0}',CreateDATDate='{1}' where Id in ({2})", fileName, DateTime.Now, CreateBarCodeIds));
            }
        }
    }
}
