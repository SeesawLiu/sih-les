using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.Sconit.CodeMaster;
using com.Sconit.Entity;
using com.Sconit.Entity.PRD;
using com.Sconit.Entity.SCM;
using NPOI.HSSF.UserModel;
using com.Sconit.Entity.Exception;
using NPOI.SS.UserModel;
using System.Collections;
using com.Sconit.Utility;
using com.Sconit.Entity.MD;
using Castle.Services.Transaction;

namespace com.Sconit.Service.Impl
{
    public class MultiSupplyResult
    {
        public bool canScan;
        public string nextSupplier;
    }

    [Transactional]
    public class MultiSupplyGroupMgrImpl : BaseMgr, IMultiSupplyGroupMgr
    {
        public IGenericMgr genericMgr { get; set; }
        private static log4net.ILog log = log4net.LogManager.GetLogger("DebugLog");

        private static string duiplicateItemVerifyStatement = @"select msi from MultiSupplyItem as msi where msi.Item = ? ";
        private static string duiplicateSupplierVerifyStatement = @"select mss from MultiSupplySupplier as mss where mss.GroupNo = ? and mss.Supplier = ?";
        private static string duiplicateVerifyStatement = @"select msg from MultiSupplyGroup as msg where msg.GroupNo = ? ";
        private static string selectMaxSupplierSeq = "select max(Seq) from MultiSupplySupplier as mss where mss.GroupNo = ? ";

        [Transaction(TransactionMode.Requires)]
        public string CreateMultiSupplyItemXlsx(Stream inputStream)
        {
            if (inputStream.Length == 0)
            {
                throw new BusinessException("Import.Stream.Empty");
            }

            HSSFWorkbook workbook = new HSSFWorkbook(inputStream);

            ISheet sheet = workbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();

            ImportHelper.JumpRows(rows, 10);

            #region 列定义
            int colGroupNo = 1; // 多轨组号
            int colDescription = 2; // 供应商代码
            int colSupplier = 3; // 供应商代码
            int colCycleQty = 4; // 循环量
            int colProportion = 5; //循环量比例
            int colSubstituteGroup = 6; //SAP替代组
            int colItem = 7; // 物料编号

            #endregion

            var errorMessage = new BusinessException();
            int colCount = 0;
            IList<MultiSupplyItem> exactMultiSupplyItem = new List<MultiSupplyItem>();
            IList<MultiSupplySupplier> exactMultiSupplySupplier = new List<MultiSupplySupplier>();
            IList<MultiSupplyGroup> exactMultiSupplyGroup = new List<MultiSupplyGroup>();
            IList<Supplier> supplierList = new List<Supplier>();
            IList<Item> itemList = new List<Item>();
            while (rows.MoveNext())
            {
                HSSFRow row = (HSSFRow)rows.Current;
                if (!ImportHelper.CheckValidDataRow(row, 1, 5))
                {
                    break;//边界
                }
                colCount++;

                var rowErrors = new List<Message>();

                string GroupNo = string.Empty;
                string description = string.Empty;
                string Item = string.Empty;
                string Supplier = string.Empty;
                string CycleQty = string.Empty;

                #region 读取数据
                GroupNo = ImportHelper.GetCellStringValue(row.GetCell(colGroupNo));
                if (GroupNo == null || GroupNo.Trim() == string.Empty)
                {
                    rowErrors.Add(new Message(MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), Resources.PRD.MultiSupplyGroup.MultiSupplyGroup_GroupNo));
                }
                else
                {
                    GroupNo = GroupNo.ToUpper();
                }

                description = ImportHelper.GetCellStringValue(row.GetCell(colDescription));

                Supplier = ImportHelper.GetCellStringValue(row.GetCell(colSupplier));
                if (Supplier == null || Supplier.Trim() == string.Empty)
                {
                    rowErrors.Add(new Message(MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), Resources.PRD.MultiSupplySupplier.MultiSupplySupplier_Supplier));
                }
                else
                {
                    Supplier = Supplier.ToUpper();
                }

                CycleQty = ImportHelper.GetCellStringValue(row.GetCell(colCycleQty));
                int cy;
                int.TryParse(CycleQty, out cy);
                if (cy <= 0)
                {
                    rowErrors.Add(new Message(MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeZero, colCount.ToString(), Resources.PRD.MultiSupplySupplier.MultiSupplySupplier_CycleQty));
                }

                Item = ImportHelper.GetCellStringValue(row.GetCell(colItem));
                if (Item == null || Item.Trim() == string.Empty)
                {
                    rowErrors.Add(new Message(MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldCanNotBeNull, colCount.ToString(), Resources.PRD.MultiSupplyItem.MultiSupplyItem_Item));
                }
                else
                {
                    Item = Item.ToUpper();
                }

