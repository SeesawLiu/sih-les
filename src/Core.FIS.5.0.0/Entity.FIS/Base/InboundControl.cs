using System;
using System.Collections;
using System.Collections.Generic;

//TODO: Add other using statements here

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public class InboundControl : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        public string InFloder { get; set; }
        public string SystemCode { get; set; }
        public string FilePattern { get; set; }
        public string FtpFolder { get; set; }
        public string ServiceName { get; set; }
        public string ArchiveFloder { get; set; }
        public string ErrorFloder { get; set; }
        public Int32 Sequence { get; set; }
        public string FileEncoding { get; set; }
        #endregion

        public override int GetHashCode()
        {
            if (Id != null)
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
            InboundControl another = obj as InboundControl;

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
