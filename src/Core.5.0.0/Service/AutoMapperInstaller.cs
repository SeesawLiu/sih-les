using System;
using AutoMapper;
using Castle.MicroKernel.Registration;
using com.Sconit.Entity.BIL;
using com.Sconit.Entity.INP;
using com.Sconit.Entity.INV;
using com.Sconit.Entity.ORD;
using com.Sconit.Entity.SCM;
using com.Sconit.Entity.VIEW;
using com.Sconit.PrintModel.ORD;
using Castle.Windsor;
using Castle.MicroKernel.SubSystems.Configuration;
using com.Sconit.PrintModel.INV;
using com.Sconit.PrintModel.INP;
using com.Sconit.Entity.KB;

namespace com.Sconit.Service
{
    public class AutoMapperInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            Mapper.CreateMap<OrderMaster, OrderMaster>()
                    .ForMember(d => d.OrderNo, o => o.Ignore())
                    .ForMember(d => d.Type, o => o.Ignore())
                    .ForMember(d => d.SubType, o => o.Ignore())
                    .ForMember(d => d.Status, o => o.Ignore())
                    .ForMember(d => d.Version, o => o.Ignore())
                    .ForMember(d => d.CreateDate, o => o.Ignore())
                    .ForMember(d => d.CreateUserId, o => o.Ignore())
                    .ForMember(d => d.CreateUserName, o => o.Ignore())
                    .ForMember(d => d.LastModifyDate, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                    .ForMember(d => d.OrderDetails, o => o.Ignore())
                    .ForMember(d => d.OrderBindings, o => o.Ignore());

            Mapper.CreateMap<OrderDetail, OrderDetail>()
                    .ForMember(d => d.Id, o => o.Ignore())
                    .ForMember(d => d.OrderNo, o => o.Ignore())
                //.ForMember(d => d.Item, o => o.Ignore())  零件也可能改
                //.ForMember(d => d.UnitQty, o => o.Ignore())
                    .ForMember(d => d.CreateDate, o => o.Ignore())
                    .ForMember(d => d.CreateUserId, o => o.Ignore())
                    .ForMember(d => d.CreateUserName, o => o.Ignore())
                    .ForMember(d => d.LastModifyDate, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserName, o => o.Ignore());

            Mapper.CreateMap<FlowDetail, FlowDetail>();

            Mapper.CreateMap<FlowMaster, FlowMaster>();

            Mapper.CreateMap<FlowStrategy, FlowStrategy>().ForMember(d => d.Flow, o => o.Ignore());

            Mapper.CreateMap<FlowDetail, OrderDetail>()
                    .ForMember(d => d.Id, o => o.Ignore())
                    .ForMember(d => d.CreateDate, o => o.Ignore())
                    .ForMember(d => d.CreateUserId, o => o.Ignore())
                    .ForMember(d => d.CreateUserName, o => o.Ignore())
                    .ForMember(d => d.LastModifyDate, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                    .ForMember(d => d.ExternalOrderNo, o => o.MapFrom(s => s.EBELN))
                    .ForMember(d => d.ExternalSequence, o => o.MapFrom(s => s.EBELP));

            Mapper.CreateMap<FlowMaster, OrderDetail>()
                  .ForMember(d => d.CreateDate, o => o.Ignore())
                  .ForMember(d => d.CreateUserId, o => o.Ignore())
                  .ForMember(d => d.CreateUserName, o => o.Ignore())
                  .ForMember(d => d.LastModifyDate, o => o.Ignore())
                  .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                  .ForMember(d => d.LastModifyUserName, o => o.Ignore());

            Mapper.CreateMap<FlowMaster, OrderMaster>()
                 .ForMember(d => d.Flow, o => o.MapFrom(ord => ord.Code))
                 .ForMember(d => d.FlowDescription, o => o.MapFrom(ord => ord.Description))
                 .ForMember(d => d.OrderStrategy, o => o.MapFrom(ord => ord.FlowStrategy))
                 .ForMember(d => d.CreateDate, o => o.Ignore())
                 .ForMember(d => d.CreateUserId, o => o.Ignore())
                 .ForMember(d => d.CreateUserName, o => o.Ignore())
                 .ForMember(d => d.LastModifyDate, o => o.Ignore())
                 .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                 .ForMember(d => d.LastModifyUserName, o => o.Ignore());

            Mapper.CreateMap<FlowMaster, IpMaster>()
                 .ForMember(d => d.Flow, o => o.MapFrom(ord => ord.Code))
                 .ForMember(d => d.CreateDate, o => o.Ignore())
                 .ForMember(d => d.CreateUserId, o => o.Ignore())
                 .ForMember(d => d.CreateUserName, o => o.Ignore())
                 .ForMember(d => d.LastModifyDate, o => o.Ignore())
                 .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                 .ForMember(d => d.LastModifyUserName, o => o.Ignore());

            Mapper.CreateMap<FlowDetail, IpDetail>()
                   .ForMember(d => d.Id, o => o.Ignore())
                   .ForMember(d => d.Type, o => o.Ignore())
                   .ForMember(d => d.OrderDetailId, o => o.MapFrom(ord => ord.Id))
                   .ForMember(d => d.OrderDetailSequence, o => o.MapFrom(ord => ord.Sequence))
                   .ForMember(d => d.Qty, o => o.Ignore())
                //.ForMember(d => d.ReceiveQty, o => o.Ignore())
                   .ForMember(d => d.ReceivedQty, o => o.Ignore())
                   .ForMember(d => d.CreateDate, o => o.Ignore())
                   .ForMember(d => d.CreateUserId, o => o.Ignore())
                   .ForMember(d => d.CreateUserName, o => o.Ignore())
                   .ForMember(d => d.LastModifyDate, o => o.Ignore())
                   .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                   .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                   .ForMember(d => d.Version, o => o.Ignore())
                   .ForMember(d => d.ManufactureParty, o => o.MapFrom(ord => ord.ManufactureParty));

            Mapper.CreateMap<OrderDetail, IpDetail>()
                    .ForMember(d => d.Id, o => o.Ignore())
                    .ForMember(d => d.Type, o => o.Ignore())
                    .ForMember(d => d.OrderDetailId, o => o.MapFrom(ord => ord.Id))
                    .ForMember(d => d.OrderDetailSequence, o => o.MapFrom(ord => ord.Sequence))
                    .ForMember(d => d.Qty, o => o.Ignore())
                //.ForMember(d => d.ReceiveQty, o => o.Ignore())
                    .ForMember(d => d.ReceivedQty, o => o.Ignore())
                    .ForMember(d => d.CreateDate, o => o.Ignore())
                    .ForMember(d => d.CreateUserId, o => o.Ignore())
                    .ForMember(d => d.CreateUserName, o => o.Ignore())
                    .ForMember(d => d.LastModifyDate, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                    .ForMember(d => d.Version, o => o.Ignore())
                    .ForMember(d => d.ManufactureParty, o => o.MapFrom(ord => ord.ManufactureParty))
                //.ForMember(d => d.Item, o => o.MapFrom(ord => ord.Item))
                //.ForMember(d => d.ItemDescription, o => o.MapFrom(ord => ord.ItemDescription))
                //.ForMember(d => d.ReferenceItemCode, o => o.MapFrom(ord => ord.ReferenceItemCode))
                //.ForMember(d => d.Uom, o => o.MapFrom(ord => ord.Uom))
                //.ForMember(d => d.UnitCount, o => o.MapFrom(ord => ord.UnitCount))
                //.ForMember(d => d.QualityType, o => o.MapFrom(ord => ord.QualityType))
                //.ForMember(d => d.UnitQty, o => o.MapFrom(ord => ord.UnitQty))
                //.ForMember(d => d.LocationFrom, o => o.MapFrom(ord => ord.LocationFrom))
                //.ForMember(d => d.LocationFromName, o => o.MapFrom(ord => ord.LocationFromName))
                //.ForMember(d => d.LocationTo, o => o.MapFrom(ord => ord.LocationTo))
                //.ForMember(d => d.LocationToName, o => o.MapFrom(ord => ord.LocationToName))
                //.ForMember(d => d.IsInspect, o => o.MapFrom(ord => ord.IsInspect))
                //.ForMember(d => d.BillAddress, o => o.MapFrom(ord => ord.BillAddress))
                //.ForMember(d => d.PriceList, o => o.MapFrom(ord => ord.PriceList))
                //.ForMember(d => d.UnitPrice, o => o.MapFrom(ord => ord.UnitPrice))
                //.ForMember(d => d.Currency, o => o.MapFrom(ord => ord.Currency))
                //.ForMember(d => d.IsProvisionalEstimate, o => o.MapFrom(ord => ord.IsProvisionalEstimate))
                //.ForMember(d => d.Tax, o => o.MapFrom(ord => ord.Tax))
                //.ForMember(d => d.IsIncludeTax, o => o.MapFrom(ord => ord.IsIncludeTax))
                //.ForMember(d => d.BillTerm, o => o.MapFrom(ord => ord.BillTerm))
            ;

            Mapper.CreateMap<OrderMaster, ReceiptMaster>()
                    .ForMember(d => d.Type, o => o.Ignore())
                    .ForMember(d => d.OrderType, o => o.MapFrom(om => om.Type))
                    .ForMember(d => d.OrderSubType, o => o.MapFrom(om => om.SubType))
                    .ForMember(d => d.EffectiveDate, o => o.Ignore())
                    .ForMember(d => d.CreateDate, o => o.Ignore())
                    .ForMember(d => d.CreateUserId, o => o.Ignore())
                    .ForMember(d => d.CreateUserName, o => o.Ignore())
                    .ForMember(d => d.LastModifyDate, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                    .ForMember(d => d.Version, o => o.Ignore());

            Mapper.CreateMap<OrderDetail, ReceiptDetail>()
                    .ForMember(d => d.Id, o => o.Ignore())
                   .ForMember(d => d.OrderDetailId, o => o.MapFrom(ord => ord.Id))
                    .ForMember(d => d.OrderDetailSequence, o => o.MapFrom(ord => ord.Sequence))
                //.ForMember(d => d.RejectedQty, o => o.Ignore())
                    .ForMember(d => d.ReceivedQty, o => o.Ignore())
                //.ForMember(d => d.ScrapQty, o => o.Ignore())
                    .ForMember(d => d.CreateDate, o => o.Ignore())
                    .ForMember(d => d.CreateUserId, o => o.Ignore())
                    .ForMember(d => d.CreateUserName, o => o.Ignore())
                    .ForMember(d => d.LastModifyDate, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                    .ForMember(d => d.Version, o => o.Ignore());

            Mapper.CreateMap<IpMaster, ReceiptMaster>()
                    .ForMember(d => d.Type, o => o.Ignore())
                    .ForMember(d => d.EffectiveDate, o => o.Ignore())
                    .ForMember(d => d.CreateDate, o => o.Ignore())
                    .ForMember(d => d.CreateUserId, o => o.Ignore())
                    .ForMember(d => d.CreateUserName, o => o.Ignore())
                    .ForMember(d => d.LastModifyDate, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                    .ForMember(d => d.Version, o => o.Ignore());

            Mapper.CreateMap<IpDetail, ReceiptDetail>()
                    .ForMember(d => d.Id, o => o.Ignore())
                    .ForMember(d => d.CreateDate, o => o.Ignore())
                    .ForMember(d => d.CreateUserId, o => o.Ignore())
                    .ForMember(d => d.CreateUserName, o => o.Ignore())
                    .ForMember(d => d.LastModifyDate, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                    .ForMember(d => d.Version, o => o.Ignore())
                    .ForMember(d => d.IpNo, o => o.Ignore())
                    .ForMember(d => d.ReceivedQty, o => o.Ignore());

            Mapper.CreateMap<IpDetail, IpDetail>()
                    .ForMember(d => d.Id, o => o.Ignore())
                    .ForMember(d => d.Qty, o => o.Ignore())
                    .ForMember(d => d.ReceivedQty, o => o.Ignore())
                    .ForMember(d => d.CreateDate, o => o.Ignore())
                    .ForMember(d => d.CreateUserId, o => o.Ignore())
                    .ForMember(d => d.CreateUserName, o => o.Ignore())
                    .ForMember(d => d.LastModifyDate, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                    .ForMember(d => d.Version, o => o.Ignore());

            Mapper.CreateMap<ReceiptMaster, IpMaster>()
                   .ForMember(d => d.EffectiveDate, o => o.Ignore())
                   .ForMember(d => d.CreateDate, o => o.Ignore())
                   .ForMember(d => d.CreateUserId, o => o.Ignore())
                   .ForMember(d => d.CreateUserName, o => o.Ignore())
                   .ForMember(d => d.LastModifyDate, o => o.Ignore())
                   .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                   .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                   .ForMember(d => d.Version, o => o.Ignore());

            Mapper.CreateMap<ReceiptDetail, IpDetail>()
                    .ForMember(d => d.Id, o => o.Ignore())
                    .ForMember(d => d.CreateDate, o => o.Ignore())
                    .ForMember(d => d.CreateUserId, o => o.Ignore())
                    .ForMember(d => d.CreateUserName, o => o.Ignore())
                    .ForMember(d => d.LastModifyDate, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                    .ForMember(d => d.Version, o => o.Ignore())
                    .ForMember(d => d.IpNo, o => o.Ignore())
                    .ForMember(d => d.ReceivedQty, o => o.Ignore());

            Mapper.CreateMap<InventoryIO, LocationLotDetail>();

            Mapper.CreateMap<LocationLotDetail, LocationLotDetail>()
                    .ForMember(d => d.Id, o => o.Ignore());

            Mapper.CreateMap<PlanBill, ActingBill>()
                    .ForMember(d => d.Id, o => o.Ignore())
                    .ForMember(d => d.PlanBill, o => o.MapFrom(pb => pb.Id))
                    .ForMember(d => d.VoidAmount, o => o.Ignore())
                    .ForMember(d => d.VoidQty, o => o.Ignore())
                    .ForMember(d => d.CreateDate, o => o.Ignore())
                    .ForMember(d => d.CreateUserId, o => o.Ignore())
                    .ForMember(d => d.CreateUserName, o => o.Ignore())
                    .ForMember(d => d.LastModifyDate, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                    .ForMember(d => d.Version, o => o.Ignore())
                    .ForMember(d => d.EffectiveDate, o => o.Ignore());

            Mapper.CreateMap<LocationTransaction, LocationTransactionDetail>()
                   .ForMember(d => d.LocationTransactionId, o => o.MapFrom(lt => lt.Id))
                   .ForMember(d => d.Id, o => o.Ignore())
                   .ForMember(d => d.Qty, o => o.Ignore())
                   .ForMember(d => d.ActingBillQty, o => o.Ignore());

            Mapper.CreateMap<LocationTransaction, LocationTransaction>();

            Mapper.CreateMap<InventoryTransaction, InventoryTransaction>();

            Mapper.CreateMap<PickLocationDetail, PickLocationDetail>();

            Mapper.CreateMap<PickListMaster, IpMaster>()
                    .ForMember(d => d.Status, o => o.Ignore())
                    .ForMember(d => d.DepartTime, o => o.MapFrom(pl => pl.StartTime))
                    .ForMember(d => d.ArriveTime, o => o.MapFrom(pl => pl.WindowTime))
                    .ForMember(d => d.EffectiveDate, o => o.Ignore())
                    .ForMember(d => d.CreateDate, o => o.Ignore())
                    .ForMember(d => d.CreateUserId, o => o.Ignore())
                    .ForMember(d => d.CreateUserName, o => o.Ignore())
                    .ForMember(d => d.LastModifyDate, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                    .ForMember(d => d.CloseDate, o => o.Ignore())
                    .ForMember(d => d.CloseUserId, o => o.Ignore())
                    .ForMember(d => d.CloseUserName, o => o.Ignore())
                    .ForMember(d => d.CloseReason, o => o.Ignore())
                    .ForMember(d => d.Version, o => o.Ignore())
                    .ForMember(d => d.Type, o => o.Ignore());

            Mapper.CreateMap<PickListMaster, ReceiptMaster>()
                    .ForMember(d => d.Status, o => o.Ignore())
                    .ForMember(d => d.EffectiveDate, o => o.Ignore())
                    .ForMember(d => d.CreateDate, o => o.Ignore())
                    .ForMember(d => d.CreateUserId, o => o.Ignore())
                    .ForMember(d => d.CreateUserName, o => o.Ignore())
                    .ForMember(d => d.LastModifyDate, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                    .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                    .ForMember(d => d.Version, o => o.Ignore())
                    .ForMember(d => d.Type, o => o.Ignore());

            Mapper.CreateMap<WorkingCalendarView, WorkingCalendarView>();

            Mapper.CreateMap<InspectResult, RejectDetail>()
                    .ForMember(d => d.InspectResultId, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.JudgeDate, o => o.MapFrom(s => s.CreateDate))
                    .ForMember(d => d.JudgeUserId, o => o.MapFrom(s => s.CreateUserId))
                    .ForMember(d => d.JudgeUserName, o => o.MapFrom(s => s.CreateUserName))
                   .ForMember(d => d.CreateDate, o => o.Ignore())
                   .ForMember(d => d.CreateDate, o => o.Ignore())
                   .ForMember(d => d.CreateUserId, o => o.Ignore())
                   .ForMember(d => d.CreateUserName, o => o.Ignore())
                   .ForMember(d => d.LastModifyDate, o => o.Ignore())
                   .ForMember(d => d.LastModifyUserId, o => o.Ignore())
                   .ForMember(d => d.LastModifyUserName, o => o.Ignore())
                   .ForMember(d => d.Version, o => o.Ignore());

            Mapper.CreateMap<Hu, HuStatus>();

            Mapper.CreateMap<OrderMaster, PrintOrderMaster>()
                .ForMember(d => d.Priority, o => o.MapFrom(s => (Int16)s.Priority))
                .ForMember(d => d.Type, o => o.MapFrom(s => (Int16)s.Type))
                .ForMember(d => d.SubType, o => o.MapFrom(s => (Int16)s.SubType))
                .ForMember(d => d.Status, o => o.MapFrom(s => (Int16)s.Status))
                .ForMember(d => d.QualityType, o => o.MapFrom(s => (Int16)s.QualityType))
                .ForMember(d => d.OrderStrategy, o => o.MapFrom(s => (Int16)s.OrderStrategy));

            Mapper.CreateMap<OrderDetail, PrintOrderDetail>()
                .ForMember(d => d.QualityType, o => o.MapFrom(s => (Int16)s.QualityType));

            Mapper.CreateMap<IpMaster, PrintIpMaster>()
                .ForMember(d => d.OrderType, o => o.MapFrom(s => (Int16)s.OrderType))
                .ForMember(d => d.OrderSubType, o => o.MapFrom(s => (Int16)s.OrderSubType))
                .ForMember(d => d.QualityType, o => o.MapFrom(s => (Int16)s.QualityType));

            Mapper.CreateMap<IpDetail, PrintIpDetail>()
                .ForMember(d => d.QualityType, o => o.MapFrom(s => (Int16)s.QualityType));

            Mapper.CreateMap<PickListMaster, PrintPickListMaster>()
                .ForMember(d => d.OrderType, o => o.MapFrom(s => (Int16)s.OrderType))
                .ForMember(d => d.QualityType, o => o.MapFrom(s => (Int16)s.QualityType));

            Mapper.CreateMap<PickListDetail, PrintPickListDetail>();

            Mapper.CreateMap<com.Sconit.Entity.KB.KanbanCard, PrintKanBanCard>();

            Mapper.CreateMap<KanBanCardInfo, PrintKanBanCardInfo>();

            Mapper.CreateMap<ReceiptMaster, PrintReceiptMaster>()
                .ForMember(d => d.OrderType, o => o.MapFrom(s => (Int16)s.OrderType))
                .ForMember(d => d.OrderSubType, o => o.MapFrom(s => (Int16)s.OrderSubType))
                .ForMember(d => d.QualityType, o => o.MapFrom(s => (Int16)s.QualityType));

            Mapper.CreateMap<Hu, PrintHu>();

            Mapper.CreateMap<OrderBomDetail, PrintOrderBomDetail>()
                .ForMember(d => d.BackFlushMethod, o => o.MapFrom(s => (Int16)s.BackFlushMethod))
                .ForMember(d => d.FeedMethod, o => o.MapFrom(s => (Int16)s.FeedMethod))
                .ForMember(d => d.OrderSubType, o => o.MapFrom(s => (Int16)s.OrderSubType))
                .ForMember(d => d.OrderType, o => o.MapFrom(s => (Int16)s.OrderType));

            Mapper.CreateMap<ReceiptDetail, PrintReceiptDetail>()
                .ForMember(d => d.QualityType, o => o.MapFrom(s => (Int16)s.QualityType));

            Mapper.CreateMap<StockTakeMaster, PrintStockTakeMaster>()
                .ForMember(d => d.Type, o => o.MapFrom(s => (Int16)s.Type));

            Mapper.CreateMap<MiscOrderMaster, PrintMiscOrderMaster>()
                .ForMember(d => d.QualityType, o => o.MapFrom(s => (Int16)s.QualityType))
                .ForMember(d => d.Type, o => o.MapFrom(s => (Int16)s.Type))
                .ForMember(d => d.Status, o => o.MapFrom(s => (Int16)s.Status))
                ;

            Mapper.CreateMap<MiscOrderDetail, PrintMiscOrderDetail>();

            Mapper.CreateMap<InspectMaster, PrintInspectMaster>();

            Mapper.CreateMap<InspectDetail, PrintInspectDetail>();

            Mapper.CreateMap<RejectMaster, PrintRejectMaster>();

            Mapper.CreateMap<RejectDetail, PrintRejectDetail>();

            Mapper.CreateMap<RejectDetail, ConcessionDetail>()
             .ForMember(d => d.LocationFrom, o => o.MapFrom(rej => rej.CurrentLocation))
             .ForMember(d => d.Qty, o => o.MapFrom(rej => rej.HandleQty))
             .ForMember(d => d.Id, o => o.Ignore())
             .ForMember(d => d.CreateDate, o => o.Ignore())
             .ForMember(d => d.CreateUserId, o => o.Ignore())
             .ForMember(d => d.CreateUserName, o => o.Ignore())
             .ForMember(d => d.LastModifyDate, o => o.Ignore())
             .ForMember(d => d.LastModifyUserId, o => o.Ignore())
             .ForMember(d => d.LastModifyUserName, o => o.Ignore());


            //试制车用的通过orderbomdet反向生成orderdetail
            Mapper.CreateMap<OrderBomDetail, OrderDetail>()
             .ForMember(d => d.Id, o => o.Ignore())
             .ForMember(d => d.OrderNo, o => o.Ignore())
             .ForMember(d => d.CreateDate, o => o.Ignore())
             .ForMember(d => d.CreateUserId, o => o.Ignore())
             .ForMember(d => d.CreateUserName, o => o.Ignore())
             .ForMember(d => d.LastModifyDate, o => o.Ignore())
             .ForMember(d => d.LastModifyUserId, o => o.Ignore())
             .ForMember(d => d.LastModifyUserName, o => o.Ignore());

            Mapper.CreateMap<IpLocationDetail, IpLocationDetail>();

            Mapper.CreateMap<ReceiptDetailInput, ReceiptDetailInput>();

            //待验明细移库用的通过inspectdetail反向生成orderdetail
            Mapper.CreateMap<InspectDetail, OrderDetail>()
             .ForMember(d => d.Id, o => o.Ignore())
             .ForMember(d => d.CreateDate, o => o.Ignore())
             .ForMember(d => d.CreateUserId, o => o.Ignore())
             .ForMember(d => d.CreateUserName, o => o.Ignore())
             .ForMember(d => d.LastModifyDate, o => o.Ignore())
             .ForMember(d => d.LastModifyUserId, o => o.Ignore())
             .ForMember(d => d.LastModifyUserName, o => o.Ignore());

            //不合格品处理单明细移库用的通过rejectdetail反向生成orderdetail
            Mapper.CreateMap<RejectDetail, OrderDetail>()
             .ForMember(d => d.Id, o => o.Ignore())
             .ForMember(d => d.CreateDate, o => o.Ignore())
             .ForMember(d => d.CreateUserId, o => o.Ignore())
             .ForMember(d => d.CreateUserName, o => o.Ignore())
             .ForMember(d => d.LastModifyDate, o => o.Ignore())
             .ForMember(d => d.LastModifyUserId, o => o.Ignore())
             .ForMember(d => d.LastModifyUserName, o => o.Ignore());

            Mapper.CreateMap<KanbanCard, KanbanScan>()
             .ForMember(d => d.Id, o => o.Ignore())
             .ForMember(d => d.CreateUserId, o => o.Ignore())
             .ForMember(d => d.CreateDate, o => o.Ignore())
             .ForMember(d => d.CreateUserName, o => o.Ignore())
             .ForMember(d => d.LastModifyDate, o => o.Ignore())
             .ForMember(d => d.LastModifyUserId, o => o.Ignore())
             .ForMember(d => d.LastModifyUserName, o => o.Ignore());

            Mapper.CreateMap<KanbanCard, KanbanCard>();

            Mapper.CreateMap<KanbanCard, KanbanCalc>()
             .ForMember(d => d.Id, o => o.Ignore());

            Mapper.CreateMap<KanbanCalc, KanbanCard>()
             .ForMember(d => d.CreateUserId, o => o.Ignore())
             .ForMember(d => d.CreateDate, o => o.Ignore())
             .ForMember(d => d.CreateUserName, o => o.Ignore())
             .ForMember(d => d.LastModifyDate, o => o.Ignore())
             .ForMember(d => d.LastModifyUserId, o => o.Ignore())
             .ForMember(d => d.LastModifyUserName, o => o.Ignore());
        }
    }
}
