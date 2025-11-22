using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json;

namespace Nop.Plugin.Baramjk.SocialLinks.Plugins
{
    public class SocialLinksFeaturesViewLocationExpander :IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            
            Console.WriteLine("=========================");
            Console.WriteLine(context.ViewName);
            Console.WriteLine(JsonConvert.SerializeObject(viewLocations));
            if (context.ViewName == "Components/Footer/Default")
            {
                viewLocations = new[] {"/Plugins/Baramjk.SocialLinks/Views/Footer/Default.cshtml"}
                    .Concat(viewLocations);
            }

            if (context.ViewName == "_ShareButton")
            {
                viewLocations = new[] {"/Plugins/Baramjk.SocialLinks/Views/ProductShare.cshtml"}
                    .Concat(viewLocations);
            }
            return viewLocations;
        }
    }
}