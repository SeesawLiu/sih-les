using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.ORD
{
    [Serializable]
    public partial class PickResult : EntityBase, IAuditable
    {
        [Display(Name = "PickResult_ResultId", ResourceType = typeof(Resources.ORD.PickResult))]
        public string ResultId { get; set; }
        [Display(Name = "PickResult_PickId", ResourceType = typeof(Resources.ORD.PickResult))]
        public string PickId { get; set; }
        [Display(Name = "PickResult_PickedHu", ResourceType = typeof(Resources.ORD.PickResult))]
        public string PickedHu { get; set; }

        [Display(Name = "PickResult_HuQty", ResourceType = typeof(Resources.ORD.PickResult))]
        public Decimal HuQty { get; set; }
        [Display(Name = "PickResult_PickedQty", ResourceType = typeof(Resources.ORD.PickResult))]
        public Decimal PickedQty { get; set; }
        [Display(Name = "PickResult_Picker", ResourceType = typeof(Resources.ORD.PickResult))]
        public string Picker { get; set; }
        [Display(Name = "PickResult_PickDate", ResourceType = typeof(Resources.ORD.PickResult))]
        public DateTime PickDate { get; set; }
        //[Display(Name = "PickResult_AsnNo", ResourceType = typeof(Resources.ORD.PickResult))]
        //public string AsnNo { get; set; }
        [Display(Name = "PickResult_Memo", ResourceType = typeof(Resources.ORD.PickResult))]
        public string Memo { get; set; }

        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }
        public Boolean IsShip { get; set; }

        public override int GetHashCode()
        {
            if (ResultId != null)
            {
                return ResultId.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            PickResult another = obj as PickResult;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.ResultId == another.ResultId);
            }
        } 
    }
}
