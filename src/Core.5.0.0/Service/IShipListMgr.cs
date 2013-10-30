using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Sconit.Service
{
    public interface IShipListMgr
    {
        void CancelShipList(string shipNo);
        void CloseShipList(string shipNo);
        string CreateShipList(string Vehicle, string Shipper, string[] Ips);
    }
}
