using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.LOG
{
    [Serializable]
    public partial class GenVanProdOrder : EntityBase
    {
        public Int32 Id { get; set; }

        [Display(Name = "GenVanProdOrder_AUFNR", ResourceType = typeof(Resources.LOG.GenVanProdOrder))]
        public string AUFNR { get; set; }

        [Display(Name = "GenVanProdOrder_ZLINE", ResourceType = typeof(Resources.LOG.GenVanProdOrder))]
        public string ZLINE { get; set; }

        [Display(Name = "GenVanProdOrder_ProdLine", ResourceType = typeof(Resources.LOG.GenVanProdOrder))]
        public string ProdLine { get; set; }

        [Display(Name = "GenVanProdOrder_BatchNo", ResourceType = typeof(Resources.LOG.GenVanProdOrder))]
        public string BatchNo { get; set; }

        [Display(Name = "GenVanProdOrder_Msg", ResourceType = typeof(Resources.LOG.GenVanProdOrder))]
        public string Msg { get; set; }

        [Display(Name = "GenVanProdOrder_CreateDate", ResourceType = typeof(Resources.LOG.GenVanProdOrder))]
        public DateTime CreateDate { get; set; }

        public override int GetHashCode()
        {
            if (Id != 0)
            {
                return Id.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            GenVanProdOrder another = obj as GenVanProdOrder;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.Id == another.Id);
            }
        }
    }
}
