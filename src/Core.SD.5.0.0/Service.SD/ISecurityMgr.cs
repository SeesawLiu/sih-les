namespace com.Sconit.Service.SD
{
    using System.Collections.Generic;

    public interface ISecurityMgr
    {
        Entity.SD.ACC.User GetUser(string userCode);

        Entity.ACC.User GetBaseUser(string userCode);
    }
}
