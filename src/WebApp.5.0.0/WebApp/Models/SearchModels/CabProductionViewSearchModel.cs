namespace com.Sconit.Web.Models.SearchModels
{
    public class CabProductionViewSearchModel : SearchModelBase
    {
        public string Flow { get; set; }
        public CodeMaster.CabType Type { get; set; }
        public bool IsOut { get; set; }
    }
}