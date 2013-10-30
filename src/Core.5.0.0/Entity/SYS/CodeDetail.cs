
//TODO: Add other using statements here

using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.SYS
{
    public partial class CodeDetail
    {
        #region Non O/R Mapping Properties
        public enum SpecialValueEnum
        {
            BlankValue,
            AllValue
        }

        [CodeDetailDescriptionAttribute(CodeMaster = com.Sconit.CodeMaster.CodeMaster.ProdLineType, ValueField = "Description")]
        [Display(Name = "CodeDetail_Description", ResourceType = typeof(Resources.SYS.CodeDetail))]
        public string ChineDescription { get; set; }
        #endregion
    }
}