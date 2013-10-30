using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using com.Sconit.Entity;

namespace com.Sconit.Web.Util
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlString Button(this HtmlHelper htmlHelper, string buttonText, string permissions, IDictionary<string, string> attributeDic)
        {
            var user = SecurityContextHolder.Get();
            if (!string.IsNullOrWhiteSpace(permissions))
            {
                string[] permissionArray = permissions.Split(',');
                var q = user.UrlPermissions.Where(p => permissionArray.Contains(p)).ToList();
                if (q == null || q.Count() == 0)
                {
                    return MvcHtmlString.Empty;
                }
            }
            var button = new TagBuilder("button");
            button.SetInnerText(buttonText);

            if (attributeDic.ContainsKey("needconfirm") && bool.Parse(attributeDic["needconfirm"]))
            {
                if (attributeDic.ContainsKey("onclick"))
                {
                    attributeDic["onclick"] = "if( confirm('" + string.Format(Resources.Global.Button_ConfirmOperation, buttonText) + "')){" + attributeDic["onclick"] + "}";
                }
                else
                {
                    attributeDic.Add("onclick", "return confirm('" + string.Format(Resources.Global.Button_ConfirmOperation, buttonText) + "');");
                }
            }
            button.MergeAttributes(attributeDic);
            return new HtmlString("&nbsp;" + button.ToString());
        }

    }
}
