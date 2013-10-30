using System;
using System.Collections.Generic;
using System.Linq;
using com.Sconit.Entity;
using com.Sconit.Entity.ACC;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.SYS;
using EmitMapper;
using NHibernate.Criterion;
using com.Sconit.Util;

namespace com.Sconit.Service.Impl
{
    public class SystemMgrImpl : BaseMgr, ISystemMgr
    {
        public IGenericMgr genericMgr { get; set; }
        private static IDictionary<EntityPreference.CodeEnum, string> entityPreferenceCache;
        private static string entityPreferenceLock = string.Empty;
        private static IDictionary<com.Sconit.CodeMaster.CodeMaster, IList<CodeDetail>> codeMasterCache;
        private static string codeMasterLock = string.Empty;
        private static IList<Menu> menuCache;
        private static string menuCacheLock = string.Empty;
        public SystemMgrImpl()
        {
        }

        public void LoadEntityPreferenceCache()
        {
            lock (menuCacheLock)
            {
                DetachedCriteria criteria = DetachedCriteria.For<EntityPreference>();
                IList<EntityPreference> epList = genericMgr.FindAll<EntityPreference>(criteria);

                entityPreferenceCache = new Dictionary<EntityPreference.CodeEnum, string>();

                foreach (EntityPreference ep in epList)
                {
                    entityPreferenceCache.Add(((EntityPreference.CodeEnum)ep.Id), ep.Value);
                }
            }
        }

        public string GetEntityPreferenceValue(EntityPreference.CodeEnum code)
        {
            if (entityPreferenceCache == null)
            {
                LoadEntityPreferenceCache();
            }

            return entityPreferenceCache[code];
        }

        public void LoadCodeDetailCache()
        {
            lock (codeMasterLock)
            {
                DetachedCriteria criteria = DetachedCriteria.For<CodeDetail>();
                IList<CodeDetail> codeDetailList = genericMgr.FindAll<CodeDetail>(criteria);

                var groupedCodeDetailList = from det in codeDetailList
                                            orderby det.Sequence
                                            group det by det.Code into result
                                            select new
                                            {
                                                Code = result.Key,
                                                List = result.ToList()
                                            };

                codeMasterCache = new Dictionary<com.Sconit.CodeMaster.CodeMaster, IList<CodeDetail>>();

                if (groupedCodeDetailList != null && groupedCodeDetailList.Count() > 0)
                {
                    foreach (var groupedCodeDetail in groupedCodeDetailList)
                    {
                        codeMasterCache.Add((com.Sconit.CodeMaster.CodeMaster)Enum.Parse(typeof(com.Sconit.CodeMaster.CodeMaster), groupedCodeDetail.Code), groupedCodeDetail.List);
                    }
                }
            }
        }

        public IDictionary<com.Sconit.CodeMaster.CodeMaster, IList<CodeDetail>> GetCodeDetailDictionary()
        {
            if (codeMasterCache == null)
            {
                LoadCodeDetailCache();
            }

            return codeMasterCache;
        }

        public IList<CodeDetail> GetCodeDetails(com.Sconit.CodeMaster.CodeMaster code)
        {
            if (codeMasterCache == null)
            {
                LoadCodeDetailCache();
            }

            IList<CodeDetail> returnList = new List<CodeDetail>();
            foreach (CodeDetail codeDetail in codeMasterCache[code])
            {
                returnList.Add(ObjectMapperManager.DefaultInstance.GetMapper<CodeDetail, CodeDetail>().Map(codeDetail));
            }

            return returnList;
        }

        public CodeDetail GetDefaultCodeDetail(com.Sconit.CodeMaster.CodeMaster code)
        {
            if (codeMasterCache == null)
            {
                LoadCodeDetailCache();
            }

            return (from cd in codeMasterCache[code]
                    where cd.IsDefault == true
                    select cd).SingleOrDefault();
        }

