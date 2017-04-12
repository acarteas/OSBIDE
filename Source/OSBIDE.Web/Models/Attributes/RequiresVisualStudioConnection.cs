using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OSBIDE.Web.Models.Attributes
{
    /// <summary>
    /// Using this attribute requires student-type users to have an active connection to visual studio.
    /// </summary>
    public class RequiresVisualStudioConnectionForStudents : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Authentication auth = new Authentication();
            string key = auth.GetAuthenticationKey();
            OsbideUser user = auth.GetActiveUser(key);
            
            //check web.config to see if we are requiring VS plugin install
            if (ConfigurationManager.AppSettings["RequireVsPlugin"].Equals("true"))
            {

                //is the user a student?
                if (user.Email != null && user.Role == SystemRole.Student)
                {
                    DateTime lastActivity = DateTime.UtcNow;
                    using (OsbideContext db = OsbideContext.DefaultWebConnection)
                    {
                        lastActivity = db.Users.Where(u => u.Id == user.Id).Select(u => u.LastVsActivity).FirstOrDefault();
                    }

                    //only allow access if they've been active in Visual Studio in the last 7 days
                    if (lastActivity < DateTime.UtcNow.Subtract(new TimeSpan(7, 0, 0, 0, 0)))
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "RequiresActiveVsConnection" }));
                    }
                }
            }
        }
    }
}