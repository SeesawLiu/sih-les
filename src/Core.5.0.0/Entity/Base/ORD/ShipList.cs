using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class ShipList : EntityBase, IAuditable
    {
        [Display(Name = "ShipList_ShipNo", ResourceType = typeof(Resources.ORD.ShipList))]
        public string ShipNo { get; set; }
        [Display(Name = "ShipList_Vehicle", ResourceType = typeof(Resources.ORD.ShipList))]
        public string Vehicle { get; set; }
        [Display(Name = "ShipList_Shipper", ResourceType = typeof(Resources.ORD.ShipList))]
        public string Shipper { get; set; }
        [Display(Name = "ShipList_Status", ResourceType = typeof(Resources.ORD.ShipList))]
        public com.Sconit.CodeMaster.OrderStatus Status { get; set; }
        
        public Int32 CloseUser { get; set; }
        [Display(Name = "ShipList_CloseUserNm", ResourceType = typeof(Resources.ORD.ShipList))]
        public string CloseUserNm { get; set; }
        [Display(Name = "ShipList_CloseDate", ResourceType = typeof(Resources.ORD.ShipList))]
        public DateTime? CloseDate { get; set; }
        public Int32 CancelUser { get; set; }
        [Display(Name = "ShipList_CancelUserNm", ResourceType = typeof(Resources.ORD.ShipList))]
        public string CancelUserNm { get; set; }
        [Display(Name = "ShipList_CancelDate", ResourceType = typeof(Resources.ORD.ShipList))]
        public DateTime? CancelDate { get; set; }

        public Int32 CreateUserId { get; set; }
        [Display(Name = "ShipList_CreateUserNm", ResourceType = typeof(Resources.ORD.ShipList))]
        public string CreateUserName { get; set; }
        [Display(Name = "ShipList_CreateDate", ResourceType = typeof(Resources.ORD.ShipList))]
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }

        public override int GetHashCode()
        {
            if (ShipNo != null)
            {
                return ShipNo.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            ShipList another = obj as ShipList;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.ShipNo == another.ShipNo);
            }
        } 
    }
}
