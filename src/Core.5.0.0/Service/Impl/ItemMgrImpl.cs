using System;
using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity.BIL;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.MD;
using NHibernate.Criterion;
using Castle.Services.Transaction;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Collections;
using com.Sconit.Utility;
using com.Sconit.Entity.CUST;
using NHibernate.Type;

namespace com.Sconit.Service.Impl
{
    [Transactional]
    public class ItemMgrImpl : BaseMgr, IItemMgr
    {
        #region 变量
        public IGenericMgr genericMgr { get; set; }
        #endregion

        #region public methods
        public IList<ItemKit> GetKitItemChildren(string kitCode)
        {
            return GetKitItemChildren(kitCode, false);
        }

        public IList<ItemKit> GetKitItemChildren(string kitItem, bool includeInActive)
        {
            DetachedCriteria criteria = DetachedCriteria.For<ItemKit>();

            criteria.Add(Expression.Eq("KitItem", kitItem));
            criteria.Add(Expression.Eq("IsActive", includeInActive));

            return genericMgr.FindAll<ItemKit>(criteria);
        }

        public decimal ConvertItemUomQty(string itemCode, string sourceUomCode, decimal sourceQty, string targetUomCode)
        {
            if (sourceUomCode == targetUomCode || sourceQty == 0)
            {
                return sourceQty;
            }

            #region 第一次转换单位，使用Item定义的单位转换
            DetachedCriteria criteria = DetachedCriteria.For<UomConversion>();
            criteria.Add(Expression.Eq("Item.Code", itemCode));

            IList<UomConversion> SpecifiedItemUomConversionList = genericMgr.FindAll<UomConversion>(criteria);

            UomConversion uomConversion = (from conv in SpecifiedItemUomConversionList
                                           where (conv.BaseUom == sourceUomCode && conv.AlterUom == targetUomCode)
                                           || (conv.BaseUom == targetUomCode && conv.AlterUom == sourceUomCode)
                                           select conv).FirstOrDefault();

            if (uomConversion != null)
            {
                if (uomConversion.BaseUom == sourceUomCode)
                {
                    return (sourceQty * uomConversion.AlterQty / uomConversion.BaseQty);
                }
                else
                {
                    return (sourceQty * uomConversion.BaseQty / uomConversion.AlterQty);
                }
            }
            #endregion

            #region 第二次转换单位，使用通用的单位转换
            criteria = DetachedCriteria.For<UomConversion>();
            criteria.Add(Expression.IsNull("Item"));

            IList<UomConversion> genericItemUomConversionList = genericMgr.FindAll<UomConversion>(criteria);

            uomConversion = (from conv in genericItemUomConversionList
                             where (conv.BaseUom == sourceUomCode && conv.AlterUom == targetUomCode)
                             || (conv.BaseUom == targetUomCode && conv.AlterUom == sourceUomCode)
                             select conv).FirstOrDefault();

            if (uomConversion != null)
            {
                if (uomConversion.BaseUom == sourceUomCode)
                {
                    return (sourceQty * uomConversion.AlterQty / uomConversion.BaseQty);
                }
                else
                {
                    return (sourceQty * uomConversion.BaseQty / uomConversion.AlterQty);
                }
            }
            #endregion

            #region 无级单位转换
            List<UomConversion> mergedItemUomConversionList = new List<UomConversion>();
            mergedItemUomConversionList.AddRange(SpecifiedItemUomConversionList);
            mergedItemUomConversionList.AddRange(genericItemUomConversionList);

            //思路：用sourceUomCode、targetUomCode分别和uomConversion.BaseUom、uomConversion.AlterUom匹配
            //总共4种情况，循环嵌套往下查找。
            //每次嵌套需要过滤掉已经用过得uomConversion来防止死循环。
            decimal? targetQty = NestConvertItemUomQty(mergedItemUomConversionList, sourceUomCode, targetUomCode, sourceQty, false);

            if (!targetQty.HasValue)
            {
                targetQty = NestConvertItemUomQty(mergedItemUomConversionList, targetUomCode, sourceUomCode, sourceQty, true);
            }

            if (targetQty.HasValue)
            {
                return targetQty.Value;
            }
            else
            {
                throw new BusinessException(Resources.ErrorMessage.Errors_Item_UomConvertionNotFound, itemCode, sourceUomCode, targetUomCode);
            }
            #endregion
        }

