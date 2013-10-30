namespace com.Sconit.Service.SD
{
    public interface IFlowMgrImpl
    {
        Entity.SD.SCM.FlowMaster GetFlowMaster(string flowCode, bool includeDetail);

        Entity.SD.SCM.FlowMaster GetFlowMasterByFacility(string facilityCode, bool includeDetail);
    }
}
