using System;
using System.Collections;
using System.Collections.Generic;

//TODO: Add other using statements here

namespace com.Sconit.Entity.FIS
{
    [Serializable]
    public class FtpControl : EntityBase
    {
        #region O/R Mapping Properties

        public Int32 Id { get; set; }

        public string FtpServer { get; set; }

        public Int32? FtpPort { get; set; }

        public string FtpUser { get; set; }

        public string FtpPassword { get; set; }

        public string FtpTempFolder { get; set; }

        public string FtpFolder { get; set; }

        public string FilePattern { get; set; }

        public string VaildFilePattern { get; set; }

        public string LocalTempFolder { get; set; }

        public string LocalFolder { get; set; }

        public string IOType { get; set; }

        public string FtpErrorFolder { get; set; }

        public string FtpBackUp { get; set; }

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
            FtpControl another = obj as FtpControl;

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
