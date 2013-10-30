

namespace com.Sconit.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using com.Sconit.Entity.Exception;
    using System.IO;
    using com.Sconit.Entity.ACC;
    using com.Sconit.Entity;

    public static class SecurityHelper
    {
        public static void AddPartyFromPermissionStatement(ref string whereStatement, string partyFromTableAlias, string partyFromFieldName, com.Sconit.CodeMaster.OrderType orderType, bool isSupplier)
        {
            //su特殊处理，不用考虑权限
            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() != "su")
            {
                if (whereStatement == string.Empty)
                {
                    if (orderType == com.Sconit.CodeMaster.OrderType.Procurement)
                    {
                        if (isSupplier)
                        {
                            whereStatement = " where  exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + " and up.PermissionCategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + " and up.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + ")";
                        }
                        else
                        {
                            whereStatement = " where (" + partyFromTableAlias + ".IsCheckPartyFromAuthority = 0  or ( exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + "and  up.PermissionCategoryType in ( " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "." + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + " )  and up.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + ")))";
                        }
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Distribution)
                    {
                        whereStatement = " where (" + partyFromTableAlias + ".IsCheckPartyFromAuthority = 0  or ( exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + "and  up.PermissionCategory =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and up.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + ")))";
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Production)
                    {
                        whereStatement = " where (" + partyFromTableAlias + ".IsCheckPartyFromAuthority = 0  or ( exists (select 1 from UserPermissionView as up where  up.UserId =" + user.Id + "and up.PermissionCategory =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and up.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + ")))";
                    }
                }
                else
                {
                    if (orderType == com.Sconit.CodeMaster.OrderType.Procurement)
                    {
                        if (isSupplier)
                        {
                            whereStatement += " and  exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + " and up.PermissionCategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + " and up.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + ")";
                        }
                        else
                        {
                            whereStatement += " and (" + partyFromTableAlias + ".IsCheckPartyFromAuthority = 0  or ( exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + "and  up.PermissionCategoryType in ( " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + " ) and up.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + ")))";
                        }
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Distribution)
                    {
                        whereStatement += " and (" + partyFromTableAlias + ".IsCheckPartyFromAuthority = 0  or ( exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + "and  up.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and up.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + ")))";
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Production)
                    {
                        whereStatement += " and (" + partyFromTableAlias + ".IsCheckPartyFromAuthority = 0  or ( exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + "and  up.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and up.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + ")))";
                    }

                }
            }
        }

        public static void AddPartyToPermissionStatement(ref string whereStatement, string partyToTableAlias, string partyToFieldName, com.Sconit.CodeMaster.OrderType orderType)
        {
            //su特殊处理，不用考虑权限
            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() != "su")
            {
                if (whereStatement == string.Empty)
                {

                    if (orderType == com.Sconit.CodeMaster.OrderType.Procurement)
                    {
                        whereStatement = " where (" + partyToTableAlias + ".IsCheckPartyToAuthority = 0 or (exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + "and  up.PermissionCategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and up.PermissionCode = " + partyToTableAlias + "." + partyToFieldName + ")))";
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Distribution)
                    {
                        whereStatement = " where (" + partyToTableAlias + ".IsCheckPartyToAuthority = 0 or (exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + "and  up.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + " )  and up.PermissionCode = " + partyToTableAlias + "." + partyToFieldName + ")))";
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Production)
                    {
                        whereStatement = " where (" + partyToTableAlias + ".IsCheckPartyToAuthority = 0 or (exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + "and  up.PermissionCategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and up.PermissionCode = " + partyToTableAlias + "." + partyToFieldName + ")))";
                    }
                }
                else
                {

                    if (orderType == com.Sconit.CodeMaster.OrderType.Procurement)
                    {
                        whereStatement += " and (" + partyToTableAlias + ".IsCheckPartyToAuthority = 0  or (exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + "and  up.PermissionCategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and up.PermissionCode = " + partyToTableAlias + "." + partyToFieldName + ")))";
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Distribution)
                    {
                        whereStatement += " and (" + partyToTableAlias + ".IsCheckPartyToAuthority = 0 or (exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + "and  up.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + " )  and up.PermissionCode = " + partyToTableAlias + "." + partyToFieldName + ")))";
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Production)
                    {
                        whereStatement += " and (" + partyToTableAlias + ".IsCheckPartyToAuthority = 0 or (exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + "and  up.PermissionCategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and up.PermissionCode = " + partyToTableAlias + "." + partyToFieldName + ")))";
                    }

                }
            }
        }

        //参数OrderType指菜单类别：供货、发货、生产
        //参数orderType指订单类型：采购/生产/移库/销售
        public static void AddPartyFromAndPartyToPermissionStatement(ref string whereStatement, string orderTypeTableAlias, string orderTypeFieldName, string partyFromTableAlias, string partyFromFieldName, string partyToTableAlias, string partyToFieldName, com.Sconit.CodeMaster.OrderType orderType, bool isSupplier)
        {
            //su特殊处理，不用考虑权限
            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() != "su")
            {
                if (whereStatement == string.Empty)
                {

                    if (orderType == com.Sconit.CodeMaster.OrderType.Procurement)
                    {
                        if (isSupplier)
                            whereStatement = " where " + orderTypeTableAlias + "." + orderTypeFieldName + " in (" + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + ")" +
                                             " and  exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and p.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + ")";
                        else
                            whereStatement = " where ((" + orderTypeTableAlias + "." + orderTypeFieldName + " in (" + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + ") and ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and p.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + "))" +
                                             " and  (exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyToTableAlias + "." + partyToFieldName + ")))" +
                                             " or (" + orderTypeTableAlias + "." + orderTypeFieldName + " in (" + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ") and ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyToTableAlias + "." + partyToFieldName +"))))"; 
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Distribution)
                    {
                        whereStatement = " where ((" + orderTypeTableAlias + "." + orderTypeFieldName + " = " + (int)com.Sconit.CodeMaster.OrderType.Distribution + " and ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and p.PermissionCode =  " + partyToTableAlias + "." + partyToFieldName + ")) and  (exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + ")))" +
                                         " or (" + orderTypeTableAlias + "." + orderTypeFieldName + " in (" + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ") and ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + "))))";
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Production)
                    {
                        whereStatement = " where (( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType  =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + "))" +
                                         " or (( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyToTableAlias + "." + partyToFieldName + ")))";
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Transfer)
                    {
                        whereStatement = " where (( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType  =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + "))" +
                                         " or (exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyToTableAlias + "." + partyToFieldName + ")))";
                    }
                }
                else
                {
                    if (orderType == com.Sconit.CodeMaster.OrderType.Procurement)
                    {
                        if (isSupplier)
                            whereStatement += " and " + orderTypeTableAlias + "." + orderTypeFieldName + " in (" + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + ")" +
                                              " and  exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and p.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + ")";
                        else
                            whereStatement += " and ((" + orderTypeTableAlias + "." + orderTypeFieldName + " in (" + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.ScheduleLine + ") and ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and p.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + "))" +
                                              " and  (exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyToTableAlias + "." + partyToFieldName + ")))" +
                                              " or (" + orderTypeTableAlias + "." + orderTypeFieldName + " in (" + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ") and ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyToTableAlias + "." + partyToFieldName + "))))";
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Distribution)
                    {
                        whereStatement += " and ((" + orderTypeTableAlias + "." + orderTypeFieldName + " = " + (int)com.Sconit.CodeMaster.OrderType.Distribution + " and ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + ") and p.PermissionCode =  " + partyToTableAlias + "." + partyToFieldName + ")) and  (exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + ")))" +
                                          " or (" + orderTypeTableAlias + "." + orderTypeFieldName + " in (" + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + ") and ( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + "))))";
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Production)
                    {
                        whereStatement += " and (( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType  =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + "))" +
                                          " or (exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyToTableAlias + "." + partyToFieldName + ")))";
                    }
                    else if (orderType == com.Sconit.CodeMaster.OrderType.Transfer)
                    {
                        whereStatement += " and (( exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType  =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyFromTableAlias + "." + partyFromFieldName + "))" +
                                          " or (exists (select 1 from UserPermissionView as p where p.UserId =" + user.Id + "and  p.PermissionCategoryType =" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + " and p.PermissionCode = " + partyToTableAlias + "." + partyToFieldName + ")))";
                    }
                }
            }
        }

        public static void AddRegionPermissionStatement(ref string whereStatement, string regionTableAlias, string regionFieldName)
        {
            //su特殊处理，不用考虑权限
            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() != "su")
            {
                if (whereStatement == string.Empty)
                {
                    whereStatement = " where exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + "and  up.PermissionCategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and up.PermissionCode = " + regionTableAlias + "." + regionFieldName + ")";
                }
                else
                {
                    whereStatement += " and exists (select 1 from UserPermissionView as up where up.UserId =" + user.Id + "and  up.PermissionCategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and up.PermissionCode = " + regionTableAlias + "." + regionFieldName + ")";
                }
            }
        }

        public static void AddLocationPermissionStatement(ref string whereStatement, string locationTableAlias, string locationFieldName)
        {
            //su特殊处理，不用考虑权限
            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() != "su")
            {
                if (whereStatement == string.Empty)
                {
                    whereStatement = " where exists (select 1 from UserPermissionView as up,Location as ln where up.UserId =" + user.Id + "and  up.PermissionCategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and up.PermissionCode = ln.Region and ln.Code = " + locationTableAlias + "." + locationFieldName + ")";
                }
                else
                {
                    whereStatement += " and exists (select 1 from UserPermissionView as up,Location as ln where up.UserId =" + user.Id + "and  up.PermissionCategoryType = " + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "  and up.PermissionCode = ln.Region and ln.Code = " + locationTableAlias + "." + locationFieldName + ")";
                }
            }
        }

        public static void AddFlowPermissionStatement(ref string whereStatement, string flowTableAlias, string flowFieldName)
        {
            //su特殊处理，不用考虑权限
            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() != "su")
            {
                if (whereStatement == string.Empty)
                {
                    whereStatement = " where exists (select 1 from UserPermissionView as up1,FlowMaster as fm1 where (fm1.IsCheckPartyFromAuthority = 0) or (up1.UserId =" + user.Id + " and  up1.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ")  and up1.PermissionCode = fm1.PartyFrom and fm1.Code = " + flowTableAlias + "." + flowFieldName + "))";
                    whereStatement += " and exists (select 1 from UserPermissionView as up2,FlowMaster as fm2 where (fm2.IsCheckPartyToAuthority = 0) or (up2.UserId =" + user.Id + " and  up2.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ")  and up2.PermissionCode = fm2.PartyTo and fm2.Code = " + flowTableAlias + "." + flowFieldName + "))";
                }
                else
                {
                    whereStatement += " and exists (select 1 from UserPermissionView as up1,FlowMaster as fm1 where (fm1.IsCheckPartyFromAuthority = 0) or (up1.UserId =" + user.Id + " and  up1.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ")  and up1.PermissionCode = fm1.PartyFrom and fm1.Code = " + flowTableAlias + "." + flowFieldName + "))";
                    whereStatement += " and exists (select 1 from UserPermissionView as up2,FlowMaster as fm2 where (fm2.IsCheckPartyToAuthority = 0) or (up2.UserId =" + user.Id + " and  up2.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ")  and up2.PermissionCode = fm2.PartyTo and fm2.Code = " + flowTableAlias + "." + flowFieldName + "))";
                }
            }
        }

        public static string CheckFlowStatement(string flow, com.Sconit.CodeMaster.OrderType type)
        {
            return CheckFlowStatement(flow, type, false);
        }

        public static string CheckFlowStatement(string flow, bool isCreateHu)
        {
            return CheckFlowStatement(flow, null, isCreateHu);
        }

        public static string CheckFlowStatement(string flow, com.Sconit.CodeMaster.OrderType? type, bool isCreateHu)
        {
            string hql = "select f from FlowMaster as f where f.Code = '" + flow.Trim() + "' and f.IsActive = " + true;
            if (isCreateHu)
            {
                hql += " and Type in (" + (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.Production + ")";
            }
            else if (type != null)
            {
                if (type == com.Sconit.CodeMaster.OrderType.Procurement)
                {
                    hql += " and Type in (" + (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + "," + (int)com.Sconit.CodeMaster.OrderType.SubContractTransfer + "," + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + ")";
                }
                else if (type == com.Sconit.CodeMaster.OrderType.Distribution)
                {
                    hql += " and Type in (" + (int)com.Sconit.CodeMaster.OrderType.Distribution + "," + (int)com.Sconit.CodeMaster.OrderType.Transfer + ")";
                }
                else if (type == com.Sconit.CodeMaster.OrderType.Production)
                {
                    hql += " and Type = " + (int)com.Sconit.CodeMaster.OrderType.Production;
                }
            }
            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() != "su")
            {
                hql += " and ((f.IsCheckPartyFromAuthority = 0) or exists(select 1 from UserPermissionView up1 where up1.UserId =" + user.Id + " and  up1.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ")  and up1.PermissionCode = f.PartyFrom))";
                hql += " and ((f.IsCheckPartyToAuthority = 0) or exists(select 1 from UserPermissionView up2 where up2.UserId =" + user.Id + " and  up2.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ")  and up2.PermissionCode = f.PartyTo))";
            }
            return hql;
        }

        public static string CheckOrderStatement(string orderNo, com.Sconit.CodeMaster.OrderType? type)
        {
            return CheckOrderStatement(orderNo, type, false);
        }

        public static string CheckOrderStatement(string orderNo, bool isCreateHu)
        {
            return CheckOrderStatement(orderNo, null, isCreateHu);
        }

        public static string CheckOrderStatement(string orderNo, com.Sconit.CodeMaster.OrderType? type, bool isCreateHu)
        {
            string hql = "select o from OrderMaster as o where o.OrderNo = '" + orderNo.Trim() + "'";

            if (type != null)
            {
                hql += " and o.Type = " + (int)type;
            }
            else if (isCreateHu)
            {
                hql += " and o.Type in (" + (int)com.Sconit.CodeMaster.OrderType.Procurement + "," + (int)com.Sconit.CodeMaster.OrderType.CustomerGoods + "," + (int)com.Sconit.CodeMaster.OrderType.SubContract + "," + (int)com.Sconit.CodeMaster.OrderType.Production + ")";
            }
            User user = SecurityContextHolder.Get();
            if (user.Code.Trim().ToLower() != "su")
            {
                hql += " and (exists(select 1 from UserPermissionView up1 where up1.UserId =" + user.Id + " and  up1.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ")  and up1.PermissionCode = o.PartyFrom))";
                hql += " and (exists(select 1 from UserPermissionView up2 where up2.UserId =" + user.Id + " and  up2.PermissionCategoryType in (" + (int)com.Sconit.CodeMaster.PermissionCategoryType.Region + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Customer + "," + (int)com.Sconit.CodeMaster.PermissionCategoryType.Supplier + ")  and up2.PermissionCode = o.PartyTo))";
            }
            return hql;
        }
    }

}
