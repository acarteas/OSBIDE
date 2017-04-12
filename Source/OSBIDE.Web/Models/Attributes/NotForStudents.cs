using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OSBIDE.Web.Models.Attributes
{
    public class NotForStudents : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Authentication auth = new Authentication();
            string key = auth.GetAuthenticationKey();
            OsbideUser user = auth.GetActiveUser(key);
            if (user == null || user.RoleValue <= (int)SystemRole.Student)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Feed", action = "Index" }));
            }
        }
    }
}