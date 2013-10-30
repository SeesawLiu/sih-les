namespace com.Sconit.Entity.MD
{
    public partial class Picker
    {
        public string CodeDescription
        {
            get
            {
                return this.Code + " [" + this.Description + "]";
            }
        }
    }
}
