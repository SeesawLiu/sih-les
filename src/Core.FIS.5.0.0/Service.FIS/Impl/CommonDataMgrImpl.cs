using com.Sconit.Entity.FIS;
using com.Sconit.Service.FIS.Impl;
using System.Collections.Generic;
using System.Linq;
using System;

namespace com.Sconit.Service.FIS.Impl
{
    public class CommonDataMgrImpl : ICommonDataMgr
    {
        public IGenericMgr genericMgr { get; set; }

        public void CreateIpDAT(CreateIpDAT createIpDAT)
        {
            this.genericMgr.Create(createIpDAT);
        }

        public bool CheckIsCreateIpDat(string ipNo)
        {
            IList<CreateIpDAT> ipDatList = this.genericMgr.FindAll<CreateIpDAT>("select c from CreateIpDAT as c where c.ASN_NO=?", ipNo);
            if (ipDatList != null && ipDatList.Count > 0)
            {
                string FileName = "ASNLE" + DateTime.Now.ToString("yyMMddHHmmss");
                this.genericMgr.Update("update CreateIpDAT set IsCreateDat=0,FileName=? where ASN_NO=?", new object[] { FileName, ipNo });
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CreateOrderDAT(CreateOrderDAT CreateOrderDAT)
        {
            this.genericMgr.Create(CreateOrderDAT);
        }

        public  bool CheckIsCreateOrderDat(string orderNo)
        {
            IList<CreateOrderDAT> orderDatList = this.genericMgr.FindAll<CreateOrderDAT>("select c from CreateOrderDAT as c where c.OrderNo=?", orderNo);
            if (orderDatList != null && orderDatList.Count > 0)
            {
                string FileName = "SEQ1LE" + DateTime.Now.ToString("yyMMddHHmmss");
                this.genericMgr.Update("update CreateOrderDAT set IsCreateDat=0,FileName=? where OrderNo=?", new object[] { FileName, orderNo });
                return true;
            }
            else
            {
                return false;
            }
        }

       

        public void CreateLog(LesINLog LesINLog)
        {
            this.genericMgr.Create(LesINLog);
        }

        public LesINLog SelectLesINLogByWmsNo(string selectSql, object param)
        {
            IList<LesINLog> lesLogList= this.genericMgr.FindAll<LesINLog>(selectSql, param);
            if (lesLogList != null && lesLogList.Count > 0)
            {
               return lesLogList.First();
            }
            else
            {
                return null;
            }
        }

        public void UpdateLog(LesINLog LesINLog)
        {
            this.genericMgr.Update(LesINLog);
        }
    }
}
