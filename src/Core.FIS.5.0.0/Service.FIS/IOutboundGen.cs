using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.Entity.FIS;
using System.Reflection;
using Castle.Windsor;

namespace com.Sconit.Service.FIS
{
    public interface IOutboundGen
    {
        void UploadFile();

        void ExportData(string systemCode, IWindsorContainer container);
    }
}
