using System;
using System.ComponentModel.DataAnnotations;

namespace com.Sconit.Entity.PRD
{
    public partial class ShiftDetail
    {
        #region Non O/R Mapping Properties

        public static char ShiftTimeSplitSymbol = '~';

        public string StartEndTime
        {
            get
            {
                return this.StartTime + "~" + this.EndTime;
            }
        }

        [Display(Name = "ShiftDetail_ShiftCount", ResourceType = typeof(Resources.PRD.ShiftDetail))]
        public int ShiftCount { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        #endregion
    }
}