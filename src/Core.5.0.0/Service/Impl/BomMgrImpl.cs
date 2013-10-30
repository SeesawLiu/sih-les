using System;
using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.MD;
using com.Sconit.Entity.PRD;
using com.Sconit.Entity.SCM;

namespace com.Sconit.Service.Impl
{
    public class BomMgrImpl : BaseMgr, IBomMgr
    {
        #region 变量
        public IItemMgr itemMgr { get; set; }
        public IFlowMgr flowMgr { get; set; }
        public IGenericMgr genericMgr { get; set; }
        #endregion

        #region public methods
        public IList<BomDetail> GetNextLevelBomDetail(string bomCode, DateTime? effectiveDate)
        {
            if (effectiveDate == null)
            {
                effectiveDate = DateTime.Now;
            }

            string hql = @"select bd from BomDetail as bd,Item as i where bd.Item = i.Code
                        and bd.Bom = ? and i.IsActive = ? and bd.StartDate <= ? 
                                and (bd.EndDate is null or bd.EndDate >= ?)";

            //DetachedCriteria detachedCriteria = DetachedCriteria.For<BomDetail>();
            //detachedCriteria.CreateAlias("Item", "item");

            //detachedCriteria.Add(Expression.Eq("Bom", bomCode));
            //detachedCriteria.Add(Expression.Eq("item.IsActive", true));
            //detachedCriteria.Add(Expression.Le("StartDate", effectiveDate));
            //detachedCriteria.Add(Expression.Or(Expression.Ge("EndDate", effectiveDate), Expression.IsNull("EndDate")));

            IList<BomDetail> bomDetailList = genericMgr.FindAll<BomDetail>(hql, new object[] { bomCode, true, effectiveDate, effectiveDate });

            if (bomDetailList != null && bomDetailList.Count() > 0)
            {
                return this.GetNoOverloadBomDetail(bomDetailList);
            }
            else
            {
                throw new BusinessException("Errors.Bom.BomDetailNotFound", bomCode);
            }
        }

        public IList<BomDetail> GetFlatBomDetail(string bomCode, DateTime? effectiveDate)
        {
            if (effectiveDate == null)
            {
                effectiveDate = DateTime.Now;
            }

            IList<BomDetail> flatBomDetailList = new List<BomDetail>();
            IList<BomDetail> nextBomDetailList = this.GetNextLevelBomDetail(bomCode, effectiveDate);

            foreach (BomDetail nextBomDetail in nextBomDetailList)
            {
                nextBomDetail.CalculatedQty = nextBomDetail.RateQty * (1 + nextBomDetail.ScrapPercentage);
                ProcessCurrentBomDetailStructrue(flatBomDetailList, nextBomDetail, effectiveDate.Value);
            }
            return flatBomDetailList;
        }

        public string FindItemBom(Item item)
        {
            //默认用Item上的BomCode，如果Item上面没有设置Bom，直接用ItemCode作为BomCode去找
            return (!string.IsNullOrWhiteSpace(item.Bom) ? item.Bom : item.Code);
        }

        public string FindItemBom(string itemCode)
        {
            Item item = this.genericMgr.FindById<Item>(itemCode);
            return FindItemBom(item);
        }

        public IList<BomDetail> GetProductLineWeightAverageBomDetail(string flow)
        {
            IList<FlowDetail> flowDetailList = this.flowMgr.GetFlowDetailList(flow);

            if (flowDetailList != null && flowDetailList.Count > 0)
            {
                DateTime dateTimeNow = DateTime.Now;
                FlowMaster flowMaster = this.genericMgr.FindById<FlowMaster>(flow);

                IList<BomDetail> batchFeedBomDetailList = new List<BomDetail>();
                foreach (FlowDetail flowDetail in flowDetailList)
                {
                    //先获取flowdetail上的bom，如果为null再以flowdetail的Item对象去找
                    string bomCode = flowDetail.Bom != null ? flowDetail.Bom : FindItemBom(flowDetail.Item);
                    IList<BomDetail> bomDetailList = GetFlatBomDetail(bomCode, dateTimeNow);

                    if (bomDetailList != null && bomDetailList.Count > 0)
                    {
                        foreach (BomDetail bomDetail in bomDetailList)
                        {
                            if (bomDetail.BackFlushMethod == com.Sconit.CodeMaster.BackFlushMethod.WeightAverage)
                            {
                                bomDetail.FeedLocation = !string.IsNullOrWhiteSpace(flowDetail.LocationFrom) ? flowDetail.LocationFrom : flowMaster.LocationFrom;
                                batchFeedBomDetailList.Add(bomDetail);
                            }
                        }
                    }
                }

                return batchFeedBomDetailList;
            }
            return null;
        }
        #endregion

