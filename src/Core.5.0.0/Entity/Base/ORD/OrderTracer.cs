using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class OrderTracer : EntityBase
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
		public string Code { get; set; }
		public DateTime? ReqTime { get; set; }
		public string Item { get; set; }
		public Decimal? OrderedQty { get; set; }
		public Decimal? FinishedQty { get; set; }
		public Decimal? Qty { get; set; }
		public Int32? RefOrderLocTransId { get; set; }
        public Int32 OrderDetailId { get; set; }
        
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
            OrderTracer another = obj as OrderTracer;

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
