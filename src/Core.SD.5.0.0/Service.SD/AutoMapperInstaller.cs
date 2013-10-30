namespace com.Sconit.Service.SD
{
    using AutoMapper;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    public class AutoMapperInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            //Order
            Mapper.CreateMap<com.Sconit.Entity.ORD.OrderMaster, com.Sconit.Entity.SD.ORD.OrderMaster>();
            Mapper.CreateMap<com.Sconit.Entity.ORD.OrderDetail, com.Sconit.Entity.SD.ORD.OrderDetail>()
                .ForMember(d => d.RemainReceivedQty, o => o.MapFrom(s => s.OrderedQty - s.ReceivedQty))
                 .ForMember(d => d.CurrentQty, o => o.MapFrom(s => s.OrderedQty - s.ReceivedQty));
            Mapper.CreateMap<com.Sconit.Entity.ORD.IpMaster, com.Sconit.Entity.SD.ORD.IpMaster>();
            Mapper.CreateMap<com.Sconit.Entity.ORD.IpDetail, com.Sconit.Entity.SD.ORD.IpDetail>()
                .ForMember(d => d.RemainReceivedQty, o => o.MapFrom(s => s.Qty - s.ReceivedQty))
                .ForMember(d => d.CurrentQty, o => o.MapFrom(s => s.Qty - s.ReceivedQty));

            Mapper.CreateMap<com.Sconit.Entity.ORD.PickListMaster, com.Sconit.Entity.SD.ORD.PickListMaster>();
            Mapper.CreateMap<com.Sconit.Entity.ORD.PickListDetail, com.Sconit.Entity.SD.ORD.PickListDetail>()
                .ForMember(d => d.CurrentQty, o => o.MapFrom(s => s.Qty - s.PickedQty));
            Mapper.CreateMap<com.Sconit.Entity.INP.InspectMaster, com.Sconit.Entity.SD.ORD.InspectMaster>();
            Mapper.CreateMap<com.Sconit.Entity.INP.InspectDetail, com.Sconit.Entity.SD.ORD.InspectDetail>();
            Mapper.CreateMap<com.Sconit.Entity.ORD.MiscOrderMaster, com.Sconit.Entity.SD.ORD.MiscOrderMaster>();
            Mapper.CreateMap<com.Sconit.Entity.ORD.MiscOrderDetail, com.Sconit.Entity.SD.ORD.MiscOrderDetail>();
            //Inv
            Mapper.CreateMap<com.Sconit.Entity.VIEW.HuStatus, com.Sconit.Entity.SD.INV.Hu>();
            Mapper.CreateMap<com.Sconit.Entity.INV.Hu, com.Sconit.Entity.SD.INV.Hu>();
            Mapper.CreateMap<com.Sconit.Entity.INV.StockTakeMaster, com.Sconit.Entity.SD.INV.StockTakeMaster>();
            Mapper.CreateMap<com.Sconit.Entity.KB.KanbanCard, com.Sconit.Entity.SD.ORD.AnDonInput>();

            //ACC
            Mapper.CreateMap<com.Sconit.Entity.ACC.User, com.Sconit.Entity.SD.ACC.User>()
            .ForMember(d => d.BarCodeTypes, s => s.Ignore());

            Mapper.CreateMap<com.Sconit.Entity.VIEW.UserPermissionView, com.Sconit.Entity.SD.ACC.Permission>();

            //MD
            Mapper.CreateMap<com.Sconit.Entity.MD.Location, com.Sconit.Entity.SD.MD.Location>();
            Mapper.CreateMap<com.Sconit.Entity.MD.LocationBin, com.Sconit.Entity.SD.MD.Bin>();
            Mapper.CreateMap<com.Sconit.Entity.MD.Item, com.Sconit.Entity.SD.MD.Item>();
            
            //SCM
            Mapper.CreateMap<com.Sconit.Entity.SCM.FlowMaster, com.Sconit.Entity.SD.SCM.FlowMaster>();
            Mapper.CreateMap<com.Sconit.Entity.SCM.FlowDetail, com.Sconit.Entity.SD.SCM.FlowDetail>();

            //PickTask
            Mapper.CreateMap<com.Sconit.Entity.ORD.PickTask, com.Sconit.Entity.SD.ORD.PickTask>();
        }
    }
}
