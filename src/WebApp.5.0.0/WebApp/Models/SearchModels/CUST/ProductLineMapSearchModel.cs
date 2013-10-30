namespace com.Sconit.Web.Models.SearchModels.CUST
{
    public class ProductLineMapSearchModel : SearchModelBase
    {
        public string SAPProductLine { get; set; }
        public string ProductLine { get; set; }
        public bool SearchIsActive { get; set; }
    }
}