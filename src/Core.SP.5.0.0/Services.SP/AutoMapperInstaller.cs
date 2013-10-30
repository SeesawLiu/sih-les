using AutoMapper;
using System;

namespace com.Sconit.Services.SP
{
    public class AutoMapperInstaller
    {
        public static void Install()
        {
            Mapper.CreateMap<com.Sconit.Entity.ORD.OrderMaster, com.Sconit.Entity.SP.ORD.OrderMaster>()
                .ForMember(d => d.Priority, o => o.MapFrom(s => (Int16)s.Priority))
                .ForMember(d => d.Type, o => o.MapFrom(s => (Int16)s.Type))
                .ForMember(d => d.SubType, o => o.MapFrom(s => (Int16)s.SubType))
                .ForMember(d => d.Status, o => o.MapFrom(s => (Int16)s.Status))
                .ForMember(d => d.QualityType, o => o.MapFrom(s => (Int16)s.QualityType))
                .ForMember(d => d.OrderStrategy, o => o.MapFrom(s => (Int16)s.OrderStrategy));

            Mapper.CreateMap<com.Sconit.Entity.ORD.OrderDetail, com.Sconit.Entity.SP.ORD.OrderDetail>()
                .ForMember(d => d.QualityType, o => o.MapFrom(s => (Int16)s.QualityType));

            Mapper.CreateMap<com.Sconit.Entity.ORD.IpMaster, com.Sconit.Entity.SP.ORD.IpMaster>();

            Mapper.CreateMap<com.Sconit.Entity.ORD.IpDetail, com.Sconit.Entity.SP.ORD.IpDetail>();

            Mapper.CreateMap<com.Sconit.Entity.ORD.PickListMaster, com.Sconit.Entity.SP.ORD.PickListMaster>();

            Mapper.CreateMap<com.Sconit.Entity.ORD.PickListDetail, com.Sconit.Entity.SP.ORD.PickListDetail>();

            Mapper.CreateMap<com.Sconit.Entity.ORD.ReceiptMaster, com.Sconit.Entity.SP.ORD.ReceiptMaster>();

            Mapper.CreateMap<com.Sconit.Entity.ORD.ReceiptDetail, com.Sconit.Entity.SP.ORD.ReceiptDetail>();
        }
    }
}
