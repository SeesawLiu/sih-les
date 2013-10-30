using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.Exception;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class ShipListMgrImpl : BaseMgr , IShipListMgr
    {
        #region 变量
        public IGenericMgr genericMgr { get; set; }
        public INumberControlMgr numberControlMgr { get; set; }
        public ISystemMgr systemMgr { get; set; }
        #endregion

        public void CancelShipList(string shipNo)
        {
            var shipList = this.genericMgr.FindById<ShipList>(shipNo);

            //只有submit状态订单可以cancel
            if (shipList.Status != CodeMaster.OrderStatus.Submit)
            {
                throw new BusinessException("不能取消状态为{1}的订单{0}。", shipList.ShipNo,
                       systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)shipList.Status).ToString()));
            }
            else
            {
                shipList.Status = CodeMaster.OrderStatus.Cancel;
                shipList.CancelDate = DateTime.Now;
                User user = SecurityContextHolder.Get();
                shipList.CancelUser = user.Id;
                shipList.CancelUserNm = user.Name;
                this.genericMgr.Update(shipList);
            }
        }

        public void CloseShipList(string shipNo)
        {
            var shipList = this.genericMgr.FindById<ShipList>(shipNo);

            //只有submit状态订单可以close
            if (shipList.Status != CodeMaster.OrderStatus.Submit)
            {
                throw new BusinessException("不能关闭状态为{1}的订单{0}。", shipList.ShipNo,
                       systemMgr.GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster.OrderStatus, ((int)shipList.Status).ToString()));
            }
            else
            {
                shipList.Status = CodeMaster.OrderStatus.Close;
                shipList.CloseDate = DateTime.Now;
                User user = SecurityContextHolder.Get();
                shipList.CloseUser = user.Id;
                shipList.CloseUserNm = user.Name;
                this.genericMgr.Update(shipList);
            }
        }

        public string CreateShipList(string Vehicle, string Shipper, string[] Ips)
        {
            ShipList shiplist = new ShipList();
            shiplist.ShipNo = GenerateShipNo();
            shiplist.Status = CodeMaster.OrderStatus.Submit;
            shiplist.Vehicle = Vehicle;
            shiplist.Shipper = Shipper;

            IList<IpMaster> ms = new List<IpMaster>();
            foreach (string ip in Ips) {
                IpMaster ipm = this.genericMgr.FindAll<IpMaster>("from IpMaster where IpNo = ?", ip).SingleOrDefault();
                if (ipm == null)
                {
                    throw new BusinessException("找不到送货单:" + ip);
                }
                else {
                    ipm.ShipNo = shiplist.ShipNo;
                    ms.Add(ipm);
                }
            }

            foreach (IpMaster m in ms) {
                this.genericMgr.Update(m);
            }

            this.genericMgr.Create(shiplist);

            return shiplist.ShipNo;
        }

        private string GenerateShipNo() {
            string prefix = "SN";
            string seq = this.numberControlMgr.GetNextSequence("PickTask_ShipList");

            return prefix + string.Format("{0:D8}", Int32.Parse(seq));
        }
    }
}