        #region private methods
        private IList<BomDetail> GetNoOverloadBomDetail(IList<BomDetail> bomDetailList)
        {
            //过滤BomCode，ItemCode，Operation，Reference相同的BomDetail，只取StartDate最大的。
            var groupedDetList = from det in bomDetailList
                                 group det by new { Bom = det.Bom, Item = det.Item, Ref = det.OpReference, Op = det.Operation } into Result
                                 select new
                                 {
                                     Bom = Result.Key.Bom,
                                     Item = Result.Key.Item,
                                     Ref = Result.Key.Ref,
                                     Op = Result.Key.Op,
                                     StartDate = Result.Max(det => det.StartDate),
                                     MaxId = Result.Max(det => det.Id)
                                 };

            IList<BomDetail> noOverloadDetailList = (from det in bomDetailList
                                                     join groupedDet in groupedDetList
                                                     on new { Bom = det.Bom, Item = det.Item, Ref = det.OpReference, Op = det.Operation, StartDate = det.StartDate }
                                                     equals new { Bom = groupedDet.Bom, Item = groupedDet.Item, Ref = groupedDet.Ref, Op = groupedDet.Op, StartDate = groupedDet.StartDate }
                                                     select det).ToList();

            #region 检查Bom + Item + Op + Ref + StartDate是否重复
            #endregion

            return noOverloadDetailList;

            #region 旧算法
            /*
            IList<BomDetail> noOverloadBomDetailList = new List<BomDetail>();
            foreach (BomDetail bomDetail in bomDetailList)
            {
                int overloadIndex = -1;
                for (int i = 0; i < noOverloadBomDetailList.Count; i++)
                {
                    //判断BomCode，ItemCode，Operation，Reference是否相同
                    if (noOverloadBomDetailList[i].Bom == bomDetail.Bom
                        && noOverloadBomDetailList[i].Item == bomDetail.Item
                        && noOverloadBomDetailList[i].Operation == bomDetail.Operation
                        && noOverloadBomDetailList[i].Reference == bomDetail.Reference)
                    {
                        //存在相同的，记录位置。
                        overloadIndex = i;
                        break;
                    }
                }

                if (overloadIndex == -1)
                {
                    //没有相同的记录，直接把BomDetail加入返回列表
                    noOverloadBomDetailList.Add(bomDetail);
                }
                else
                {
                    //有相同的记录，判断bomDetail.StartDate和结果集中的大。
                    if (noOverloadBomDetailList[overloadIndex].StartDate < bomDetail.StartDate)
                    {
                        //bomDetail.StartDate大于结果集中的，替换结果集
                        noOverloadBomDetailList[overloadIndex] = bomDetail;
                    }
                }
            }
            return noOverloadBomDetailList;
                */
            #endregion
        }

        private void ProcessCurrentBomDetailStructrue(IList<BomDetail> flatBomDetailList, BomDetail currentBomDetail, DateTime efftiveDate)
        {
            if (currentBomDetail.StructureType == com.Sconit.CodeMaster.BomStructureType.Normal) //普通结构
            {
                ProcessCurrentBomDetailItem(flatBomDetailList, currentBomDetail, efftiveDate);
            }
            else if (currentBomDetail.StructureType == com.Sconit.CodeMaster.BomStructureType.Virtual) //虚结构
            {
                //如果是虚结构(X)，不把自己加到返回表里，继续向下分解
                NestingGetNextLevelBomDetail(flatBomDetailList, currentBomDetail.Item, currentBomDetail.Uom, currentBomDetail.CalculatedQty, efftiveDate);
            }
        }

        private void ProcessCurrentBomDetailItem(IList<BomDetail> flatBomDetailList, BomDetail currentBomDetail, DateTime efftiveDate)
        {
            TryLoadBomItem(currentBomDetail);
            if (currentBomDetail.CurrentItem.IsVirtual)
            {
                //如果是虚零件(X)，继续向下分解
                NestingGetNextLevelBomDetail(flatBomDetailList, currentBomDetail.Item, currentBomDetail.Uom, currentBomDetail.CalculatedQty, efftiveDate);
            }
            else if (currentBomDetail.CurrentItem.IsKit)
            {
                //组件，先拆分组件再继续向下分解
                //考虑组件的比例
                IList<ItemKit> itemKitList = itemMgr.GetKitItemChildren(currentBomDetail.Item);

                if (itemKitList != null && itemKitList.Count() > 0)
                {
                    foreach (ItemKit itemKit in itemKitList)
                    {
                        NestingGetNextLevelBomDetail(flatBomDetailList, itemKit.ChildItem.Code, currentBomDetail.Uom, (currentBomDetail.CalculatedQty * itemKit.Qty), efftiveDate);
                    }
                }
                else
                {
                    throw new BusinessException("Errors.ItemKit.ChildrenItemNotFound", currentBomDetail.Item);
                }
            }
            else
            {
                //thinking:是否需要考虑某种零件不能作为BomDetail.Item

                //直接加入到flatBomDetailList
                flatBomDetailList.Add(currentBomDetail);
            }
        }

        private void NestingGetNextLevelBomDetail(IList<BomDetail> flatBomDetailList, string currentBomItem, string currentBomItemUom, decimal calculatedQty, DateTime efftiveDate)
        {
            string nextLevelBomCode = this.FindItemBom(currentBomItem);
            IList<BomDetail> nextBomDetailList = this.GetNextLevelBomDetail(nextLevelBomCode, efftiveDate);

            foreach (BomDetail nextBomDetail in nextBomDetailList)
            {
                //当前子件的Uom和下层Bom的Uom不匹配，需要做单位转换
                BomMaster nextLevelBom = genericMgr.FindById<BomMaster>(nextBomDetail.Bom);
                if (currentBomItemUom != nextLevelBom.Uom)
                {
                    //单位换算
                    nextBomDetail.CalculatedQty = itemMgr.ConvertItemUomQty(currentBomItem, currentBomItemUom, 1, nextLevelBom.Uom)
                        * calculatedQty * nextBomDetail.RateQty * (1 + nextBomDetail.ScrapPercentage);
                }
                else
                {
                    nextBomDetail.CalculatedQty = nextBomDetail.RateQty * calculatedQty * (1 + nextBomDetail.ScrapPercentage);
                }

                ProcessCurrentBomDetailStructrue(flatBomDetailList, nextBomDetail, efftiveDate);
            }
        }

        private void TryLoadBomItem(BomDetail bomDetail)
        {
            if (bomDetail.CurrentItem == null)
            {
                bomDetail.CurrentItem = this.genericMgr.FindById<Item>(bomDetail.Item);
            }
        }
        #endregion
    }
}
