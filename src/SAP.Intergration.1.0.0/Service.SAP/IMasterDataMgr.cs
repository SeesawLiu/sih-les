using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Sconit.Service.SAP
{
    public interface IMasterDataMgr
    {
        void LoadSAPItems(string itemCode, string plantCode);

        void LoadSAPSuppliers(string supplierCode);

        void ImportSAPItem(IList<Entity.SAP.MD.Item> itemList);

        void GetSAPQuota(string itemCode, string plantCode);
    }
}
