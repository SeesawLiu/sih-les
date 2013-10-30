using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity.INV;

//TODO: Add other using statements here

namespace com.Sconit.Entity.ORD
{
    public partial class PickListDetail
    {
        #region Non O/R Mapping Properties

        //TODO: Add Non O/R Mapping Properties here. 

        #endregion
        public decimal CurrentPickQty { get; set; }

        public IList<PickListDetailInput> PickListDetailInputs { get; set; }

        public void AddPickListDetailInput(PickListDetailInput pickListDetailInput)
        {
            if (PickListDetailInputs == null)
            {
                PickListDetailInputs = new List<PickListDetailInput>();
            }
            PickListDetailInputs.Add(pickListDetailInput);
        }

        public IList<string> GetPickedHuList()
        {
            if (PickListDetailInputs != null)
            {
                return PickListDetailInputs.Select(i => i.HuId).ToList();
            }

            return null;
        }

        public OrderDetail CurrentOrderDetail { get; set; }
    }

    public class PickLocationDetail
    {
        public string Item { get; set; }
        public decimal UnitCount { get; set; }
        public string Uom { get; set; }
        public string ManufactureParty { get; set; }
        public string LotNo { get; set; }
        public string Area { get; set; }
        public string Bin { get; set; }
        public decimal Qty { get; set; }
        public bool IsOdd { get; set; }
        public bool IsDevan { get; set; }

        public OrderDetail OrderDetail { get; set; }
        public string PickStrategy { get; set; }
        public bool IsInventory { get; set; }  //ÊÇ·ñ¿â´æ£¬false´ú±í¿â´æ²»×ã
    }

    public class PickListDetailInput
    {
        public string HuId { get; set; }

        //¸¨Öú×Ö¶Î
    }
}