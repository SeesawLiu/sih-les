using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Sconit.Service.SAP
{
    public class MasterDataMgrImpl
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("MasterDateImport");
        public IGenericMgr genericMgr { get; set; }

        /// <summary>
        /// 只取工厂0084
        /// </summary>
        /// <param name="itemList"></param>
        public void CreateOrUpdateItem(List<Entity.SAP.MD.Item> itemList)
        {
            itemList = itemList.Where(i => i.Plant == "0084").ToList();
            foreach (var item in itemList)
            {
                try
                {
                    var baseItemList = this.genericMgr.FindAll<Entity.MD.Item>();
                    var q_baseItem = baseItemList.Where(i => i.Code == item.Code);
                    if (q_baseItem == null || q_baseItem.Count() == 0)
                    {
                        var baseItem = new Entity.MD.Item();
                        baseItem.Code = item.Code;
                        baseItem.Description = item.Description;
                        baseItem.Uom = item.Uom;
                        //baseItem.Plant = item.Plant;
                        baseItem.ReferenceCode = item.ReferenceCode;
                        this.genericMgr.Create(baseItem);
                        //是否记录log
                    }
                    else
                    {
                        var baseItem = q_baseItem.Single();
                        if (baseItem.Description != item.Description)
                        {
                            baseItem.Description = item.Description;
                            this.genericMgr.Update(baseItem);
                        }
                        if (baseItem.Uom != item.Uom)
                        {
                            //log
                        }
                        if (baseItem.ReferenceCode != item.ReferenceCode)
                        {
                            //log
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("MasterDateImport.Item", ex);
                    //log todo Send Message
                    continue;
                }
            }
        }
    }
}
