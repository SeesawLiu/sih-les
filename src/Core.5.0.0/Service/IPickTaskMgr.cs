using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.INV;

namespace com.Sconit.Service
{
    public interface IPickTaskMgr
    {
        //创建拣货任务
        void CreatePickTask(string orderno);
        void CreatePickTask(OrderMaster orderMaster);
        void CreatePickTask(string orderno, IList<int> orderDetailIds);
        //取消订单，先判断是否有拣货，如果没有则在取消订单后冻结所有相关拣货任务
        Boolean IsOrderPicked(string orderno);
        void CancelAllPickTask(string orderno);
        //冻结解冻
        void HoldPickTask(IList<string> pickIds);
        void UnholdPickTask(IList<string> pickIds);
        //分派
        void AssignPickTask(IList<string> pickIds, IList<string> pickers);
        string GetDefaultPicker(PickTask task);
        //配送条码
        IList<Hu> GetHuByPickTask(PickTask task);

        IList<PickTask> GetPickerTasks(string picker);
        IList<string> GetUnpickedHu(string pickid);

        //拣货结果相关
        void Pick(string pickid, string pickedhu, string picker);
        void Pick(string pickedhu, string picker);
        void CancelPickResult(PickResult result);

        //发货相关
        void CheckHuOnShip(string pickedhu, string picker);
        string ShipPerOrder(IList<string> pickedhus, string vehicleno, string picker);
        string ShipPerFlow(IList<string> pickedhus, string vehicleno, string picker);
    }
}
