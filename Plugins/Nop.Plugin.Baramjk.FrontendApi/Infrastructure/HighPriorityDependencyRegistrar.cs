using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.FrontendApi.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using MessageTokenProvider = Nop.Plugin.Baramjk.FrontendApi.Services.MessageTokenProvider;

namespace Nop.Plugin.Baramjk.FrontendApi.Infrastructure
{
    /// <summary>
    /// This class was created because some services of this plugin need to inject as high priority
    /// </summary>
    public class HighPriorityDependencyRegistrar : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            //! ATTENTION: All of this services will add as highest priority, For add as normal priority you can add them in "DependencyRegistrar"
            services.AddScoped<IMessageTokenProvider, MessageTokenProvider>();
            services.AddTransient<ILocalizationService, CustomLocalizationService>();
        }

        public int Order => 1000;
    }
}