                string proportion = ImportHelper.GetCellStringValue(row.GetCell(colProportion));
                string substituteGroup = ImportHelper.GetCellStringValue(row.GetCell(colSubstituteGroup));

                //Excel重复性验证
                Item ItemInstance = null;
                if (!string.IsNullOrWhiteSpace(GroupNo) && !string.IsNullOrWhiteSpace(Supplier) && !string.IsNullOrWhiteSpace(Item))
                {
                    MultiSupplyItem innerSameMultiSupplyItem = exactMultiSupplyItem.FirstOrDefault(m => m.Item.ToUpper() == Item);
                    if (innerSameMultiSupplyItem != null)
                    {
                        if (innerSameMultiSupplyItem.GroupNo.ToUpper() != GroupNo)
                        {
                            rowErrors.Add(new Message(MessageType.Error, Resources.PRD.MultiSupplyItem.MultiSupplyItem_Import_ItemOnlyBelongOneGroup, colCount.ToString(), Item, innerSameMultiSupplyItem.GroupNo));
                        }
                        else if (innerSameMultiSupplyItem.GroupNo.ToUpper() == GroupNo && innerSameMultiSupplyItem.Supplier.ToUpper() == Supplier)
                        {
                            rowErrors.Add(new Message(MessageType.Error, Resources.PRD.MultiSupplyItem.MultiSupplyItem_Import_ItemOnlyBelongOneGroupDiffSupplier, colCount.ToString(), Item, innerSameMultiSupplyItem.GroupNo, innerSameMultiSupplyItem.Supplier));
                        }
                    }

                    //循环量相同性验证
                    if (exactMultiSupplySupplier.FirstOrDefault(m => m.GroupNo.ToUpper() == GroupNo && m.Supplier.ToUpper() == Supplier && m.CycleQty != cy) != null)
                    {
                        rowErrors.Add(new Message(MessageType.Error, Resources.PRD.MultiSupplyItem.MultiSupplyItem_Import_GroupSupplierDiffCycleQty, colCount.ToString(), GroupNo, Supplier));
                    }
                    //供应商存在性验证
                    if (supplierList.FirstOrDefault(i => i.Code.ToUpper() == Supplier) == null)
                    {
                        IList<Supplier> SupplierCount = this.genericMgr.FindAll<Supplier>("from Supplier as s where s.Code = ?", Supplier);
                        if (SupplierCount == null || SupplierCount.Count() == 0)
                        {
                            rowErrors.Add(new Message(MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldValueNotExist, colCount.ToString(), Resources.PRD.MultiSupplyGroup.MultiSupplyGroup_Supplier, Supplier));
                        }
                        else
                        {
                            supplierList.Add(SupplierCount[0]);
                        }
                    }

                    //零件存在性
                    ItemInstance = itemList.FirstOrDefault(i => i.Code.ToUpper() == Item);
                    if (ItemInstance == null)
                    {
                        IList<Item> ItemCount = this.genericMgr.FindAll<Item>("from Item as i where i.Code= ?", Item);
                        if (ItemCount == null || ItemCount.Count() == 0)
                        {
                            rowErrors.Add(new Message(MessageType.Error, Resources.ErrorMessage.Errors_Import_LineFieldValueNotExist, colCount.ToString(), colCount.ToString(), Resources.PRD.MultiSupplyItem.MultiSupplyItem_Item, Item));
                        }
                        else
                        {
                            ItemInstance = ItemCount[0];
                            itemList.Add(ItemCount[0]);
                        }
                    }
                    //数据库重复性验证
                    var items = this.genericMgr.FindAll<MultiSupplyItem>(duiplicateItemVerifyStatement, new object[] { Item });
                    var sameItemInDiffGroup = items.FirstOrDefault(c => c.GroupNo.ToUpper() != GroupNo);
                    if (sameItemInDiffGroup != null)
                    {
                        rowErrors.Add(new Message(MessageType.Error, Resources.PRD.MultiSupplyItem.MultiSupplyItem_Import_ItemOnlyBelongOneGroup, colCount.ToString(), Item, sameItemInDiffGroup.GroupNo));
                    }

                    var sameItemInSameGroupandSupplier = items.FirstOrDefault(c => c.GroupNo.ToUpper() == GroupNo && c.Supplier.ToUpper() == Supplier);
                    if (sameItemInSameGroupandSupplier != null)
                    {
                        rowErrors.Add(new Message(MessageType.Error, Resources.PRD.MultiSupplyItem.MultiSupplyItem_Import_ItemOnlyBelongOneGroupDiffSupplier, colCount.ToString(), Item, sameItemInSameGroupandSupplier.GroupNo, sameItemInSameGroupandSupplier.Supplier));
                    }
                }

