using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.SAP.ORD
{
    [Serializable]
    public partial class TableIndex : SAPEntityBase
    {
        #region O/R Mapping Properties
		
		public string TableName { get; set; }
        public Int64 Id { get; set; }
		public DateTime LastModifyDate { get; set; }
        public Int32 Version { get; set; }
        #endregion

		public override int GetHashCode()
        {
			if (TableName != null)
            {
                return TableName.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            TableIndex another = obj as TableIndex;

            if (another == null)
            {
                return false;
            }
            else
            {
            	return (this.TableName == another.TableName);
            }
        } 
    }
	
}
