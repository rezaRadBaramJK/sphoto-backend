using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Baramjk.FrontendApi.Infrastructure
{
    /// <summary>
    ///     Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        ///     Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(WebApiFrontendDefaults.CONFIGURATION_ROUTE_NAME,
                "Plugins/WebApiFrontend/Configure",
                new { controller = "WebApiFrontend", action = "Configure", area = AreaNames.Admin });
        }

        /// <summary>
        ///     Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}