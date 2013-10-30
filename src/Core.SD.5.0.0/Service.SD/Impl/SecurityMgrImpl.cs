namespace com.Sconit.Service.SD.Impl
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using com.Sconit.Entity.SYS;

    public class SecurityMgrImpl : com.Sconit.Service.SD.ISecurityMgr
    {
        public IGenericMgr genericMgr { get; set; }
        public com.Sconit.Service.ISecurityMgr securityMgr { get; set; }

        public Entity.SD.ACC.User GetUser(string userCode)
        {
            Entity.ACC.User user = securityMgr.GetUserWithPermissions(userCode);
            if (user == null)
            {
                throw new Entity.Exception.BusinessException("用户不存在。");
            }
            if (user.IsActive == false)
            {
                throw new Entity.Exception.BusinessException("用户未激活。");
            }
            var sdUser = new Entity.SD.ACC.User();
            sdUser.AccountExpired = user.AccountExpired;
            sdUser.AccountLocked = user.AccountLocked;
            sdUser.Code = user.Code;
            sdUser.Email = user.Email;
            sdUser.FirstName = user.FirstName;
            sdUser.Id = user.Id;
            sdUser.IsActive = user.IsActive;
            sdUser.Language = user.Language;
            sdUser.LastName = user.LastName;
            sdUser.MobilePhone = user.MobilePhone;
            sdUser.Password = user.Password;
            sdUser.PasswordExpired = user.PasswordExpired;
            sdUser.TelPhone = user.TelPhone;

            if (user.Permissions != null)
            {
                user.Permissions = user.Permissions.Where(p => p.PermissionCategoryType != Sconit.CodeMaster.PermissionCategoryType.Url).ToList();
                sdUser.Permissions = Mapper.Map<IList<Entity.VIEW.UserPermissionView>, List<Entity.SD.ACC.Permission>>(user.Permissions);

            }
            sdUser.BarCodeTypes = GetBarCodeTypes();
            return sdUser;
        }

        public Entity.ACC.User GetBaseUser(string userCode)
        {
            return securityMgr.GetUser(userCode);
        }

        private List<Entity.SD.ACC.BarCodeType> GetBarCodeTypes()
        {
            List<Entity.SD.ACC.BarCodeType> barCodeTypes = new List<Entity.SD.ACC.BarCodeType>();

            var snRules = genericMgr.FindAll<SNRule>().OrderByDescending(s => s.PreFixed.Length);
            foreach (var rule in snRules)
            {
                switch ((com.Sconit.CodeMaster.DocumentsType)rule.Code)
                {
                    case com.Sconit.CodeMaster.DocumentsType.ORD_Procurement:
                    case com.Sconit.CodeMaster.DocumentsType.ORD_Transfer:
                    case com.Sconit.CodeMaster.DocumentsType.ORD_Distribution:
                    case com.Sconit.CodeMaster.DocumentsType.ORD_Production:
                    case com.Sconit.CodeMaster.DocumentsType.ORD_SubContract:
                    case com.Sconit.CodeMaster.DocumentsType.ORD_CustomerGoods:
                    case com.Sconit.CodeMaster.DocumentsType.ORD_SubContractTransfer:
                    case com.Sconit.CodeMaster.DocumentsType.ORD_ScheduleLine:
                        barCodeTypes.Add(
                            new Entity.SD.ACC.BarCodeType() { PreFixed = rule.PreFixed, Type = Entity.SD.ACC.OrdType.ORD }
                        );
                        break;
                    case com.Sconit.CodeMaster.DocumentsType.ASN_Procurement:
                    case com.Sconit.CodeMaster.DocumentsType.ASN_Transfer:
                    case com.Sconit.CodeMaster.DocumentsType.ASN_Distribution:
                    case com.Sconit.CodeMaster.DocumentsType.ASN_CustomerGoods:
                    case com.Sconit.CodeMaster.DocumentsType.ASN_ScheduleLine:
                    case com.Sconit.CodeMaster.DocumentsType.ASN_SubContract:
                    case com.Sconit.CodeMaster.DocumentsType.ASN_SubContractTransfer:
                        barCodeTypes.Add(
                         new Entity.SD.ACC.BarCodeType() { PreFixed = rule.PreFixed, Type = Entity.SD.ACC.OrdType.ASN }
                     );
                        break;
                    case com.Sconit.CodeMaster.DocumentsType.PIK_Transfer:
                    case com.Sconit.CodeMaster.DocumentsType.PIK_Distribution:
                        barCodeTypes.Add(
                         new Entity.SD.ACC.BarCodeType() { PreFixed = rule.PreFixed, Type = Entity.SD.ACC.OrdType.PIK }
                     );
                        break;
                    case com.Sconit.CodeMaster.DocumentsType.STT:
                        barCodeTypes.Add(
                         new Entity.SD.ACC.BarCodeType() { PreFixed = rule.PreFixed, Type = Entity.SD.ACC.OrdType.STT }
                     );
                        break;
                    case com.Sconit.CodeMaster.DocumentsType.INS:
                        barCodeTypes.Add(
                         new Entity.SD.ACC.BarCodeType() { PreFixed = rule.PreFixed, Type = Entity.SD.ACC.OrdType.INS }
                     );
                        break;
                    case com.Sconit.CodeMaster.DocumentsType.SEQ:
                        barCodeTypes.Add(
                         new Entity.SD.ACC.BarCodeType() { PreFixed = rule.PreFixed, Type = Entity.SD.ACC.OrdType.SEQ }
                     );
                        break;
                    case com.Sconit.CodeMaster.DocumentsType.BIL_Procurement:
                        break;
                    case com.Sconit.CodeMaster.DocumentsType.BIL_Distribution:
                        break;
                    case com.Sconit.CodeMaster.DocumentsType.RED_Procurement:
                        break;
                    case com.Sconit.CodeMaster.DocumentsType.RED_Distribution:
                        break;
                    case com.Sconit.CodeMaster.DocumentsType.MIS_In:
                    case com.Sconit.CodeMaster.DocumentsType.MIS_Out:
                       barCodeTypes.Add(
                        new Entity.SD.ACC.BarCodeType() { PreFixed = rule.PreFixed, Type = Entity.SD.ACC.OrdType.MIS });
                        break;
                    case com.Sconit.CodeMaster.DocumentsType.REJ:
                        break;
                    case com.Sconit.CodeMaster.DocumentsType.REC_Procurement:
                    case com.Sconit.CodeMaster.DocumentsType.REC_Transfer:
                    case com.Sconit.CodeMaster.DocumentsType.REC_Distribution:
                    case com.Sconit.CodeMaster.DocumentsType.REC_Production:
                    case com.Sconit.CodeMaster.DocumentsType.REC_SubContract:
                    case com.Sconit.CodeMaster.DocumentsType.REC_CustomerGoods:
                    case com.Sconit.CodeMaster.DocumentsType.CON:
                        break;
                    default:
                        break;
                }
            }
            return barCodeTypes;
        }

    }
}
