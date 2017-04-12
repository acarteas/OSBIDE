using Microsoft.Ajax.Utilities;
using OSBIDE.Data.DomainObjects;
using System;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace OSBIDE.Web.Models.Attributes
{
    public class OsbideAuthorize : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var auth = new Authentication();
            var key = auth.GetAuthenticationKey();

            //were we supplied an authentication key from the query string?
            if (filterContext.HttpContext != null)
            {
                var authQueryKey = filterContext.HttpContext.Request.QueryString.Get("auth");
                if (!authQueryKey.IsNullOrWhiteSpace())
                {
                    key = authQueryKey;

                    //if the key is valid, log the user into the system and then retry the request
                    if (auth.IsValidKey(key))
                    {
                        auth.LogIn(auth.GetActiveUser(key));
                        var routeValues = new RouteValueDictionary();
                        routeValues["controller"] = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                        routeValues["action"] = filterContext.ActionDescriptor.ActionName;
                        foreach (var parameterKey in filterContext.ActionParameters.Keys)
                        {
                            var parameterValue = filterContext.ActionParameters[parameterKey];
                            routeValues[parameterKey] = parameterValue;
                        }
                        filterContext.Result = new RedirectToRouteResult(routeValues);
                        return;
                    }
                }
            }
            if (auth.IsValidKey(key) == false)
            {
                if (filterContext.HttpContext != null)
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "Login", returnUrl = filterContext.HttpContext.Request.Url }));
            }
            else
            {
                //log the request
                var log = new ActionRequestLog
                {
                    ActionName = filterContext.ActionDescriptor.ActionName,
                    ControllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                    CreatorId = auth.GetActiveUser(key).Id,
                    AccessDate = DateTime.UtcNow,
                    SchoolId = 1, // need to get school Id
                };
                try
                {
                    log.IpAddress = filterContext.RequestContext.HttpContext.Request.ServerVariables["REMOTE_ADDR"];
                }
                catch(Exception)
                {
                    log.IpAddress = "Unknown";
                }
                var parameters = new StringBuilder();
                foreach (var parameterKey in filterContext.ActionParameters.Keys)
                {
                    var parameterValue = filterContext.ActionParameters[parameterKey] ?? DomainConstants.ActionParameterNullValue;
                    parameters.Append(string.Format("{0}={1}{2}", parameterKey, parameterValue, DomainConstants.ActionParameterDelimiter));
                }
                log.ActionParameters = parameters.ToString();

                //save to azure table storage
                DomainObjectHelpers.LogActionRequest(log);
            }
        }
    }
}