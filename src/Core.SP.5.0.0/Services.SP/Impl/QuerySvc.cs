using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Sconit.Persistence.SP;
//using com.Sconit.Entity.SP.ORD;
using com.Sconit.PrintModel.ORD;

namespace com.Sconit.Services.SP.Impl
{
    public class QuerySvc : IQuerySvc
    {
        private static NHQueryDao queryDao = new NHQueryDao();

        private static string selectOrderDetMasterStatement = "select pod from PrintOrderDetail as pod where pod.OrderNo=?";

        public PrintOrderMaster GetOrderMstr(string OrderNo)
        {
            var obj = queryDao.FindById<PrintOrderMaster>(OrderNo);
            return obj;
        }

        public IList<PrintOrderDetail> GetOrderDets(string OrderNo)
        {

            var obj = queryDao.FindAllWithCustomQuery<PrintOrderDetail>(selectOrderDetMasterStatement, OrderNo);
            return obj;
        }
    }
}
