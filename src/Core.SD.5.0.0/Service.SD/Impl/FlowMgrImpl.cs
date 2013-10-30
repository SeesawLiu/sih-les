namespace com.Sconit.Service.SD.Impl
{
    using System.Collections.Generic;
    using com.Sconit.Entity.SCM;
    using NHibernate.Criterion;
    using com.Sconit.Entity.Exception;
    using Castle.Services.Transaction;
    using System;
    using AutoMapper;
    using System.Linq;

    public class FlowMgrImpl : com.Sconit.Service.SD.IFlowMgrImpl
    {
        #region 变量
        public IGenericMgr genericMgr { get; set; }
        public Service.IFlowMgr flowMgr { get; set; }
        #endregion

        public Entity.SD.SCM.FlowMaster GetFlowMaster(string flowCode,bool includeDetail)
        {
            var flowMaster = this.genericMgr.FindById<Entity.SCM.FlowMaster>(flowCode);
            var flow = Mapper.Map<Entity.SCM.FlowMaster, Entity.SD.SCM.FlowMaster>(flowMaster);
            if (includeDetail)
            {
                var flowDetails = this.flowMgr.GetFlowDetailList(flowCode, false, true);
                flow.FlowDetails = Mapper.Map<IList<Entity.SCM.FlowDetail>, List<Entity.SD.SCM.FlowDetail>>(flowDetails);

                foreach (var flowDetail in flow.FlowDetails)
                {
                    flowDetail.LocationFrom = string.IsNullOrWhiteSpace(flowDetail.LocationFrom) ? flow.LocationFrom : flowDetail.LocationFrom;
                    flowDetail.LocationTo = string.IsNullOrWhiteSpace(flowDetail.LocationTo) ? flow.LocationTo : flowDetail.LocationTo;
                }
            }
            return flow;
        }

        public Entity.SD.SCM.FlowMaster GetFlowMasterByFacility(string facilityCode, bool includeDetail)
        {
            var flowMaster = this.genericMgr.FindAll<Entity.SCM.FlowMaster>("from FlowMaster as fm where fm.Code in(select plf.ProductLine from ProductLineFacility as plf where plf.Code=?)", facilityCode);
            var flow = Mapper.Map<Entity.SCM.FlowMaster, Entity.SD.SCM.FlowMaster>(flowMaster[0]);
            if (includeDetail)
            {
                var flowDetails = this.flowMgr.GetFlowDetailList(flowMaster[0].Code, false, true);
                flow.FlowDetails = Mapper.Map<IList<Entity.SCM.FlowDetail>, List<Entity.SD.SCM.FlowDetail>>(flowDetails);

                foreach (var flowDetail in flow.FlowDetails)
                {
                    flowDetail.LocationFrom = string.IsNullOrWhiteSpace(flowDetail.LocationFrom) ? flow.LocationFrom : flowDetail.LocationFrom;
                    flowDetail.LocationTo = string.IsNullOrWhiteSpace(flowDetail.LocationTo) ? flow.LocationTo : flowDetail.LocationTo;
                }
            }
            return flow;
        }

    }
}