        public IList<CodeDetail> GetCodeDetails(com.Sconit.CodeMaster.CodeMaster code, bool? includeBlankOption, string blankOptionDescription, string blankOptionValue)
        {
            IList<CodeDetail> codeDetailList = this.GetCodeDetails(code);

            if (includeBlankOption.HasValue && includeBlankOption.Value)
            {
                CodeDetail blankCodeDetail = new CodeDetail();
                blankCodeDetail.Description = blankOptionDescription != null ? blankOptionDescription : string.Empty;
                blankCodeDetail.Value = blankOptionValue != null ? blankOptionValue : string.Empty;
                codeDetailList.Insert(0, blankCodeDetail);
            }

            return codeDetailList;
        }

        public string GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster code, string value)
        {
            if (codeMasterCache == null)
            {
                LoadCodeDetailCache();
            }

            if (codeMasterCache.ContainsKey(code))
            {
                CodeDetail codeDetail = codeMasterCache[code].Where(det => det.Value == value).SingleOrDefault();
                if (codeDetail != null)
                {
                    string desc = Resources.CodeDetail.ResourceManager.GetString(codeDetail.Description);
                    if (desc != null)
                    {
                        return desc;
                    }
                    else
                    {
                        return codeDetail.Description;
                        //throw new TechnicalException("Description define not correct of codeMaster [" + code + "] and value [" + value + "].");
                    }
                }
                else
                {
                    //throw new TechnicalException("CodeMaster [" + code + "] does not contain value [" + value + "].");
                    return Resources.CodeDetail.Errors_CodeDetail_ValueNotFound;
                }
            }
            else
            {
                throw new TechnicalException("CodeMaster [" + code + "] does not exist.");
            }
        }

        public string GetCodeDetailDescription(com.Sconit.CodeMaster.CodeMaster code, int value)
        {
            return GetCodeDetailDescription(code, value.ToString());
        }

        public string TranslateCodeDetailDescription(string description)
        {
            string desc = Resources.CodeDetail.ResourceManager.GetString(description);
            if (desc != null)
            {
                return desc;
            }
            else
            {
                return description;
            }
        }

        public string TranslateEntityPreferenceDescription(string description)
        {
            string desc = Resources.EntityPreference.ResourceManager.GetString(description);
            if (desc != null)
            {
                return desc;
            }
            else
            {
                return description;
            }
        }

        public void LoadMenuCache()
        {
            lock (menuCacheLock)
            {
                DetachedCriteria criteria = DetachedCriteria.For<Menu>();
                criteria.Add(Expression.Eq("IsActive", true));
                menuCache = genericMgr.FindAll<Menu>(criteria);
            }
        }

        public IList<Menu> GetAllMenu()
        {
            if (menuCache == null)
            {
                LoadMenuCache();
            }

            return menuCache;
        }

        public User GetMonitorUser()
        {
            return genericMgr.FindById<User>(BusinessConstants.SYSTEM_USER_MONITOR);
        }

        #region SI Service URL Replace
        private static string _siServiceAddress { get; set; }
        private static string _siServicePort { get; set; }

        private string SIServiceAddress
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_siServiceAddress))
                {
                    _siServiceAddress = this.GetEntityPreferenceValue(
                        com.Sconit.Entity.SYS.EntityPreference.CodeEnum.SIServiceAddress);
                }
                return _siServiceAddress;
            }
        }

        private string SIServicePort
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_siServicePort))
                {
                    _siServicePort = this.GetEntityPreferenceValue(
                        com.Sconit.Entity.SYS.EntityPreference.CodeEnum.SIServicePort);
                }
                return _siServicePort;
            }
        }

        public string ReplaceSIServiceUrl(string originalUrl)
        {
            return ServiceURLHelper.ReplaceServiceUrl(originalUrl, this.SIServiceAddress, this.SIServicePort);
        }
        #endregion
    }
}
