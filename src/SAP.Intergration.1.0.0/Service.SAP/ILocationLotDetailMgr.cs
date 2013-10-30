using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Sconit.Service.SAP
{
    public interface ILocationLotDetailMgr
    {
        void ReportLocationLotDetail(string ftpServer, int ftpPort, string ftpUser, string ftpPass, string ftpFolder,
                               string localFolder, string localTempFolder);
    }
}
