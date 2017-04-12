using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OSBIDE.Web.Models.Attributes
{
    public class DenyAccess : ActionFilterAttribute
    {
        List<SystemRole> _roles = new List<SystemRole>();

        public DenyAccess(params SystemRole[] roles)
        {
            _roles = roles.ToList();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Authentication auth = new Authentication();
            string key = auth.GetAuthenticationKey();
            OsbideUser user = auth.GetActiveUser(key);
            bool hasValidRole = true;
            if (user != null)
            {
                foreach(SystemRole role in _roles)
                {
                    if (user.Role == role)
                    {
                        hasValidRole = false;
                    }
                }
            }
            if (hasValidRole == false)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Profile", action = "Index" }));
            }
        }
    }
}