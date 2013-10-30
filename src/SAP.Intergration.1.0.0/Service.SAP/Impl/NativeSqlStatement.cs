using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Sconit.Service.SAP.Impl
{
    public class NativeSqlStatement
    {
        public static string SELECT_VAN_OPERATION_REPORT_STATEMENT = @"select rep.* from CUST_OpReport as rep WITH(NOLOCK)
                                                                        inner join 
                                                                        (select mstr.OrderNo, mstr.Status, Max(Op.Op) as MaxOp from ORD_OrderOp as op WITH(NOLOCK)                                                                       
                                                                        inner join ORD_OrderMstr_4 as mstr WITH(NOLOCK) on op.OrderNo = mstr.OrderNo
                                                                        inner join CUST_ProductLineMap as map WITH(NOLOCK) on mstr.Flow = map.ProdLine and isnull(map.CabFlow, '') <> '' or (mstr.Flow = map.CabFlow) or (mstr.Flow = map.ChaFlow)
                                                                        --执行中、完工和关闭的订单才能报工
                                                                        where mstr.TraceCode is not null and mstr.Status in (?, ?, ?) and op.IsBackflush = ?
                                                                        group by mstr.OrderNo, mstr.Status) as a on rep.OrderNo = a.OrderNo
                                                                        where (a.MaxOp >= rep.Op or a.Status in (?, ?)) and rep.IsReport = ?";

        public static string SELECT_PRD_OPERATION_REPORT_STATEMENT = @"select distinct ordMstr.ExtOrderNo, op.Op, op.WorkCenter, recDet.RecQty, recDet.ScrapQty, recDet.Id from ORD_RecDet_4 as recDet WITH(NOLOCK) 
                                                                        inner join ORD_RecMstr_4 as recMstr WITH(NOLOCK) on recDet.RecNo = recMstr.RecNo
                                                                        inner join ORD_OrderDet_4 as ordDet WITH(NOLOCK) on recDet.OrderDetId = ordDet.Id 
                                                                        inner join ORD_OrderOp as op WITH(NOLOCK) on op.OrderDetId = ordDet.Id
                                                                        inner join ORD_OrderMstr_4 as ordMstr WITH(NOLOCK) on ordDet.OrderNo = ordMstr.OrderNo
                                                                        --非整车生产线，CabFlow为空
                                                                        inner join CUST_ProductLineMap as map WITH(NOLOCK) on ordMstr.Flow = map.ProdLine and isnull(map.CabFlow, '') = ''
                                                                        where recMstr.Status = ? and recDet.Id > ? and op.IsReport = ?";

        public static string SELECT_PRD_OPERATION_CANCEL_STATEMENT = @"select distinct ordMstr.ExtOrderNo, op.Op, op.WorkCenter, recDet.RecQty, recDet.ScrapQty, recDet.Id from ORD_RecDet_4 as recDet WITH(NOLOCK) 
                                                                        inner join ORD_RecMstr_4 as recMstr WITH(NOLOCK) on recDet.RecNo = recMstr.RecNo
                                                                        inner join ORD_OrderDet_4 as ordDet WITH(NOLOCK) on recDet.OrderDetId = ordDet.Id 
                                                                        inner join ORD_OrderOp as op WITH(NOLOCK) on op.OrderDetId = ordDet.Id
                                                                        inner join ORD_OrderMstr_4 as ordMstr WITH(NOLOCK) on ordDet.OrderNo = ordMstr.OrderNo
                                                                        --非整车生产线，CabFlow为空
                                                                        inner join CUST_ProductLineMap as map WITH(NOLOCK) on ordMstr.Flow = map.ProdLine and isnull(map.CabFlow, '') = ''
                                                                        where recMstr.Status = ? and recMstr.LastModifyDate > ? and recMstr.CreateDate < ? and op.IsReport = ?";

        #region 移动类型——物料回冲
        public static string SELECT_INPROCESS_CLOSED_PRODUCT_ORDER_STATEMENT = @"select om.OrderNo, om.CloseDate from ORD_OrderMstr_4 as om WITH(NOLOCK) where ((om.Status = ? and om.CloseDate > ?) or om.Status = ?) and om.Type = ? order by om.LastModifyDate asc";

        public static string SELECT_ORDER_BOM_DETAIL_STATEMENT = @"select OrderNo, Item, Uom, Location, ReserveNo, ReserveLine, AUFNR, ICHARG, BWART from ORD_OrderBomDet WITH(NOLOCK) where OrderNo in (?";

        public static string SELECT_ORDER_BACKFLUSH_DETAIL_STATEMENT = @"select Id, OrderNo, Item, Uom, -BFQty, LocFrom, ReserveNo, ReserveLine, AUFNR, PlanBill, EffDate, CreateDate, ICHARG, BWART from ORD_OrderBackflushDet WITH(NOLOCK) where Id > ? and NotReport = ?";

        public static string SELECT_PLANBILL_STATEMENT = "select Id, Party from BIL_PlanBill WITH(NOLOCK) where Id in (?";

        public static string SELECT_LOC_AND_PLANT_STATEMENT = @"select loc.Code, loc.SAPLocation, reg.Plant from MD_Location as loc inner join MD_Region as reg on loc.Region = reg.Code where loc.Code in (?";

        public static string SELECT_PO_EBELN_AND_EBELP_STATEMENT = @"select det.ExtNo, det.ExtSeq from ORD_OrderDet_1 as det WITH(NOLOCK) inner join ORD_OrderMstr_1 as mstr WITH(NOLOCK) on det.OrderNo = mstr.OrderNo where det.Id = ?";

        public static string SELECT_SL_EBELN_AND_EBELP_STATEMENT = @"select mstr.ExtOrderNo, det.ExtSeq from ORD_OrderDet_8 as det WITH(NOLOCK) inner join ORD_OrderMstr_8 as mstr WITH(NOLOCK) on det.OrderNo = mstr.OrderNo where det.Id = ?";

        public static string SELECT_PO_EBELN_STATEMENT = @"select mstr.ExtOrderNo from ORD_OrderMstr_1 as mstr WITH(NOLOCK) where mstr.OrderNo = ?";

        public static string SELECT_SL_EBELN_STATEMENT = @"select mstr.ExtOrderNo from ORD_OrderMstr_8 as mstr WITH(NOLOCK) where mstr.OrderNo = ?";
        #endregion
    }
}
