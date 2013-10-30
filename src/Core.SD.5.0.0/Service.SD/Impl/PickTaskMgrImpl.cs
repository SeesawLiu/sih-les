using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Services.Transaction;
using com.Sconit.Entity.Exception;
using AutoMapper;

namespace com.Sconit.Service.SD.Impl
{
    [Transactional]
    public class PickTaskMgrImpl : IPickTaskMgr
    {
        public IGenericMgr genericMgr { get; set; }
        public Service.IPickTaskMgr pickTaskMgr { get; set; }

        public void Pick(string pickedhu, string user)
        {
            pickTaskMgr.Pick(pickedhu, GetPickerByUser(user));
        }

        public void CheckHuOnShip(string pickedhu, string user)
        {
            pickTaskMgr.CheckHuOnShip(pickedhu, GetPickerByUser(user));
        }

        public string Ship(IList<string> pickedhus, string vehicleno, string user)
        {
            return pickTaskMgr.ShipPerOrder(pickedhus, vehicleno, GetPickerByUser(user));
        }

        public IList<Entity.SD.ORD.PickTask> GetPickerTasks(string user)
        {
            IList<Entity.ORD.PickTask> pts = pickTaskMgr.GetPickerTasks(GetPickerByUser(user));
            if (pts != null)
            {
                return Mapper.Map<IList<Entity.ORD.PickTask>, List<Entity.SD.ORD.PickTask>>(pts).OrderBy(s => s.PickId).ToList();
            }
            else {
                return new List<Entity.SD.ORD.PickTask>();
            }
        }

        public IList<string> GetUnpickedHu(string pickid)
        {
            return pickTaskMgr.GetUnpickedHu(pickid);
        }

        private string GetPickerByUser(string user)
        {
            Entity.MD.Picker picker = this.genericMgr.FindAll<Entity.MD.Picker>("from Picker where UserCode = ? ",
                                            user).SingleOrDefault();
            if (picker == null)
            {
                throw new BusinessException(string.Format("找不到用户{0}对应的拣货工", user));
            }
            else {
                return picker.Code;
            }
        }
    }
}
