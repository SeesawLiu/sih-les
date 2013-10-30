using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Services.Transaction;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity;
using com.Sconit.Entity.ACC;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class PickTaskMgrImpl : BaseMgr, IPickTaskMgr
    {
        #region 变量
        public IGenericMgr genericMgr { get; set; }
        public ILocationDetailMgr locationDetailMgr { get; set; }
        public INumberControlMgr numberControlMgr { get; set; }
        public IHuMgr huMgr { get; set; }
        public IIpMgr ipMgr { get; set; }
        #endregion

        public void CreatePickTask(string orderno)
        {
            CreatePickTask(this.genericMgr.FindAll<OrderMaster>("from OrderMaster where OrderNo = ? ", orderno).SingleOrDefault());
        }

        public void CreatePickTask(OrderMaster orderMaster)
        {
            if (orderMaster.Status == CodeMaster.OrderStatus.Submit
             || orderMaster.Status == CodeMaster.OrderStatus.InProcess)
            {
                IList<OrderDetail> ods = this.genericMgr.FindAll<OrderDetail>("from OrderDetail where OrderNo = ? ", orderMaster.OrderNo);
                if (ods != null)
                {
                    foreach (OrderDetail od in ods)
                    {
                        if (od.IsCreatePickList)
                        {
                            PickTask task = this.genericMgr.FindAll<PickTask>("from PickTask where OrderNo = ? and OrdDetId = ? ",
                                new object[] { od.OrderNo, od.Id }).SingleOrDefault();
                            if (task == null)
                            {
                                CreatePickTaskFromOrder(orderMaster, od);
                            }
                            else
                            {
                                throw new BusinessException("task already exists!");
                            }
                        }
                    }
                }
            }
            else
            {
                throw new BusinessException("bad status!");
            }
        }

        public void CreatePickTask(string orderno, IList<int> orderDetailIds)
        {
            OrderMaster om = this.genericMgr.FindAll<OrderMaster>("from OrderMaster where OrderNo = ? ",
                orderno).SingleOrDefault();
            if (om != null)
            {
                if (om.Status == CodeMaster.OrderStatus.Submit
                 || om.Status == CodeMaster.OrderStatus.InProcess)
                {
                    foreach (int oid in orderDetailIds)
                    {
                        OrderDetail od = this.genericMgr.FindAll<OrderDetail>("from OrderDetail where Id = ?",
                            oid).SingleOrDefault();
                        if (od != null)
                        {
                            PickTask task = this.genericMgr.FindAll<PickTask>("from PickTask where OrderNo = ? and OrdDetId = ? ",
                                new object[] { od.OrderNo, od.Id }).SingleOrDefault();
                            if (task == null)
                            {
                                CreatePickTaskFromOrder(om, od);
                            }
                            else
                            {
                                throw new BusinessException("task already exists!");
                            }
                        }
                    }
                }
                else
                {
                    throw new BusinessException("bad status!");
                }
            }
        }

        private string GeneratePickTaskId()
        {
            string prefix = "PT";
            string seq = this.numberControlMgr.GetNextSequence("PickTask_Task");

            return prefix + string.Format("{0:D8}", Int32.Parse(seq));
        }

        private string GeneratePickResultId()
        {
            string prefix = "PR";
            string seq = this.numberControlMgr.GetNextSequence("PickTask_Result");

            return prefix + string.Format("{0:D8}", Int32.Parse(seq));
        }

        private PickTask CreatePickTaskFromOrder(OrderMaster orderMaster, OrderDetail od)
        {
            PickTask task = new PickTask();
            task.PickId = GeneratePickTaskId();
            task.OrderNo = orderMaster.OrderNo;
            task.OrdDetId = od.Id;
            task.DemandType = CodeMaster.PickDemandType.Purchase;
            task.IsHold = false;
            task.Flow = orderMaster.Flow;
            task.FlowDesc = orderMaster.FlowDescription;
            task.Item = od.Item;
            task.ItemDesc = od.ItemDescription;
            task.Uom = od.Uom;
            task.BaseUom = od.BaseUom;
            task.PartyFrom = orderMaster.PartyFrom;
            task.PartyFromName = orderMaster.PartyFromName;
            task.PartyTo = orderMaster.PartyTo;
            task.PartyToName = orderMaster.PartyToName;
            task.LocationFrom = od.LocationFrom;
            task.LocationFromName = od.LocationFromName;
            task.LocationTo = od.LocationTo;
            task.LocationToName = od.LocationToName;
            task.WindowTime = orderMaster.WindowTime;
            task.ReleaseDate = orderMaster.ReleaseDate.Value;
            task.Supplier = od.ManufactureParty;
            task.SupplierName = od.ManufactureParty;
            task.UnitCount = od.UnitCount;
            //可拣数
            IList<PickTask> pickTaskList = this.genericMgr.FindAll<PickTask>("from PickTask where OrdDetId = ?", od.Id);
            decimal? sumPickTaskReqQty = 0;
            decimal? sumPickTaskShipQty = 0;
            if (pickTaskList != null && pickTaskList.Count() > 0)
            {
                sumPickTaskReqQty = pickTaskList.Sum(p => p.OrderedQty);
                sumPickTaskShipQty = pickTaskList.Sum(p => p.ShippedQty);
            }
            task.OrderedQty = od.OrderedQty - od.ShippedQty - (sumPickTaskReqQty.HasValue ? sumPickTaskReqQty.Value : decimal.Zero) + (sumPickTaskShipQty.HasValue ? sumPickTaskShipQty.Value : decimal.Zero);
            task.PickedQty = 0;
            task.ShippedQty = 0;
            task.Picker = this.GetPicker(task.Item,task.LocationFrom);
            task.PrintCount = 0;
            task.Memo = "";

            this.genericMgr.Create(task);
            return task;
        }


        public bool IsOrderPicked(string orderno)
        {
            IList<PickTask> picked = this.genericMgr.FindAll<PickTask>("from PickTask where OrderNo = ? and PickedQty > 0 ",
                orderno);
            return (picked.Count > 0);
        }

        public void CancelAllPickTask(string orderno)
        {
            this.genericMgr.Update("Update PickTask set IsHold = 1 where OrderNo = ?", orderno);
        }

        public void HoldPickTask(IList<string> pickIds)
        {
            foreach (string pickid in pickIds)
            {
                this.genericMgr.Update("Update PickTask set IsHold = 1 where PickId = ?", pickid);
            }
        }

        public void UnholdPickTask(IList<string> pickIds)
        {
            foreach (string pickid in pickIds)
            {
                this.genericMgr.Update("Update PickTask set IsHold = 0 where PickId = ?", pickid);
            }
        }

        public void AssignPickTask(IList<string> pickIds, IList<string> pickers)
        {
            for (int i = 0; i < pickIds.Count; i++)
            {
                if (pickers[i].Equals("n/a"))
                {
                    pickers[i] = string.Empty;
                }
                this.genericMgr.Update("Update PickTask set Picker = ? where PickId = ?",
                    new object[] { pickers[i], pickIds[i] });
            }
        }


        public void CancelPickResult(PickResult result)
        {
            //查询拣货任务关联的拣货结果，取消拣货（删除拣货结果明细），更新累计拣货数（订单单位）
            //，恢复已经删除的配送条码PickHu，释放已占用的库存。
            //禁止取消AsnNo有值的拣货结果
            if (!result.IsShip)
            {
                PickTask task = this.genericMgr.FindAll<PickTask>("from PickTask where PickId = ? ", result.PickId).SingleOrDefault();
                if (task == null)
                {
                    throw new BusinessException("找不到拣货任务");
                }

                //恢复配送条码
                PickHu oldPickedHu = new PickHu();
                oldPickedHu.PickId = result.PickId;
                oldPickedHu.RepackHu = result.PickedHu;
                this.genericMgr.Create(oldPickedHu);

                //释放已占用库存
                locationDetailMgr.CancelInventoryOccupy(CodeMaster.OccupyType.Pick, result.ResultId);

                //更新累计数
                task.PickedQty -= result.PickedQty;
                this.genericMgr.Update(task);

                //删除result
                this.genericMgr.Delete(result);
            }
            else
            {
                throw new BusinessException("拣货条码已经发货,不能取消");
            }
        }


        public string GetDefaultPicker(PickTask task)
        {
            string defaultPicker = "";
            IList<PickRule> rules = this.genericMgr.FindAll<PickRule>("from PickRule where Item = ? and Location = ? Order by Id desc",
                new object[] { task.Item, task.LocationFrom });
            if (rules != null)
            {
                foreach (PickRule pr in rules)
                {
                    defaultPicker = pr.Picker;
                    break;
                }
            }

            if (defaultPicker != "")
            {
                return defaultPicker;
            }
            else
            {
                IList<Picker> pickers = this.genericMgr.FindAll<Picker>("from Picker where Location = ? ", task.LocationFrom);
                if (pickers != null)
                {
                    foreach (Picker p in pickers)
                    {
                        defaultPicker = p.Code;
                        break;
                    }
                }

                if (defaultPicker != "")
                {
                    return defaultPicker;
                }
            }

            return defaultPicker;
        }

        public IList<Hu> GetHuByPickTask(PickTask task)
        {
            IList<Hu> hus = new List<Hu>();

            IList<string> ph = this.genericMgr.FindAllWithNativeSql<string>("select RepackHu from ord_PickHu where PickId = ? ", task.PickId);
            if (ph != null && ph.Count > 0)
            {
                //已经生成过
                hus = huMgr.LoadHus(ph);
            }
            else
            {
                //第一次
                hus = huMgr.CreateHu(task, string.Empty);
                if (hus != null && hus.Count > 0)
                {
                    foreach (Hu h in hus)
                    {
                        PickHu newPh = new PickHu();
                        newPh.PickId = task.PickId;
                        newPh.RepackHu = h.HuId;

                        this.genericMgr.Create(newPh);
                    }
                }
            }

            return hus;
        }

        public void Pick(string pickid, string pickedhu, string picker)
        {
            PickTask task = this.genericMgr.FindAll<PickTask>("from PickTask where PickId = ? ", pickid).SingleOrDefault();
            if (task == null)
            {
                throw new BusinessException("找不到拣货任务");
            }
            else if (!task.Picker.Equals(picker))
            {
                throw new BusinessException("条码对应拣货工为" + task.Picker);
            }
            else if (task.IsHold)
            {
                throw new BusinessException("拣货任务已被冻结");
            }
            else if (task.PickedQty >= task.OrderedQty)
            {
                throw new BusinessException("拣货任务已经完成");
            }

            IList<Hu> hu = huMgr.LoadHus(new string[] { pickedhu });
            if (hu != null && hu.Count == 1)
            {
                //是否有足够库存并占用库存
                if (locationDetailMgr.CanInventoryOccupy(task.Supplier, task.LocationFrom, task.Item, CodeMaster.QualityType.Qualified, hu[0].Qty))
                {
                    //创建拣货结果
                    string resultid = GeneratePickResultId();
                    PickResult result = new PickResult();
                    result.ResultId = resultid;
                    result.PickId = task.PickId;
                    result.Picker = picker;
                    result.PickDate = DateTime.Now;
                    result.PickedHu = pickedhu;
                    result.PickedQty = hu[0].Qty;
                    result.IsShip = false;

                    locationDetailMgr.InventoryOccupy(CodeMaster.OccupyType.Pick, result.ResultId,
                        task.Supplier, task.LocationFrom, task.Item, CodeMaster.QualityType.Qualified, hu[0].Qty);

                    this.genericMgr.Create(result);

                    //更新拣货数
                    task.PickedQty += hu[0].Qty;
                    this.genericMgr.Update(task);

                    //删除配送条码
                    this.genericMgr.Delete("from PickHu where PickId = '"
                        + task.PickId + "' and RepackHu = '" + pickedhu + "'");
                }
                else
                {
                    throw new BusinessException("没有足够库存:" + task.Supplier + "," + task.LocationFrom + "," + task.Item);
                }
            }
            else
            {
                throw new BusinessException("找不到对应的条码!");
            }
        }

        public void Pick(string pickedhu, string picker)
        {
            PickHu findhu = this.genericMgr.FindAll<PickHu>("from PickHu where RepackHu = ?",
                         pickedhu).SingleOrDefault();
            if (findhu == null)
            {
                throw new BusinessException("找不到对应的条码!");
            }
            else
            {
                Pick(findhu.PickId, pickedhu, picker);
            }
        }

        public void CheckHuOnShip(string pickedhu, string picker)
        {
            IList<Hu> hu = huMgr.LoadHus(new string[] { pickedhu });
            if (hu != null && hu.Count == 1)
            {
                PickResult result = this.genericMgr.FindAll<PickResult>("from PickResult where PickedHu = ?",
                    pickedhu).SingleOrDefault();
                if (result != null)
                {
                    //已拣货已发货
                    if (result.IsShip)
                    {
                        throw new BusinessException("该条码已发货!");
                    }
                }
                else
                {
                    PickHu findhu = this.genericMgr.FindAll<PickHu>("from PickHu where RepackHu = ?",
                        pickedhu).SingleOrDefault();
                    if (findhu != null)
                    {
                        //尝试自动拣货
                        try
                        {
                            Pick(findhu.PickId, findhu.RepackHu, picker);
                        }
                        catch (BusinessException be)
                        {
                            throw new BusinessException("该条码执行拣货失败:" + be.GetMessages()[0]);
                        }
                    }
                    else
                    {
                        throw new BusinessException("该条码未拣货或已发货!");
                    }
                }
            }
            else
            {
                throw new BusinessException("该条码不存在!");
            }
        }

        [Transaction(TransactionMode.Requires)]
        public string ShipPerOrder(IList<string> pickedhus, string vehicleno, string picker)
        {
            IList<PickResult> results = new List<PickResult>();
            IDictionary<string, OrderDetail> odDict = new Dictionary<string, OrderDetail>();
            //IDictionary<string,PickTask> resultDict = new Dictionary<string,PickTask>();
            //IList<PickTask> pickTaskList = new List<PickTask>();
            IDictionary<string, PickTask> taskDict = new Dictionary<string, PickTask>();
            IDictionary<string, OrderMaster> ip = new Dictionary<string, OrderMaster>();
            foreach (string hu in pickedhus)
            {
                PickResult result = this.genericMgr.FindAll<PickResult>("from PickResult where PickedHu = ?",
                     hu).SingleOrDefault();
                if (result == null)
                {
                    throw new BusinessException("找不到拣货结果,条码:" + hu);
                }
                else
                {
                    results.Add(result);
                    if (!odDict.ContainsKey(result.PickId))
                    {
                        PickTask task = this.genericMgr.FindAll<PickTask>("from PickTask where PickId = ? ",
                            result.PickId).SingleOrDefault();
                        if (task == null)
                        {
                            throw new BusinessException("找不到拣货任务:" + result.PickId);
                        }
                        else
                        {
                            //记录pickid与task的映射关系
                            if (!taskDict.ContainsKey(result.PickId))
                            {
                                taskDict.Add(result.PickId, task);
                            }

                            //记录pickid与orderdetail的关系
                            OrderDetail od = this.genericMgr.FindAll<OrderDetail>("from OrderDetail where Id = ? ",
                                task.OrdDetId).SingleOrDefault();
                            if (od == null)
                            {
                                throw new BusinessException("找不到订单明细,ID:" + task.OrdDetId);
                            }
                            else
                            {
                                odDict.Add(result.PickId, od);
                            }
                        }
                    }

                    //添加占用明细
                    OrderDetailInput odi = new OrderDetailInput();
                    //将拣货条码记录到wmsipseq,后续收货需要使用
                    odi.WMSIpSeq = result.PickedHu;
                    odi.ShipQty = result.PickedQty;
                    odi.OccupyType = CodeMaster.OccupyType.Pick;
                    odi.OccupyReferenceNo = result.ResultId;
                    odi.ConsignmentParty = taskDict[result.PickId].Supplier;

                    odDict[result.PickId].AddOrderDetailInput(odi);
                }
            }//foreach hu

            IDictionary<string, OrderMaster> omDict = new Dictionary<string, OrderMaster>();
            foreach (KeyValuePair<string, OrderDetail> kv in odDict)
            {
                if (omDict.ContainsKey(kv.Value.OrderNo))
                {
                    omDict[kv.Value.OrderNo].AddOrderDetail(kv.Value);
                }
                else
                {
                    OrderMaster om = this.genericMgr.FindAll<OrderMaster>("from OrderMaster where Status in (1,2) and OrderNo = ? ", kv.Value.OrderNo).SingleOrDefault();
                    if (om == null)
                    {
                        throw new BusinessException("找不到订单:" + kv.Value.OrderNo);
                    }
                    else
                    {
                        om.AddOrderDetail(kv.Value);
                        omDict.Add(kv.Value.OrderNo, om);
                    }
                }
                #region 循环更新订单明细
                kv.Value.ShippedQty += kv.Value.ShipQtyInput;
                genericMgr.Update(kv.Value);
                #endregion
            } //foreach oddict

            string ipNos = string.Empty;
            foreach (KeyValuePair<string, OrderMaster> kv in omDict)
            {
                if (kv.Value.Status == com.Sconit.CodeMaster.OrderStatus.Submit
                    && kv.Value.Type != com.Sconit.CodeMaster.OrderType.Production
                    && kv.Value.Type != com.Sconit.CodeMaster.OrderType.SubContract)
                {
                    kv.Value.Status = com.Sconit.CodeMaster.OrderStatus.InProcess;
                    kv.Value.StartDate = DateTime.Now;
                    User user = SecurityContextHolder.Get();
                    kv.Value.StartUserId = user.Id;
                    kv.Value.StartUserName = user.FullName;
                    genericMgr.Update(kv.Value);
                }

                IpMaster ipm = ipMgr.TransferOrder2Ip(new OrderMaster[] { kv.Value });
                ipm.Vehicle = vehicleno;
                ipMgr.CreateIp(ipm);

                ipNos += ipm.IpNo + ",";
            }
            //发货成功后更新PickTask的ShippedQty
            foreach (KeyValuePair<string, OrderDetail> okv in odDict)
            {
                taskDict[okv.Key].ShippedQty += okv.Value.OrderDetailInputs.Sum(odi => odi.ShipQty);

                this.genericMgr.Update(taskDict[okv.Key]);
            }

            //更新pickresult的发货标记
            foreach (PickResult pr in results)
            {
                pr.IsShip = true;
                this.genericMgr.Update(pr);
            }
            return ipNos;
        }

        public string ShipPerFlow(IList<string> pickedhus, string vehicleno, string picker)
        {
            throw new NotImplementedException();
        }


        public IList<PickTask> GetPickerTasks(string picker)
        {
            string whereStatement = "select c.* from ord_picktask c inner join view_ordermstr o on c.orderno = o.orderno where "
                + "(o.Status = " + (int)CodeMaster.OrderStatus.Submit
                                                    + " or o.Status = "
                                                    + (int)CodeMaster.OrderStatus.InProcess
                                                    + ") and c.orderedqty > c.pickedqty and c.IsHold = 0 and c.Picker = '" + picker + "'";
            return this.genericMgr.FindEntityWithNativeSql<PickTask>(whereStatement);
        }

        public IList<string> GetUnpickedHu(string pickid)
        {
            return this.genericMgr.FindAllWithNativeSql<string>("select RepackHu from ord_PickHu where PickId = ? ", pickid);
        }

        private string GetPicker(string item, string location)
        {
            string picker = string.Empty;

            //先匹配规则，然后匹配库位，再匹配有效的
            IList<PickRule> pickRule = this.genericMgr.FindAll<PickRule>("from PickRule where Item = ? and Location = ?",
                new object[] { item, location });
            if (pickRule != null && pickRule.Count > 0)
            {
                picker = pickRule[0].Picker;
            }
            else
            {
                IList<Picker> locpickers = this.genericMgr.FindAll<Picker>("from Picker where Location = ? and IsActive = ?", new object[] { location, true });
                if (locpickers != null && locpickers.Count > 0)
                {
                    picker = locpickers[0].Code;
                }
                else
                {
                    IList<Picker> anyPicker = this.genericMgr.FindAll<Picker>("from Picker where IsActive = ?", new object[] { true });
                    if (anyPicker != null && anyPicker.Count > 0)
                    {
                        picker = anyPicker[0].Code;
                    }
                }
            }
            return picker;
        }
    }
}
