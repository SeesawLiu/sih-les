using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using com.Sconit.Persistence;
using com.Sconit.Entity.VIEW;
using Castle.Windsor;
using com.Sconit.Entity.INV;

namespace com.Sconit.Service.Impl
{
    public class ReportMgrImpl : IReportMgr
    {
        public ISqlQueryDao sqlQueryDao { get; set; }

        public List<object> GetRealTimeLocationDetail(string procedureName, SqlParameter[] parameters)
        {
            List<Object> result = new List<object>();
            DataSet ds = sqlQueryDao.GetDatasetByStoredProcedure(procedureName, parameters);
            result.Add((int)ds.Tables[0].Rows[0].ItemArray[0]);
            var locationDetailList = (from t in ds.Tables[1].AsEnumerable()
                                      select new LocationDetailView
                                      {
                                          Location = t.Field<string>("Location"),
                                          Item = t.Field<string>("Item"),
                                          ManufactureParty = t.Field<string>("ManufactureParty"),
                                          IsCS = t.Field<bool>("IsCS"),
                                          Qty = t.Field<decimal>("Qty"),
                                          LotNo = t.Field<string>("LotNo"),
                                          //CsQty = t.Field<decimal>("CsQty"),
                                          QualifyQty = t.Field<decimal>("QualifyQty"),
                                          InspectQty = t.Field<decimal>("InspectQty"),
                                          RejectQty = t.Field<decimal>("RejectQty"),
                                          ItemDescription = t.Field<string>("ItemDescription"),
                                          Uom = t.Field<string>("Uom")
                                          //ATPQty = t.Field<decimal>("ATPQty"),
                                          //FreezeQty = t.Field<decimal>("FreezeQty")
                                      }).ToList();
            result.Add(locationDetailList);

            return result;
        }

        public List<object> GetHistoryInvAjaxPageData(string procedureName, SqlParameter[] parameters)
        {
            List<Object> result = new List<object>();
            DataSet ds = sqlQueryDao.GetDatasetByStoredProcedure(procedureName, parameters);
            result.Add((int)ds.Tables[0].Rows[0].ItemArray[0]);
            var HistoryInvList = (from t in ds.Tables[1].AsEnumerable()
                                  select new HistoryInventory
                                 {
                                     Location = t.Field<string>("Location"),
                                     Item = t.Field<string>("Item"),
                                     ManufactureParty = t.Field<string>("ManufactureParty"),
                                     LotNo = t.Field<string>("LotNo"),
                                     CsQty = t.Field<decimal>("CsQty"),
                                     QualifyQty = t.Field<decimal>("QualifyQty"),
                                     InspectQty = t.Field<decimal>("InspectQty"),
                                     RejectQty = t.Field<decimal>("RejectQty"),
                                     TobeQualifyQty = t.Field<decimal>("TobeQualifyQty"),
                                     TobeInspectQty = t.Field<decimal>("TobeInspectQty"),
                                     TobeRejectQty = t.Field<decimal>("TobeRejectQty")

                                 }).ToList();
            result.Add(HistoryInvList);

            return result;
        }

        public List<object> GetInventoryAgeAjaxPageData(string procedureName, SqlParameter[] parameters)
        {
            List<Object> result = new List<object>();
            DataSet ds = sqlQueryDao.GetDatasetByStoredProcedure(procedureName, parameters);
                result.Add((int)ds.Tables[0].Rows[0].ItemArray[0]);
            var HistoryInvList = (from t in ds.Tables[1].AsEnumerable()
                                  select new InventoryAge
                                  {
                                      Location = t.Field<string>("Location"),
                                      Item = t.Field<string>("Item"),
                                      Range0 = t.Field<object>("Range0") == null ? "" :Convert.ToDouble(t.Field<object>("Range0")).ToString(),
                                      Range1 = t.Field<object>("Range1") == null ? "" : Convert.ToDouble(t.Field<object>("Range1")).ToString(),
                                      Range2 = t.Field<object>("Range2") == null ? "" : Convert.ToDouble(t.Field<object>("Range2")).ToString(),
                                      Range3 = t.Field<object>("Range3") == null ? "" : Convert.ToDouble(t.Field<object>("Range3")).ToString(),
                                      Range4 = t.Field<object>("Range4") == null ? "" : Convert.ToDouble(t.Field<object>("Range4")).ToString(),
                                      Range5 = t.Field<object>("Range5") == null ? "" : Convert.ToDouble(t.Field<object>("Range5")).ToString(),
                                      Range6 = t.Field<object>("Range6") == null ? "" : Convert.ToDouble(t.Field<object>("Range6")).ToString(),
                                      Range7 = t.Field<object>("Range7") == null ? "" : Convert.ToDouble(t.Field<object>("Range7")).ToString(),
                                      Range8 = t.Field<object>("Range8") == null ? "" : Convert.ToDouble(t.Field<object>("Range8")).ToString(),
                                      Range9 = t.Field<object>("Range9") == null ? "" : Convert.ToDouble(t.Field<object>("Range9")).ToString(),
                                      Range10 = t.Field<object>("Range10") == null ? "" : Convert.ToDouble(t.Field<object>("Range10")).ToString(),
                                      Range11 = t.Field<object>("Range11") == null ? "" : Convert.ToDouble(t.Field<object>("Range11")).ToString()

                                  }).ToList();
            result.Add(HistoryInvList);

            return result;
        }

        public List<object> GetReportTransceiversAjaxPageData(string procedureName, SqlParameter[] parameters)
        {
            List<Object> result = new List<object>();
            DataSet ds = sqlQueryDao.GetDatasetByStoredProcedure(procedureName, parameters);
            result.Add((int)ds.Tables[0].Rows[0].ItemArray[0]);
            var locationDetailList = (from t in ds.Tables[1].AsEnumerable()
                                      select new Transceivers
                                      {
                                          Location = t.Field<string>("Location"),
                                          Item = t.Field<string>("Item"),
                                          //SAPLocation = t.Field<string>("SAPLocation"),
                                          BOPQty = t.Field<decimal>("BOPQty"),
                                          EOPQty = t.Field<decimal>("EOPQty"),
                                          InputQty = t.Field<decimal>("InputQty"),
                                          OutputQty = t.Field<decimal>("OutputQty")
                                         
                                      }).ToList();
            result.Add(locationDetailList);

            return result;
        }

    }
}
