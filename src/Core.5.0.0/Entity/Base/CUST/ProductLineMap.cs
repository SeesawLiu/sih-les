using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.CUST
{
    [Serializable]
    public partial class ProductLineMap : EntityBase
    {
        #region O/R Mapping Properties
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "ProductLineMap_SAPProductLine", ResourceType = typeof(Resources.CUST.ProductLineMap))]
        public string SAPProductLine { get; set; }

        [Display(Name = "ProductLineMap_ProductLine", ResourceType = typeof(Resources.CUST.ProductLineMap))]
        public string ProductLine { get; set; }

        [Display(Name = "ProductLineMap_TransmissionFlow", ResourceType = typeof(Resources.CUST.ProductLineMap))]
        public string TransmissionFlow { get; set; }

        [Display(Name = "ProductLineMap_SaddleFlow", ResourceType = typeof(Resources.CUST.ProductLineMap))]
        public string SaddleFlow { get; set; }

        public CodeMaster.ProductLineMapType Type { get; set; }

        [Display(Name = "ProductLineMap_CabProdLine", ResourceType = typeof(Resources.CUST.ProductLineMap))]
        public string CabProdLine { get; set; }

        [Display(Name = "ProductLineMap_ChassisProdLine", ResourceType = typeof(Resources.CUST.ProductLineMap))]
        public string ChassisProdLine { get; set; }

        [Display(Name = "ProductLineMap_AssemblyProdLine", ResourceType = typeof(Resources.CUST.ProductLineMap))]
        public string AssemblyProdLine { get; set; }

        [Display(Name = "ProductLineMap_SpecialProdLine", ResourceType = typeof(Resources.CUST.ProductLineMap))]
        public string SpecialProdLine { get; set; }

        [Display(Name = "ProductLineMap_MaxOrderCount", ResourceType = typeof(Resources.CUST.ProductLineMap))]
        public Int32 MaxOrderCount { get; set; }

        [Display(Name = "ProductLineMap_InitialVanOrder", ResourceType = typeof(Resources.CUST.ProductLineMap))]
        public string InitialVanOrder { get; set; }

        [Display(Name = "ProductLineMap_Plant", ResourceType = typeof(Resources.CUST.ProductLineMap))]
        public string Plant { get; set; }

        [Display(Name = "ProductLineMap_IsActive", ResourceType = typeof(Resources.CUST.ProductLineMap))]
        public Boolean IsActive { get; set; }

        [Display(Name = "ProductLineMap_CheckProdLine", ResourceType = typeof(Resources.CUST.ProductLineMap))]
        public string CheckProdLine { get; set; }

        #endregion

        public override int GetHashCode()
        {
            if (SAPProductLine != null)
            {
                return SAPProductLine.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            ProductLineMap another = obj as ProductLineMap;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.SAPProductLine == another.SAPProductLine);
            }
        }
    }

}
