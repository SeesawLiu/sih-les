using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Sconit.Service.SD
{
    public interface IPickTaskMgr
    {
        IList<com.Sconit.Entity.SD.ORD.PickTask> GetPickerTasks(string user);
        IList<string> GetUnpickedHu(string pickid);
        void Pick(string pickedhu, string user);
        void CheckHuOnShip(string pickedhu, string user);
        string Ship(IList<string> pickedhus, string vehicleno, string user);
    }
}
