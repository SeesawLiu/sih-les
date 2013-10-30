using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.Entity.FIS;

namespace com.Sconit.Service.FIS
{
    public interface IOutboundMgr
    {
        void ProcessOutbound(OutboundControl outboundControl);
    }
}
