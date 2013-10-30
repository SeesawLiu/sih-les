using System.Web.Mvc;
using log4net;

namespace com.Sconit.Web.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private static ILog log = LogManager.GetLogger("Log.WebAppErrer");

        public void OnException(ExceptionContext filterContext)
        {
            log.Error(filterContext.Exception);
        }
    }
}