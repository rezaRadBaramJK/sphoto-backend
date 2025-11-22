using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Factories.Abstractions;
using Nop.Plugin.Baramjk.Framework.Services.ActionLogs;
using Nop.Plugin.Baramjk.Framework.Services.Addresses;
using Nop.Plugin.Baramjk.Framework.Services.Addresses.Abstractions;
using Nop.Plugin.Baramjk.Framework.Services.BaramjkPlugins;
using Nop.Plugin.Baramjk.Framework.Services.BarCodes;
using Nop.Plugin.Baramjk.Framework.Services.CleanUps;
using Nop.Plugin.Baramjk.Framework.Services.Currencies;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Plugin.Baramjk.Framework.Services.DomainServices;
using Nop.Plugin.Baramjk.Framework.Services.Jwts;
using Nop.Plugin.Baramjk.Framework.Services.License;
using Nop.Plugin.Baramjk.Framework.Services.Networks;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Products;
using Nop.Plugin.Baramjk.Framework.Services.Ui.DataTables;
using Nop.Plugin.Baramjk.Framework.Services.Vendors;
using Nop.Plugin.Baramjk.Framework.Services.DataBaseUtils;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification.Delegates;
using Nop.Services.Configuration;

namespace Nop.Plugin.Baramjk.Framework.Infrastructures
{
    
    public class DependencyRegistrar : IDependencyRegistrar
    {
        //Fayyaz: we will use this to get 
        
        
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            //register custom services
            services.AddScoped<HttpClientHelper>();
            services.AddScoped<ILicenseService, CheckLicenseService>();
            services.AddScoped<ILicenseParser, LicenseParser>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<LicenseGeneratorService>();
            services.AddScoped<IDemoLicenseService, DemoLicenseService>();
            services.AddScoped<IBaramjkPluginService, BaramjkPluginService>();
            services.AddScoped<IBarCodeService, BarCodeService>();
            services.AddScoped<IFavoriteProductService, FavoriteProductService>();
            services.AddScoped<IFavoriteVendorService, FavoriteVendorService>();
            services.AddScoped<IProductVisitService, ProductVisitService>();
            services.AddScoped<ISearchProductService, SearchProductService>();
            services.AddScoped<IVendorSearchService, VendorSearchService>();
            services.AddTransient<IBillingAddressService, BaramjkBillingAddressService>();
            services.AddTransient<VendorDetailsService>();
            services.AddTransient<CleanupService>();

            //payement
            services.AddScoped<IGatewayService, GatewayService>();
            services.AddScoped<IGatewayClientProvider, GatewayClientProvider>();
            services.AddScoped<ITranslationService, TranslationService>();
            services.AddScoped<ITranslationService, TranslationService>();
            services.AddScoped<ITranslationVerifyService, TranslationVerifyService>();

            //register custom factories
            services.AddScoped<IPictureModelFactory, PictureModelFactory>();
            services.AddScoped<ICustomerDtoFactory, CustomerDtoFactory>();
            services.AddScoped<ILicenseManagerService, LicenseManagerService>();
            services.AddScoped<ICategoryDtoFactory, CategoryDtoFactory>();
            services.AddScoped<IProductDtoFactory, ProductDtoFactory>();
            services.AddScoped<IVendorDtoFactory, VendorDtoFactory>();
            services.AddSingleton<IDispatcherService, DispatcherService>();
            services.AddScoped<IActionLogService, ActionLogService>();
            services.AddScoped<ICurrencyTools, CurrencyTools>();
            services.AddScoped<IFakeAddressFactory, FakeAddressFactory>();

            //
            services.AddScoped<IDataTablesBuilders, DataTablesBuilders>();

            //Base
            services.AddScoped(typeof(IDomainService<,>), typeof(BaseDomainService<,>));
            services.AddScoped(typeof(IDomainFactory<,>), typeof(BaseDomainFactory<,>));

            services.AddScoped<IDatabaseUtilsService, DatabaseUtilsService>();
            services.AddScoped<IDatabaseTableUtilsService, DatabaseTableUtilsService>();
            //What's App
            services.AddTransient<WhatsAppProviderResolver>(serviceProvider => () =>
            {
                var settingService = serviceProvider.GetService<ISettingService>();
                var key = settingService.GetSettingByKeyAsync("WhatsAppSettings.ProviderName", string.Empty).Result;

                if (string.IsNullOrEmpty(key))
                    return null;

                return serviceProvider
                    .GetServices<IWhatsAppProvider>()
                    .FirstOrDefault(wp => wp.ProviderName == key);
            });


        }

        public int Order => 1;
    }
}