using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.Sconit.Web.Models.SearchModels.SCM
{
    public class SequenceGroupSearchModel : SearchModelBase
    {
        public string Code { get; set; }

        public string ProdLine { get; set; }

        public string AppVanSeries { get; set; }

        public bool IsActive { get; set; }
    }
}