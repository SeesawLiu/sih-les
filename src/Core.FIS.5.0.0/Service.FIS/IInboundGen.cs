using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;

namespace com.Sconit.Service.FIS
{
    public interface IInboundGen
    {
        void DownloadFile();

        void ImportData(IWindsorContainer container);
    }
}
