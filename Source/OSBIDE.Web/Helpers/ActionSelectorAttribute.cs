using System.Web.Mvc;

namespace OSBIDE.Web.Helpers
{
    public class ActionSelectorAttribute : ActionMethodSelectorAttribute
    {
        public string Name { get; set; }

        public override bool IsValidForRequest(ControllerContext controllerContext, System.Reflection.MethodInfo methodInfo)
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                    string.Compare(controllerContext.RequestContext.HttpContext.Request.Form["ActionSelector"], Name, true) == 0;
        }
    }
}