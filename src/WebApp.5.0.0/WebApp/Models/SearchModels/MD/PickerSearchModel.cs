namespace com.Sconit.Web.Models.SearchModels.MD
{
    public class PickerSearchModel : SearchModelBase
    {
        public string SearchCode { get; set; }
        public string SearchDescription { get; set; }
        public string SearchLocation { get; set; }
        public string SearchUserCode { get; set; }
        public bool SearchIsActive { get; set; }
    }
}