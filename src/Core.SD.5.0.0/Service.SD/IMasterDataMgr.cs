using System;
namespace com.Sconit.Service.SD
{
    public interface IMasterDataMgrImpl
    {
        Entity.SD.MD.Bin GetBin(string binCode);

        Entity.SD.MD.Location GetLocation(string locationCode);

        Entity.SD.MD.Item GetItem(string itemCode);

        DateTime GetEffDate(string date);

        string GetEntityPreference(Entity.SYS.EntityPreference.CodeEnum entityEnum);
    }
}
