using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.SocialLinks.Services;

namespace Nop.Plugin.Baramjk.SocialLinks.Plugins
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<ISocialLinkEntityService, SocialLinkEntityService>();
            services.Configure<RazorViewEngineOptions>(options => { options.ViewLocationExpanders.Add(new SocialLinksFeaturesViewLocationExpander()); });
        }

        public int Order => int.MaxValue;
    }
}