                if (rowErrors.Count > 0)
                {
                    errorMessage.AddMessages(rowErrors);
                }
                else
                {
                    var msi = new MultiSupplyItem
                                              {
                                                  GroupNo = GroupNo,
                                                  Supplier = Supplier,
                                                  Item = ItemInstance.Code,
                                                  ItemDescription = ItemInstance.Description,
                                                  SubstituteGroup = substituteGroup,
                                              };
                    exactMultiSupplyItem.Add(msi);

                    if (exactMultiSupplySupplier.FirstOrDefault(m => m.GroupNo.ToUpper() == GroupNo && m.Supplier.ToUpper() == Supplier) == null)
                    {
                        var suppliers = this.genericMgr.FindAll<MultiSupplySupplier>(duiplicateSupplierVerifyStatement,
                                                                                     new object[] { GroupNo, Supplier });
                        if (suppliers == null || suppliers.Count() == 0)
                        {
                            var mss = new MultiSupplySupplier
                                                          {
                                                              GroupNo = GroupNo,
                                                              Supplier = Supplier,
                                                              CycleQty = cy,
                                                              IsActive = true,
                                                              Proportion = proportion
                                                          };
                            exactMultiSupplySupplier.Add(mss);
                        }
                    }

                    if (exactMultiSupplyGroup.FirstOrDefault(m => m.GroupNo.ToUpper() == GroupNo) == null)
                    {
                        var msgGroupNo = this.genericMgr.FindAll<MultiSupplyGroup>(duiplicateVerifyStatement, GroupNo);
                        if (msgGroupNo == null || msgGroupNo.Count() == 0)
                        {
                            var msg = new MultiSupplyGroup { GroupNo = GroupNo, Description = description };
                            exactMultiSupplyGroup.Add(msg);
                        }
                    }
                }

                #endregion
            }

            foreach (MultiSupplyGroup msGroup in exactMultiSupplyGroup)
            {
                IList<MultiSupplySupplier> msSupplierList = exactMultiSupplySupplier.Where(p => p.GroupNo.ToUpper() == msGroup.GroupNo.ToUpper()).ToList();
                if (msSupplierList.Count <= 1)
                {
                    errorMessage.AddMessage("导入的数据中，多轨组{0}，必须包含大于1个供应商才可以创建。", msGroup.GroupNo);
                }
            }

            if (errorMessage.HasMessage)
            {
                throw errorMessage;
            }

            #region SaveData
            foreach (MultiSupplyGroup msGroup in exactMultiSupplyGroup)
            {
                this.genericMgr.Create(msGroup);

                IList<MultiSupplySupplier> msSupplierList = exactMultiSupplySupplier.Where(p => p.GroupNo.ToUpper() == msGroup.GroupNo.ToUpper()).ToList();

                int count = 1;
                foreach (MultiSupplySupplier msSupplier in msSupplierList)
                {
                    msSupplier.Seq = count;
                    this.genericMgr.Create(msSupplier);
                    count++;
                    exactMultiSupplySupplier.Remove(msSupplier);
                }
            }

            if (exactMultiSupplySupplier.Count > 0)
            {
                var groups = exactMultiSupplySupplier.Select(c => c.GroupNo.ToUpper()).Distinct();
                foreach (var group in groups)
                {
                    var dbbMaxSeq = this.genericMgr.FindAll(selectMaxSupplierSeq, group)[0];
                    int maxSeq;
                    int.TryParse((dbbMaxSeq != null ? dbbMaxSeq.ToString() : "0"), out maxSeq);

                    foreach (MultiSupplySupplier msSupplier in exactMultiSupplySupplier.Where(c => c.GroupNo.ToUpper() == group))
                    {
                        maxSeq++;
                        msSupplier.Seq = maxSeq;
                        this.genericMgr.Create(msSupplier);
                    }
                }
            }

            foreach (MultiSupplyItem msItem in exactMultiSupplyItem)
            {
                this.genericMgr.Create(msItem);
            }
            #endregion
            
            return Resources.Global.ImportSuccess_BatchImportSuccessful;
        }

        [Transaction(TransactionMode.Requires)]
        public void DeleteMultiSupplyGroup(string groupNo)
        {
            this.genericMgr.Delete(string.Format("from MultiSupplySupplier as s where s.GroupNo = '{0}'", groupNo));
            this.genericMgr.Delete(string.Format("from MultiSupplyItem as s where s.GroupNo = '{0}'", groupNo));
            this.genericMgr.DeleteById<MultiSupplyGroup>(groupNo);
        }
    }
}
