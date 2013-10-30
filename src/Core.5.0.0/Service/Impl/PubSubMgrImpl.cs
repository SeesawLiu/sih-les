using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace com.Sconit.Service.Impl
{
    public class PubSubMgrImpl : BaseMgr, IPubSubMgr
    {
        public string EndpointAddress { get; set; }

        public IPublishing CreateProxy()
        {
            string endpointAddressInString = EndpointAddress;
            EndpointAddress endpointAddress = new EndpointAddress(endpointAddressInString);
            //WSDualHttpBinding wsDualHttpBinding = new WSDualHttpBinding();
            //_proxy = ChannelFactory<IPublishing>.CreateChannel(wsDualHttpBinding, endpointAddress);
            NetTcpBinding netTcpBinding = new NetTcpBinding(SecurityMode.None);
            return ChannelFactory<IPublishing>.CreateChannel(netTcpBinding, endpointAddress);
        }
    }
}
