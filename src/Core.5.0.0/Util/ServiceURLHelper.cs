using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Sconit.Util
{
    public static class ServiceURLHelper
    {
        public static string ReplaceServiceUrl(string originalUrl, string serviceAddress, string servicePort)
        {
            string[] urlDetails = originalUrl.Split('/');
            string serviceUrl = string.Empty;
            if (!string.IsNullOrEmpty(serviceAddress))
            {
                if (!string.IsNullOrEmpty(servicePort))
                {
                    urlDetails[2] = serviceAddress + ":" + servicePort;
                }
                else
                {
                    urlDetails[2] = serviceAddress;
                }
            }

            for (int i = 0; i < urlDetails.Length; i++)
            {
                if (i < urlDetails.Length - 1)
                {
                    serviceUrl = serviceUrl + urlDetails[i] + "/";
                }
                else
                {
                    serviceUrl = serviceUrl + urlDetails[i];
                }
            }
            return serviceUrl;
        }
    }
}
