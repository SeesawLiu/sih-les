using System;
using System.Xml.Serialization;

namespace com.Sconit.Entity.SAP.ORD
{
    [Serializable]
    public partial class PostDO : SAPEntityBase, ITraceable
    {
        #region O/R Mapping Properties
		
		public Int32 Id { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 收货单号
        /// </summary>
        public string ReceiptNo { get; set; }

        public string Result { get; set; }

        public string ZTCODE { get; set; }

        public string Success { get; set; }

        [XmlIgnore]
        public StatusEnum Status { get; set; }
        [XmlIgnore]
        public DateTime CreateDate { get; set; }
        [XmlIgnore]
        public DateTime LastModifyDate { get; set; }
        [XmlIgnore]
        public Int32 ErrorCount { get; set; }
        
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
            PostDO another = obj as PostDO;

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
