using System;
using System.ComponentModel.DataAnnotations;
using com.Sconit.Entity.SYS;

namespace com.Sconit.Entity.KB
{
    [Serializable]
    public partial class KanbanLost : EntityBase,IAuditable
    {
        #region O/R Mapping Properties

        public int Id { get; set; }

        [Export(ExportName="ExportKanbanLost",ExportSeq=10)]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "Errors_Common_FieldRequired", ErrorMessageResourceType = typeof(Resources.ErrorMessage))]
        [Display(Name = "KanbanCard_CardNo", ResourceType = typeof(Resources.KB.KanbanCard))]
        public string CardNo { get; set; }

        public Int32 CreateUserId { get; set; }
        public string CreateUserName { get; set; }
        public DateTime CreateDate { get; set; }
        public Int32 LastModifyUserId { get; set; }
        public string LastModifyUserName { get; set; }
        public DateTime LastModifyDate { get; set; }

       
        #endregion

        public override int GetHashCode()
        {
            if (CardNo != null)
            {
                return CardNo.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            KanbanLost another = obj as KanbanLost;

            if (another == null)
            {
                return false;
            }
            else
            {
                return (this.CardNo == another.CardNo);
            }
        }
    }

}
