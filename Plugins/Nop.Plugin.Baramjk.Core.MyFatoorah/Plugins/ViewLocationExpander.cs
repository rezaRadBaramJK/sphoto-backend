using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            viewLocations = context.AreaName != "Admin"
                ? new[]
                {
                    "/Plugins/Nop.Plugin.Payments.MyFatoorah/Views/" + context.ControllerName + "/" + context.ViewName +
                    ".cshtml"
                }.Concat(viewLocations)
                : new[]
                {
                    "/Plugins/Nop.Plugin.Payments.MyFatoorah/Areas/Admin/Views/" + context.ControllerName + "/" +
                    context.ViewName + ".cshtml"
                }.Concat(viewLocations);
            return viewLocations;
        }
    }
}