using System;

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public partial class FormatControl : EntityBase
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
        public string SystemCode { get; set; }
		public Int32 Sequence { get; set; }
		public Int32 StartPos { get; set; }
		public Int32 FieldLen { get; set; }
        
        #endregion

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
            FormatControl another = obj as FormatControl;

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
