using System;
using System.Collections;
using System.Collections.Generic;

//TODO: Add other using statements here

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public class OutboundControl : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        public string SystemCode { get; set; }

        public string OutFolder { get; set; }

        public string ServiceName { get; set; }

        public string ArchiveFolder { get; set; }

        public Int32 Sequence { get; set; }

        public string TempFolder { get; set; }

        public string FileEncoding { get; set; }
        public string FilePrefix { get; set; }
        public string FileSuffix { get; set; }
        public Boolean IsActive { get; set; }
        public Int32 Mark { get; set; }
        public string UndefinedString1 { get; set; }
        public string UndefinedString2 { get; set; }
        public string UndefinedString3 { get; set; }
        public string UndefinedString4 { get; set; }
        public string UndefinedString5 { get; set; }

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
            OutboundControl another = obj as OutboundControl;

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