        public PriceListDetail GetItemPrice(string itemCode, string uom, string priceList, string currency, DateTime? effectiveDate)
        {
            if (!effectiveDate.HasValue)
            {
                effectiveDate = DateTime.Now;
            }

            DetachedCriteria criteria = DetachedCriteria.For<PriceListDetail>();

            criteria.CreateAlias("PriceList", "pl");

            criteria.Add(Expression.Eq("pl.Code", priceList));
            criteria.Add(Expression.Eq("Item", itemCode));
            criteria.Add(Expression.Eq("Uom", uom));
            criteria.Add(Expression.Eq("pl.Currency", currency));
            criteria.Add(Expression.Le("StartDate", effectiveDate));
            criteria.Add(Expression.Or(Expression.Ge("EndDate", effectiveDate), Expression.IsNull("EndDate")));

            criteria.AddOrder(Order.Desc("StartDate"));

            IList<PriceListDetail> resultList = genericMgr.FindAll<PriceListDetail>(criteria, 0, 1);
            if (resultList != null && resultList.Count() > 0)
            {
                return resultList[0];
            }
            else
            {
                return null;
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void CreateItem(Item item)
        {
            ItemPackage itemPackage = new ItemPackage();
            itemPackage.Item = item.Code;
            itemPackage.IsDefault = true;
            itemPackage.UnitCount = item.UnitCount;
            itemPackage.Description = string.Empty;

            genericMgr.Create(item);
            genericMgr.Create(itemPackage);
        }

        [Transaction(TransactionMode.Requires)]
        public void UpdateItem(Item item)
        {
            string hql = "from ItemPackage where Item = ? and UnitCount = ?";
            IList<ItemPackage> itemPackageList = genericMgr.FindAll<ItemPackage>(hql, new object[] { item.Code, item.UnitCount });
            if (itemPackageList != null && itemPackageList.Count > 0)
            {
                ItemPackage itemPackage = itemPackageList[0];
                if (!itemPackage.IsDefault)
                {
                    itemPackage.IsDefault = true;
                    genericMgr.Update(itemPackage);
                }
            }
            else
            {
                #region 原默认包装至False
                hql = "from ItemPackage as i where i.Item = ? and i.IsDefault= ?";
                IList<ItemPackage> defualtItemPackageList = genericMgr.FindAll<ItemPackage>(hql, new object[] { item.Code, true });
                for (int i = 0; i < defualtItemPackageList.Count; i++)
                {
                    defualtItemPackageList[i].IsDefault = false;
                    this.genericMgr.Update(defualtItemPackageList[i]);
                }
                #endregion

                #region 没有找到包装，新增包装
                ItemPackage itemPackage = new ItemPackage();
                itemPackage.Item = item.Code;
                itemPackage.IsDefault = true;
                itemPackage.UnitCount = item.UnitCount;
                itemPackage.Description = string.Empty;
                this.genericMgr.Create(itemPackage);
                #endregion
            }

            genericMgr.Update(item);
        }

        public IList<ItemDiscontinue> GetItemDiscontinues(string itemCode, DateTime effectiveDate)
        {
            string hql = "from ItemDiscontinue where Item = ? and StartDate < ? and (EndDate is null or EndDate >= ?)";
            return this.genericMgr.FindAll<ItemDiscontinue>(hql, new object[] { itemCode, effectiveDate, effectiveDate });
        }

        public IList<ItemDiscontinue> GetParentItemDiscontinues(string itemCode, DateTime effectiveDate)
        {
            string hql = "from ItemDiscontinue where DiscontinueItem = ? and StartDate < ? and (EndDate is null or EndDate >= ?)";
            return this.genericMgr.FindAll<ItemDiscontinue>(hql, new object[] { itemCode, effectiveDate, effectiveDate });
        }

        public IList<Item> GetItems(IList<string> itemCodeList)
        {
            if (itemCodeList != null && itemCodeList.Count > 0)
            {
                IList<Item> itemList = new List<Item>();

                string hql = string.Empty;
                IList<object> parm = new List<object>();

                foreach (string itemCode in itemCodeList.Distinct())
                {
                    if (hql == string.Empty)
                    {
                        hql = "from Item where Code in (?";
                    }
                    else
                    {
                        hql += ", ?";
                    }
                    parm.Add(itemCode);

                    if (parm.Count() >= 2000)
                    {
                        hql += ")";
                        ((List<Item>)itemList).AddRange(this.genericMgr.FindAll<Item>(hql, parm.ToArray()));
                        hql = string.Empty;
                        parm = new List<object>();
                    }
                }

                hql += ")";
                if (parm.Count() > 0)
                {
                    ((List<Item>)itemList).AddRange(this.genericMgr.FindAll<Item>(hql, parm.ToArray()));
                }

                return itemList;
            }

            return null;
        }


        [Transaction(TransactionMode.Requires)]
        public void CreateItemTraceXls(Stream inputStream)
        {
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);
            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();
            ImportHelper.JumpRows(rows, 10);
            BusinessException businessException = new BusinessException();
            #region 列定义
            int colItem = 1;//车系
            #endregion
            IList<ItemTrace> exactItemTraceList = new List<ItemTrace>();
            IList<Item> allItemList = this.genericMgr.FindAll<Item>();
            IList<ItemTrace> allItemTraceList = this.genericMgr.FindAll<ItemTrace>();
            int i = 10;
            while (rows.MoveNext())
            {
                i++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 2))
                {
                    break;//边界
                }
                string itemCode = string.Empty;
                ItemTrace itemTrace = new ItemTrace();
                #region 读取数据
                itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    businessException.AddMessage(string.Format("第{0}行关键件不能为空", i));
                }
                else
                {
                    var items = allItemList.FirstOrDefault(a => a.Code == itemCode);
                    var existsItemTrace = allItemTraceList.FirstOrDefault(a => a.Item == itemCode);
                    //var duplicateItemTrace=
                    if (items == null)
                    {
                        businessException.AddMessage(string.Format("第{0}行{1}物料编号不存在。", i, itemCode));
                    }
                    else if (existsItemTrace!=null)
                    {
                        businessException.AddMessage(string.Format("第{0}行{1}关键件已经存在。", i, itemCode));
                    }
                    else
                    {
                        itemTrace.Item = items.Code;
                        itemTrace.ItemDescription = items.Description;
                        if (exactItemTraceList == null || exactItemTraceList.Count == 0)
                        {
                            exactItemTraceList.Add(itemTrace);
                        }
                        else
                        {
                            bool isDuplicate = exactItemTraceList.Where(e => e.Item == itemCode).Count()>0;
                            if (isDuplicate)
                            {
                                businessException.AddMessage(string.Format("第{0}行{1}关键件在模板中重复。", i, itemCode));
                            }
                            else
                            {
                                exactItemTraceList.Add(itemTrace);
                            }
                        }
                        
                    }
                }
                #endregion
            }
            if (businessException.HasMessage)
            {
                throw businessException;
            }
            if (exactItemTraceList == null || exactItemTraceList.Count == 0)
            {
                throw new BusinessException("模版为空，请确认。");
            }
            foreach (ItemTrace itemTrace in exactItemTraceList)
            {
                genericMgr.Create(itemTrace);
            }
        }

        //[Transaction(TransactionMode.Requires)]
        public void BatchUpdateItemXls(Stream inputStream)
        {
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);
            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();
            ImportHelper.JumpRows(rows, 10);
            BusinessException businessException = new BusinessException();
            #region 列定义
            int colItem = 1;//件号
            int colMinUC = 4;//上线包装
            int colContainer = 5;//容器代码
            int colContainerDesc = 6;//容器描述
            #endregion
            IList<Item> exactItemList = new List<Item>();
            int rowCount = 10;
            while (rows.MoveNext())
            {
                rowCount++;
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 2))
                {
                    break;//边界
                }
                string itemCode = string.Empty;
                decimal minUC = 0;
                string container = string.Empty;
                string containerDesc = string.Empty;
                Item item = new Item();
                #region 读取数据

                #region 件号
                itemCode = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    businessException.AddMessage(string.Format("第{0}行物料编号不能为空", rowCount));
                    continue;
                }
                else
                {
                    var items = this.genericMgr.FindAll<Item>(" select i from Item as i where i.Code=? ", itemCode);
                    if (items == null)
                    {
                        businessException.AddMessage(string.Format("第{0}行物料编号{1}不存在。", rowCount, itemCode));
                        continue;
                    }
                    else
                    {
                        item = items.First();
                    }
                }
                #endregion

                #region 上线包装
                string readMinUC = ImportHelper.GetCellStringValue(row.GetCell(colMinUC));
                if (string.IsNullOrWhiteSpace(readMinUC))
                {
                    //businessException.AddMessage(string.Format("第{0}行上线包装不能为空", rowCount));
                }
                else
                {
                    if (decimal.TryParse(readMinUC, out minUC))
                    {
                        item.MinUnitCount = minUC;
                    }
                    else
                    {
                        businessException.AddMessage(string.Format("第{0}行上线包装{1}，填写有误。", rowCount, readMinUC));
                    }
                }
                #endregion

                #region 容器代码
                container = ImportHelper.GetCellStringValue(row.GetCell(colContainer));
                if (string.IsNullOrWhiteSpace(container))
                {
                    //businessException.AddMessage(string.Format("第{0}行容器代码不能为空", rowCount));
                }
                else
                {
                    item.Container = container;
                }
                #endregion

                #region 容器描述
                containerDesc = ImportHelper.GetCellStringValue(row.GetCell(colContainerDesc));
                if (string.IsNullOrWhiteSpace(containerDesc))
                {
                    //businessException.AddMessage(string.Format("第{0}行容器描述不能为空", rowCount));
                }
                else
                {
                    item.ContainerDesc = containerDesc;
                }
                #endregion

                #endregion

                if (exactItemList == null || exactItemList.Count == 0)
                {
                    exactItemList.Add(item);
                }
                else if (exactItemList.Where(e => e.Code == item.Code).Count() > 0)
                {
                    businessException.AddMessage(string.Format("第{0}行物料编号{1}在模板中重复。", rowCount, itemCode));
                }
                else
                {
                    exactItemList.Add(item);
                }
            }
            if (businessException.HasMessage)
            {
                throw businessException;
            }
            if (exactItemList == null || exactItemList.Count == 0)
            {
                throw new BusinessException("模版为空，请确认。");
            }
            foreach (Item item in exactItemList)
            {
                genericMgr.Update(item);
                this.genericMgr.Update(" update FlowDetail set MinUnitCount=?,UnitCount=?, Container =?,ContainerDesc=? where Item=? and exists( select 1 from FlowMaster as m where m.Code=Flow and m.Type=2 and exists( select 1 from FlowStrategy as fs where fs.Flow=m.Code and fs.Strategy<>7 ) )", new object[] { item.MinUnitCount, item.MinUnitCount, item.Container, item.ContainerDesc, item.Code }, new IType[] { NHibernate.NHibernateUtil.Decimal, NHibernate.NHibernateUtil.Decimal, NHibernate.NHibernateUtil.String, NHibernate.NHibernateUtil.String, NHibernate.NHibernateUtil.String });
                this.genericMgr.Update(" update FlowDetail set MinUnitCount=?, Container =? where Item=? and exists( select 1 from FlowMaster as m where m.Code=Flow and m.Type=1 ) ", new object[] { item.MinUnitCount, item.Container, item.Code }, new IType[] { NHibernate.NHibernateUtil.Decimal, NHibernate.NHibernateUtil.String,  NHibernate.NHibernateUtil.String });
                //this.genericMgr.Update(" update KanbanCard set Qty=?, Container =? where Item=? ", new object[] { item.MinUnitCount, item.Container, item.Code }, new IType[] { NHibernate.NHibernateUtil.Decimal, NHibernate.NHibernateUtil.String, NHibernate.NHibernateUtil.String });
                
            }
        }


        #region 保管员
        [Transaction(TransactionMode.Requires)]
        public void CreateCustodian(Custodian custodian)
        {
            string ItemCodeStr = custodian.ItemCodes.Replace("\r\n", ",");
            string[] ItemCodeArray = ItemCodeStr.Split(',');
            BusinessException businessException = new BusinessException();
            #region 判定Item是否有效
            foreach (string itemCode in ItemCodeArray.Distinct())
            {
                if (itemCode != null && itemCode != "")
                {
                    IList<Item> items = this.genericMgr.FindAll<Item>("from Item where Code='" + itemCode + "'");
                    if (items == null || items.Count == 0)
                    {
                        businessException.AddMessage(itemCode.ToString() + ",");
                    }
                }
            }
            if (businessException.HasMessage)
            {
                businessException.AddMessage("不存在。");
                throw businessException;
            }

            #endregion

            #region 判定物料是否已经分配保管员
            string hql = string.Empty;
            IList<object> parm = new List<object>();
            foreach (string itemCode in ItemCodeArray.Distinct())
            {
                if (hql == string.Empty)
                {
                    hql = "from Custodian where Location=? and UserCode=? and item in (?";
                    parm.Add(custodian.Location);
                    parm.Add(custodian.UserCode);
                }
                else
                {
                    hql += ", ?";
                }
                parm.Add(itemCode);

            }

            hql += ")";
            IList<Custodian> CustodianList = this.genericMgr.FindAll<Custodian>(hql, parm.ToArray());

            if (CustodianList != null && CustodianList.Count > 0)
            {

                foreach (Custodian cd in CustodianList)
                {
                    businessException.AddMessage(cd.Item.ToString() + ",");
                }
                if (businessException.HasMessage)
                {
                    businessException.AddMessage("已经分配保管员。");
                    throw businessException;
                }
            }
            #endregion

            foreach (string itemCode in ItemCodeArray.Distinct())
            {
                if (itemCode != "")
                {
                    Custodian cus = new Custodian();
                    cus.Item = itemCode;
                    cus.Location = custodian.Location;
                    cus.UserCode = custodian.UserCode;

                    this.genericMgr.Create(cus);
                }
            }


        }
        #endregion
        #endregion

        #region private methods
        private decimal? NestConvertItemUomQty(IList<UomConversion> itemUomConversionList, string sourceUomCode, string targetUomCode, decimal convQty, bool isUomReversed)
        {
            IList<UomConversion> matchedUomConversionList = (from conv in itemUomConversionList
                                                             where (conv.BaseUom == sourceUomCode)   //用Source Uom匹配
                                                             || (conv.AlterUom == sourceUomCode)
                                                             orderby conv.Item descending            //把Item有值的放前面
                                                             select conv).ToList();

            foreach (UomConversion matchedUomConversion in matchedUomConversionList)
            {
                if (matchedUomConversion.BaseUom == sourceUomCode)
                {
                    sourceUomCode = matchedUomConversion.AlterUom;
                    if (isUomReversed)
                    {
                        convQty = (convQty * matchedUomConversion.AlterQty / matchedUomConversion.BaseQty);
                    }
                    else
                    {
                        convQty = (convQty * matchedUomConversion.BaseQty / matchedUomConversion.AlterQty);
                    }
                }
                else
                {
                    sourceUomCode = matchedUomConversion.BaseUom;
                    if (isUomReversed)
                    {
                        convQty = (convQty * matchedUomConversion.BaseQty / matchedUomConversion.AlterQty);
                    }
                    else
                    {
                        convQty = (convQty * matchedUomConversion.AlterQty / matchedUomConversion.BaseQty);
                    }
                }

                if (sourceUomCode == targetUomCode)
                {
                    //新的Source Uom等于Target Uom, 找到目标转换单位
                    return convQty;
                }
                else
                {
                    //没有找到目标转换单位
                    //先过滤掉已经匹配过的转换
                    IList<UomConversion> filteredUomConversionList = itemUomConversionList.Where(m => !m.Equals(matchedUomConversion)).ToList();

                    //再嵌套往下查找
                    decimal? targetQty = NestConvertItemUomQty(filteredUomConversionList, sourceUomCode, targetUomCode, convQty, false);
                    if (targetQty.HasValue)
                    {
                        return targetQty;
                    }
                    else
                    {
                        targetQty = NestConvertItemUomQty(filteredUomConversionList, targetUomCode, sourceUomCode, convQty, true);
                    }
                }
            }

            return null;
        }
        #endregion
    }
}
