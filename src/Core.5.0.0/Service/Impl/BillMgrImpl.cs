using System;
using System.Linq;
using AutoMapper;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.BIL;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.ORD;
using System.Collections.Generic;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class BillMgrImpl : BaseMgr, IBillMgr
    {
        #region 变量
        public IGenericMgr genericMgr { get; set; }
        public ISystemMgr systemMgr { get; set; }
        public IItemMgr itemMgr { get; set; }
        #endregion

        #region Public Methods
        [Transaction(TransactionMode.Requires)]
        public PlanBill CreatePlanBill(ReceiptDetail receiptDetail, ReceiptDetailInput receiptDetailInput)
        {
            return CreatePlanBill(receiptDetail, receiptDetailInput, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public PlanBill CreatePlanBill(ReceiptDetail receiptDetail, ReceiptDetailInput receiptDetailInput, DateTime effectiveDate)
        {
            PlanBill planBill = null;

            #region 寄售查找是否有PlanBill
            if (!(receiptDetail.BillTerm == CodeMaster.OrderBillTerm.ReceivingSettlement
                || receiptDetail.BillTerm == CodeMaster.OrderBillTerm.NA))
            {
                planBill = this.genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill where Item = ? and BillAddr = ? and BillTerm not in(?,?)",
                    new object[] { receiptDetail.Item, receiptDetail.BillAddress, CodeMaster.OrderBillTerm.NA, CodeMaster.OrderBillTerm.ReceivingSettlement }).FirstOrDefault();

                if (planBill != null)
                {
                    planBill.CurrentVoidQty = 0;
                    planBill.CurrentCancelVoidQty = 0;
                    planBill.CurrentActingQty = 0;
                    planBill.CurrentLocation = null;
                    planBill.CurrentHuId = null;
                    planBill.CurrentActingBill = null;
                    planBill.CurrentBillTransaction = null;

                    if (planBill.BillTerm != receiptDetail.BillTerm)
                    {
                        //throw new BusinessException("物料{0}的结算方式（{1}）和寄售结算方式（{2}）不一致。", receiptDetail.Item,
                        //   systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.BillTerm, (int)receiptDetail.BillTerm),
                        //   systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.BillTerm, (int)planBill.BillTerm));
                        throw new BusinessException("物料{0}的结算方式（{1}）和寄售结算方式（{2}）不一致。", receiptDetail.Item,
                           systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.OrderBillTerm, (int)receiptDetail.BillTerm),
                           systemMgr.GetCodeDetailDescription(CodeMaster.CodeMaster.OrderBillTerm, (int)planBill.BillTerm));
                    }
                }

                //if (planBill.Uom != receiptDetail.Uom)
                //{
                //    throw new BusinessException("物料{0}的收货单位{1}和寄售结算的单位{2}不一致。", receiptDetail.Item, receiptDetail.Uom, planBill.Uom);
                //}
            }
            else
            {
                planBill = this.genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill where Item = ? and BillAddr = ? and RecNo = ?",
                  new object[] { receiptDetail.Item, receiptDetail.BillAddress, receiptDetail.ReceiptNo }).FirstOrDefault();

                if (planBill != null)
                {
                    planBill.CurrentVoidQty = 0;
                    planBill.CurrentCancelVoidQty = 0;
                    planBill.CurrentActingQty = 0;
                    planBill.CurrentLocation = null;
                    planBill.CurrentHuId = null;
                    planBill.CurrentActingBill = null;
                    planBill.CurrentBillTransaction = null;
                }
            }
            #endregion

            #region 没有PlanBill，创建PlanBill
            if (planBill == null)
            {
                planBill = new PlanBill();
                if (receiptDetail.BillTerm == CodeMaster.OrderBillTerm.ReceivingSettlement
                    || receiptDetail.BillTerm == CodeMaster.OrderBillTerm.NA)
                {
                    planBill.OrderNo = receiptDetail.OrderNo;
                    planBill.IpNo = receiptDetail.IpNo;
                    //planBill.ExternalIpNo = receiptDetail.ExternalIpNo;
                    planBill.ReceiptNo = receiptDetail.ReceiptNo;
                    planBill.ExternalReceiptNo = receiptDetail.Id.ToString();
                }
                if (receiptDetail.OrderType == com.Sconit.CodeMaster.OrderType.Procurement
                    || receiptDetail.OrderType == com.Sconit.CodeMaster.OrderType.SubContract
                    || receiptDetail.OrderType == com.Sconit.CodeMaster.OrderType.ScheduleLine)
                {
                    planBill.Type = com.Sconit.CodeMaster.BillType.Procurement;
                    if (receiptDetailInput.ReceiveQty > 0)
                    {
                        planBill.Party = receiptDetail.CurrentPartyFrom;
                        planBill.PartyName = receiptDetail.CurrentPartyFromName;
                    }
                    else
                    {
                        planBill.Party = receiptDetail.CurrentPartyTo;
                        planBill.PartyName = receiptDetail.CurrentPartyToName;
                    }
                }
                else if (receiptDetail.OrderType == com.Sconit.CodeMaster.OrderType.Distribution)
                {
                    planBill.Type = com.Sconit.CodeMaster.BillType.Distribution;
                    if (receiptDetailInput.ReceiveQty > 0)
                    {
                        planBill.Party = receiptDetail.CurrentPartyTo;
                        planBill.PartyName = receiptDetail.CurrentPartyToName;
                    }
                    else
                    {
                        planBill.Party = receiptDetail.CurrentPartyFrom;
                        planBill.PartyName = receiptDetail.CurrentPartyFromName;
                    }
                }
                planBill.Item = receiptDetail.Item;
                planBill.ItemDescription = receiptDetail.ItemDescription;

                planBill.Uom = receiptDetail.Uom;
                planBill.UnitCount = receiptDetail.UnitCount;
                planBill.BillTerm = receiptDetail.BillTerm == CodeMaster.OrderBillTerm.NA ? CodeMaster.OrderBillTerm.ReceivingSettlement : receiptDetail.BillTerm;
                planBill.BillAddress = receiptDetail.BillAddress;
                //planBill.BillAddressDescription = receiptDetail.BillAddressDescription;
                planBill.PriceList = receiptDetail.PriceList;
                planBill.Currency = receiptDetail.Currency;
                planBill.UnitPrice = receiptDetail.UnitPrice.HasValue ? receiptDetail.UnitPrice.Value : 0;
                planBill.IsProvisionalEstimate = receiptDetail.UnitPrice.HasValue ? receiptDetail.IsProvisionalEstimate : false;
                planBill.Tax = receiptDetail.Tax;
                planBill.IsIncludeTax = receiptDetail.IsIncludeTax;
                planBill.PlanAmount = 0;
                if (receiptDetail.OrderSubType == com.Sconit.CodeMaster.OrderSubType.Normal)
                {
                    //planBill.PlanQty = receiptDetailInput.ReceiveQty / receiptDetail.UnitQty;
                    planBill.PlanQty = receiptDetailInput.ReceiveQty;
                }
                else
                {
                    //planBill.PlanQty = -receiptDetailInput.ReceiveQty / receiptDetail.UnitQty;
                    planBill.PlanQty = -receiptDetailInput.ReceiveQty;
                }
                planBill.UnitQty = receiptDetail.UnitQty;
                //planBill.HuId = receiptDetailInput.HuId;
                //planBill.LocationFrom = receiptDetail.LocationFrom;
                planBill.EffectiveDate = effectiveDate;

                this.genericMgr.Create(planBill);
            }
            #endregion

            #region 有PlanBill，增加待结算数量
            else
            {

                if (receiptDetail.OrderSubType == com.Sconit.CodeMaster.OrderSubType.Normal)
                {
                    if (planBill.Uom != receiptDetail.Uom)
                    {
                        planBill.PlanQty += this.itemMgr.ConvertItemUomQty(receiptDetail.Item, receiptDetail.Uom, receiptDetailInput.ReceiveQty, planBill.Uom);
                    }
                    else
                    {
                        planBill.PlanQty += receiptDetailInput.ReceiveQty;
                    }
                }
                else
                {
                    if (planBill.Uom != receiptDetail.Uom)
                    {
                        planBill.PlanQty -= this.itemMgr.ConvertItemUomQty(receiptDetail.Item, receiptDetail.Uom, receiptDetailInput.ReceiveQty, planBill.Uom);
                    }
                    else
                    {
                        planBill.PlanQty -= receiptDetailInput.ReceiveQty;
                    }
                }

                this.genericMgr.Update(planBill);
            }
            #endregion

            #region 收货结算
            if (receiptDetail.BillTerm == CodeMaster.OrderBillTerm.NA
                || receiptDetail.BillTerm == CodeMaster.OrderBillTerm.ReceivingSettlement)
            {
                if (receiptDetail.OrderSubType == com.Sconit.CodeMaster.OrderSubType.Normal)
                {
                    if (planBill.Uom != receiptDetail.Uom)
                    {
                        planBill.CurrentActingQty = this.itemMgr.ConvertItemUomQty(receiptDetail.Item, receiptDetail.Uom, receiptDetailInput.ReceiveQty, planBill.Uom);
                    }
                    else
                    {
                        planBill.CurrentActingQty = receiptDetailInput.ReceiveQty;
                    }
                }
                else
                {
                    if (planBill.Uom != receiptDetail.Uom)
                    {
                        planBill.CurrentActingQty = -this.itemMgr.ConvertItemUomQty(receiptDetail.Item, receiptDetail.Uom, receiptDetailInput.ReceiveQty, planBill.Uom);
                    }
                    else
                    {
                        planBill.CurrentActingQty = -receiptDetailInput.ReceiveQty;
                    }
                }
                BillTransaction billTransaction = this.SettleBill(planBill, effectiveDate);
                planBill.CurrentActingBill = billTransaction.ActingBill;
                planBill.CurrentBillTransaction = billTransaction.Id;
            }
            #endregion

            return planBill;
        }

        [Transaction(TransactionMode.Requires)]
        public PlanBill LoadPlanBill(string item, string location, string consignmentSupplier, DateTime effectiveDate, bool isInitLoc)
        {
            string uom = null;
            string billAddr = null;
            CodeMaster.OrderBillTerm billTerm = CodeMaster.OrderBillTerm.OnlineBilling;
            decimal uc = 1;
            string baseUom = null;

            if (!isInitLoc)
            {
                IList<object[]> parms = this.genericMgr.FindAllWithNativeSql<object[]>(@"select det.Uom, ISNULL(det.BillAddr, mstr.BillAddr) as BillAddr, ISNULL(det.LocTo, mstr.LocTo) as LocTo, det.BillTerm as dBillTerm, mstr.BillTerm as mBillTerm, det.UC, i.Uom
                                                            from SCM_FlowDet as det inner join SCM_FlowMstr as mstr on det.Flow = mstr.Code
                                                            inner join MD_Item as i on det.Item = i.Code
                                                            where det.Item = ? and mstr.PartyFrom = ? and mstr.Type = ? and det.IsActive = ? and mstr.IsActive = ?",
                                                                new object[] { item, consignmentSupplier, CodeMaster.OrderType.Procurement, true, true });

                if (parms == null || parms.Count == 0)
                {
                    //没有采购路线一定不能进行收货结算，否则有可能因为没有合同导致sap无法结算
                    throw new BusinessException("没有找到供应商{0}物料{1}的采购路线，不能出现寄售负数库存。", consignmentSupplier, item);
                    //Item item1 = this.genericMgr.FindById<Item>(item);
                    //uom = item1.Uom;
                    //uc = item1.UnitCount;
                    //baseUom = item1.Uom;

                    //PartyAddress partyAddress = this.genericMgr.FindAll<PartyAddress>("select pa from PartyAddress as pa inner join pa.Address as a  where pa.Party = ? and a.Type = ?", new object[] { consignmentSupplier, CodeMaster.AddressType.BillAddress }).First();

                    //billAddr = partyAddress.Address.Code;
                    //billTerm = CodeMaster.OrderBillTerm.NA;
                }
                else
                {
                    object[] parm = parms.Where(p => (string)p[2] == location).FirstOrDefault();

                    if (parm == null)
                    {
                        parm = parms.First();
                    }

                    uom = (string)parm[0];
                    billAddr = (string)parm[1];
                    CodeMaster.OrderBillTerm detBillTerm = (CodeMaster.OrderBillTerm)(int.Parse(parm[3].ToString()));
                    CodeMaster.OrderBillTerm mstrBillTerm = (CodeMaster.OrderBillTerm)(int.Parse(parm[4].ToString()));
                    //CodeMaster.OrderBillTerm billTerm = detBillTerm != CodeMaster.OrderBillTerm.NA ? detBillTerm : mstrBillTerm;
                    //指定供应商的一定是寄售结算
                    billTerm = CodeMaster.OrderBillTerm.OnlineBilling;
                    uc = (decimal)parm[5];
                    baseUom = (string)parm[6];
                }
            }
            else
            {
                Item itemInstance = this.genericMgr.FindById<Item>(item);
                string ba = this.genericMgr.FindAllWithNativeSql<string>("select a.Code from MD_Address as a inner join MD_PartyAddr as pa on a.Code = pa.Address where pa.Party = ? and a.Type = ?",
                    new object[] { consignmentSupplier, com.Sconit.CodeMaster.AddressType.BillAddress }).FirstOrDefault();
                if (ba == null)
                {
                    throw new BusinessException("没有找到供应商{0}开票地址，不能出现寄售负数库存。", consignmentSupplier);
                }
                uom = itemInstance.Uom;
                billAddr = ba;
                //指定供应商的一定是寄售结算
                billTerm = CodeMaster.OrderBillTerm.OnlineBilling;
                uc = itemInstance.UnitCount;
                baseUom = itemInstance.Uom;
            }
            PlanBill planBill = this.genericMgr.FindEntityWithNativeSql<PlanBill>("select * from BIL_PlanBill where Item = ? and BillAddr = ? and BillTerm = ?", new object[] { item, billAddr, billTerm }).FirstOrDefault();

            #region 没有PlanBill，创建PlanBill
            if (planBill == null)
            {
                planBill = new PlanBill();
                planBill.Type = com.Sconit.CodeMaster.BillType.Procurement;
                planBill.Party = consignmentSupplier;
                planBill.Item = item;
                planBill.ItemDescription = this.genericMgr.FindById<Item>(item).Description;
                planBill.Uom = uom;
                planBill.UnitCount = uc;
                planBill.BillTerm = billTerm;
                planBill.BillAddress = billAddr;
                planBill.PlanAmount = 0;
                if (baseUom != uom)
                {
                    planBill.UnitQty = itemMgr.ConvertItemUomQty(item, baseUom, 1, uom);
                }
                else
                {
                    planBill.UnitQty = 1;
                }
                planBill.PlanQty = 0;
                planBill.EffectiveDate = effectiveDate;

                this.genericMgr.Create(planBill);
            }
            #endregion

            else
            {
                planBill.CurrentVoidQty = 0;
                planBill.CurrentCancelVoidQty = 0;
                planBill.CurrentActingQty = 0;
                planBill.CurrentLocation = null;
                planBill.CurrentHuId = null;
                planBill.CurrentActingBill = null;
                planBill.CurrentBillTransaction = null;
            }
            return planBill;
        }

        [Transaction(TransactionMode.Requires)]
        public BillTransaction SettleBill(PlanBill planBill)
        {
            return SettleBill(planBill, DateTime.Now);
        }

        [Transaction(TransactionMode.Requires)]
        public BillTransaction SettleBill(PlanBill planBill, DateTime effectiveDate)
        {
            //if (string.IsNullOrWhiteSpace(receiptNo))
            //{
            //    throw new TechnicalException("receiptNo can't be null or white space");
            //}

            //检验，已结算数+本次结算数不能大于总结算数量，可能有负数结算，所以要用绝对值比较
            //if (Math.Abs(planBill.ActingQty + planBill.CurrentActingQty) > Math.Abs(planBill.PlanQty))
            //{
            //    throw new BusinessException(Resources.BIL.Bill.Errors_ActingQtyExceedPlanQty, planBill.Item);
            //}

            ActingBill actingBill = this.CreateActingBill(planBill, effectiveDate);

            #region 计算结算金额，不在支持订单折扣，直接按照单价计算金额
            //if (Math.Abs(oldPlannedBill.ActingQty.Value + oldPlannedBill.CurrentActingQty) < Math.Abs(oldPlannedBill.PlannedQty))
            //{
            //    //总结算数小于计划数，用实际单价计算待开票金额
            //    planBill.CurrentBillAmount = oldPlannedBill.UnitPrice * oldPlannedBill.CurrentActingQty;
            //}
            //else
            //{
            //    planBill.CurrentBillAmount = oldPlannedBill.PlannedAmount - (oldPlannedBill.ActingAmount.HasValue ? oldPlannedBill.ActingAmount.Value : 0);
            //}
            //actingBill.BillAmount += planBill.CurrentBillAmount;
            #endregion

            #region 更新Planed Bill的已结算数量
            planBill.ActingQty += planBill.CurrentActingQty;
            //PlanBill不在有关闭的情况
            //planBill.IsClose = (planBill.ActingQty != planBill.PlanQty);
            this.genericMgr.Update(planBill);
            #endregion

            if (actingBill.Id == 0)
            {
                genericMgr.Create(actingBill);
            }
            else
            {
                genericMgr.Update(actingBill);
            }

            #region 记BillTransaction
            return RecordBillTransaction(planBill, actingBill, effectiveDate, false);
            #endregion
            //return actingBill;
        }

        [Transaction(TransactionMode.Requires)]
        public BillTransaction VoidSettleBill(ActingBill actingBill, PlanBill planBill, bool voidPlanBill)
        {
            #region 更新ActingBill
            //if (Math.Abs(actingBill.BillQty) < Math.Abs(actingBill.BilledQty + actingBill.VoidQty + actingBill.CurrentVoidQty))
            //{
            //    //待开票数量不足，不能冲销结算
            //    throw new BusinessException(Resources.BIL.Bill.Errors_VoidActBillFailActQtyNotEnough, actingBill.Item);
            //}

            actingBill.VoidQty += actingBill.CurrentVoidQty;
            //actingBill.IsClose = (actingBill.BillQty == (actingBill.BilledQty + actingBill.VoidQty));
            this.genericMgr.Update(actingBill);
            #endregion

            #region 反向更新PlanBill
            if (planBill == null)
            {
                planBill = this.genericMgr.FindById<PlanBill>(actingBill.PlanBill);

                planBill.CurrentVoidQty = 0;
                planBill.CurrentCancelVoidQty = 0;
                planBill.CurrentActingQty = 0;
                planBill.CurrentLocation = null;
                planBill.CurrentHuId = null;
                planBill.CurrentActingBill = null;
                planBill.CurrentBillTransaction = null;
            }

            planBill.ActingQty -= actingBill.CurrentVoidQty;

            //if (Math.Abs(planBill.ActingQty + planBill.VoidQty) > Math.Abs(planBill.PlanQty))
            //{
            //    //冲销的数量大于待开票数量
            //    throw new BusinessException(Resources.BIL.Bill.Errors_VoidActingQtyExceedPlanQty, planBill.Item);
            //}

            if (voidPlanBill)
            {
                //if (Math.Abs(planBill.ActingQty + planBill.VoidQty + actingBill.CurrentVoidQty) > Math.Abs(planBill.PlanQty))
                //{
                //    //已经结算，不能冲销寄售信息
                //    throw new BusinessException(Resources.BIL.Bill.Errors_VoidPlanBillFailPlanQtyNotEnough, planBill.Item);
                //}

                planBill.VoidQty += actingBill.CurrentVoidQty;
            }

            //if (planBill.PlanQty == (planBill.ActingQty + planBill.VoidQty))
            //{
            //    planBill.IsClose = true;
            //}
            //else
            //{
            //    planBill.IsClose = false;
            //}

            this.genericMgr.Update(planBill);
            #endregion

            #region 记录账单事务
            planBill.CurrentVoidQty = actingBill.CurrentVoidQty;
            return this.RecordBillTransaction(planBill, actingBill, actingBill.EffectiveDate, true);
            #endregion
        }

        [Transaction(TransactionMode.Requires)]
        public void VoidPlanBill(PlanBill planBill)
        {
            //if (Math.Abs(planBill.ActingQty + planBill.VoidQty + planBill.CurrentVoidQty) > Math.Abs(planBill.PlanQty))
            //{
            //    //已经结算，不能冲销寄售信息
            //    throw new BusinessException(Resources.BIL.Bill.Errors_VoidPlanBillFailPlanQtyNotEnough, planBill.Item);
            //}

            planBill.VoidQty += planBill.CurrentVoidQty;

            //if (planBill.PlanQty == (planBill.ActingQty + planBill.VoidQty))
            //{
            //    planBill.IsClose = true;
            //}
            //else
            //{
            //    planBill.IsClose = false;
            //}
            this.genericMgr.Update(planBill);
        }

        [Transaction(TransactionMode.Requires)]
        public void CancelVoidPlanBill(PlanBill planBill)
        {
            planBill.VoidQty -= planBill.CurrentCancelVoidQty;
            this.genericMgr.Update(planBill);
        }
        #endregion

        #region Private Methods
        private ActingBill CreateActingBill(PlanBill planBill, DateTime effectiveDate)
        {
            DateTime formatedEffDate = DateTime.Parse(effectiveDate.ToShortDateString());   //仅保留年月日

            ActingBill actingBill = (this.genericMgr.FindEntityWithNativeSql<ActingBill>("select * from BIL_ActBill where PlanBill = ?", planBill.Id)).SingleOrDefault();
            if (actingBill == null)
            {
                actingBill = Mapper.Map<PlanBill, ActingBill>(planBill);
                actingBill.EffectiveDate = formatedEffDate;
                //actingBill.IsClose = (actingBill.BillQty != (actingBill.BilledQty + actingBill.VoidQty));
            }
            else
            {
                actingBill.CurrentVoidQty = 0;
            }
            actingBill.BillQty += planBill.CurrentActingQty;
            return actingBill;
        }

        private BillTransaction RecordBillTransaction(PlanBill planBill, ActingBill actingBill, DateTime effectiveDate, bool isVoid)
        {
            #region 记BillTransaction
            BillTransaction billTransaction = new BillTransaction();

            billTransaction.OrderNo = planBill.OrderNo;
            billTransaction.IpNo = planBill.IpNo;
            billTransaction.ExternalIpNo = planBill.ExternalIpNo;
            billTransaction.ReceiptNo = planBill.ReceiptNo;
            billTransaction.ExternalReceiptNo = planBill.ExternalReceiptNo;
            billTransaction.IsIncludeTax = planBill.IsIncludeTax;
            billTransaction.Item = planBill.Item;
            billTransaction.ItemDescription = planBill.ItemDescription;
            billTransaction.Uom = planBill.Uom;
            billTransaction.UnitCount = planBill.UnitCount;
            billTransaction.HuId = planBill.CurrentHuId;
            billTransaction.TransactionType =
                planBill.Type == com.Sconit.CodeMaster.BillType.Procurement ?
                (isVoid ? com.Sconit.CodeMaster.BillTransactionType.POSettleVoid : com.Sconit.CodeMaster.BillTransactionType.POSettle) :
                (isVoid ? com.Sconit.CodeMaster.BillTransactionType.SOSettleVoid : com.Sconit.CodeMaster.BillTransactionType.SOSettle);
            billTransaction.BillAddress = planBill.BillAddress;
            billTransaction.BillAddressDescription = planBill.BillAddressDescription;
            billTransaction.Party = planBill.Party;
            billTransaction.PartyName = planBill.PartyName;
            billTransaction.PriceList = planBill.PriceList;
            billTransaction.Currency = planBill.Currency;
            billTransaction.UnitPrice = planBill.UnitPrice;
            billTransaction.IsProvisionalEstimate = planBill.IsProvisionalEstimate;
            billTransaction.Tax = planBill.Tax;

            #region 记录数量
            decimal qty = isVoid ? planBill.CurrentVoidQty : planBill.CurrentActingQty;
            billTransaction.BillQty = (isVoid ? -1 : 1)      //冲销为负数
                                        * (planBill.Type == com.Sconit.CodeMaster.BillType.Procurement ? -1 * qty : qty);  //采购付款为负数
            //billTransaction.BillAmount = 0;
            #endregion
            billTransaction.UnitQty = planBill.UnitQty;
            billTransaction.LocationFrom = planBill.LocationFrom;
            billTransaction.SettleLocation = planBill.CurrentLocation;
            billTransaction.EffectiveDate = effectiveDate;
            billTransaction.PlanBill = planBill.Id;
            billTransaction.ActingBill = actingBill.Id;

            User user = SecurityContextHolder.Get();
            billTransaction.CreateUserId = user.Id;
            billTransaction.CreateUserName = user.FullName;
            billTransaction.CreateDate = DateTime.Now;

            this.genericMgr.Create(billTransaction);

            return billTransaction;
            #endregion
        }
        #endregion
    }
}
