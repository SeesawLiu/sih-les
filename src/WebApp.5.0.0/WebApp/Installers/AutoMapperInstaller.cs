using System.Web.Mvc;
using AutoMapper;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using com.Sconit.Entity.SYS;
using com.Sconit.Web.Models;

/// <summary>
/// Summary description for AutoMapperInstaller
/// </summary>
namespace com.Sconit.Web.Installer
{
    public class AutoMapperInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            //Order
            Mapper.CreateMap<com.Sconit.Entity.SAP.TRANS.InvTrans, com.Sconit.Service.SAP.MI_LES.ZSMIGO>();
            Mapper.CreateMap<com.Sconit.Entity.SAP.TRANS.InvTrans, com.Sconit.Entity.SAP.TRANS.InvTrans>();
            Mapper.CreateMap<com.Sconit.Service.SAP.MI_LES.ZSMIGORT, com.Sconit.Entity.SAP.TRANS.TransCallBack>();

            //ProdOrder
            Mapper.CreateMap<com.Sconit.Service.SAP.MI_PO_HEAD_LES.ZSPOOUT, Entity.SAP.ORD.ProdOrder>();
            Mapper.CreateMap<com.Sconit.Service.SAP.MI_PO_BOM_LES.ZSPOBOM, Entity.SAP.ORD.ProdOrderBomDet>();

            //ProcOrder
            Mapper.CreateMap<com.Sconit.Service.SAP.MI_SL_OUT.ZSEKPTH, Entity.SAP.ORD.ProcOrder>();
            Mapper.CreateMap<com.Sconit.Service.SAP.MI_SL_OUT.ZSEKPTI, Entity.SAP.ORD.ProcOrderDetail>();

            Mapper.CreateMap<com.Sconit.Service.SAP.MI_POIF_LES.ZSPOOUT, com.Sconit.Service.SAP.MI_PO_HEAD_LES.ZSPOOUT>();
            Mapper.CreateMap<com.Sconit.Service.SAP.MI_POIF_LES.ZSPOBOM, com.Sconit.Service.SAP.MI_PO_BOM_LES.ZSPOBOM>();

            Mapper.CreateMap<com.Sconit.Entity.ORD.OrderDetail, com.Sconit.Entity.SAP.ORD.OrderDetail>();

            Mapper.CreateMap<com.Sconit.Entity.SAP.MD.Item, com.Sconit.Entity.SAP.MD.SAPItem>();
            
            Mapper.CreateMap<Menu, MenuModel>();

            Mapper.CreateMap<CodeDetail, SelectListItem>()
                .ForMember(d => d.Value, opt => opt.MapFrom(s => s.Value))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Description));

            Mapper.CreateMap<com.Sconit.Entity.ORD.OrderDetail, com.Sconit.Entity.LOG.SeqOrderChange>();
            Mapper.CreateMap<com.Sconit.Entity.ORD.OrderDetail, com.Sconit.Entity.LOG.DistributionRequisition>()
                .ForMember(d => d.OrderDetId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Flow, opt => opt.MapFrom(s => s.Flow))
                .ForMember(d => d.FlowDescription, opt => opt.MapFrom(s => s.FlowDescription))
                .ForMember(d => d.PartyFrom, opt => opt.MapFrom(s => s.MastPartyFrom))
                .ForMember(d => d.PartyTo, opt => opt.MapFrom(s => s.MastPartyTo));
        }
    }
}