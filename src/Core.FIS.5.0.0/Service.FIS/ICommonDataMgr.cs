using com.Sconit.Entity.FIS;

namespace com.Sconit.Service.FIS
{
    public interface ICommonDataMgr
    {
        //void CreateEdiOrderInfo(EdiOrderInfo ediOrderInfo);
        void CreateIpDAT(CreateIpDAT createIpDAT);
        bool CheckIsCreateIpDat(string ipNo);
        void CreateOrderDAT(CreateOrderDAT CreateOrderDAT);
         bool CheckIsCreateOrderDat(string orderNo);
        void CreateLog(LesINLog LesINLog);
        LesINLog SelectLesINLogByWmsNo(string selectSql, object param);
        void UpdateLog(LesINLog LesINLog);
        
    }
}