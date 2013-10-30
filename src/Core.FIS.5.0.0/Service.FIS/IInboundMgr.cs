using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.Entity.FIS;

namespace com.Sconit.Service.FIS
{
    public interface IInboundMgr
    {
        void ProcessInboundFile(InboundControl inboundControl, string[] files);
    }
}
