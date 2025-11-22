using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.Banner.Factories;
using Nop.Plugin.Baramjk.Banner.Services;
using Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Plugin.Baramjk.Banner.Plugins
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            //services
            services.AddScoped<SliderService>();
            services.AddScoped<BannerService>();
            services.AddScoped<IEntityAttachmentService, BannerService>();
            services.AddTransient<BannerLocalizationService>();
            services.AddTransient<BannerProductAttributeService>();
            services.AddTransient<BannerDownloadService>();
            services.AddTransient<BannerCopyProductService>();
            services.AddTransient<BannerVendorService>();
            //factories
            services.AddTransient<IProductModelFactory, BannerProductModelFactory>();
            services.AddTransient<BannerWebApiCatalogModelFactory>();
            services.AddTransient<BannerFactory>();
        }

        public int Order => int.MaxValue;
    }
}