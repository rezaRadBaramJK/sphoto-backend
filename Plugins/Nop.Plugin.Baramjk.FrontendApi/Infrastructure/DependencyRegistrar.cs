using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.Framework.Services.Addresses.Abstractions;
using Nop.Plugin.Baramjk.Framework.Services.Language;
using Nop.Plugin.Baramjk.Framework.Services.Products;
using Nop.Plugin.Baramjk.FrontendApi.Factories;
using Nop.Plugin.Baramjk.FrontendApi.Services;
using Nop.Plugin.Baramjk.FrontendApi.Services.Abstractions;
using Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications;
using Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.OAuthTokenParsers;
using Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.Validators;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using RecentlyViewedProductsService = Nop.Plugin.Baramjk.FrontendApi.Services.RecentlyViewedProductsService;

namespace Nop.Plugin.Baramjk.FrontendApi.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        ///     Register services and interfaces
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="appSettings">App settings</param>
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            //! ATTENTION: All of this services will add as normal priority, For add as high priority you can add them in "HighPriorityDependencyRegistrar"
            services.AddScoped<IAuthorizationUserService, AuthorizationUserService>();
            services.AddTransient<IWebApiProductModelFactory, WebApiProductModelFactory>();
            services.AddTransient<IWebApiVendorModelFactory, WebApiVendorModelFactory>();
            services.AddTransient<IWebApiCatalogModelFactory, WebApiCatalogModelFactory>();
            services.AddTransient<IdTokenValidatorService>();
            services.AddTransient<ExternalAuthenticationApiService>();
            services.AddTransient<GoogleOAuthTokenParser>();
            services.AddTransient<AppleOAuthTokenParser>();
            services.AddTransient<FaceBookOAuthTokenParser>();
            services.AddTransient<IWebApiOrderModelFactory, WebApiOrderModelFactory>();
            services.AddTransient<IRecentlyViewedProductsService, RecentlyViewedProductsService>();
            services.AddTransient<PictureHelper>();
            services.AddTransient<VendorFactory>();
            services.AddTransient<WishlistFactory>();
            services.AddTransient<FavoriteVendorService>();
            services.AddTransient<ConditionAttributeService>(); 
            services.AddScoped<ICustomerRegistrationService, CustomerRegistrationExService>();
            services.AddScoped<IBaramjkLanguageService, BaramjkLanguageService>();
            services.AddScoped<FavoriteProductService>();
            services.AddScoped<IAddProductService, AddProductService>();
            services.AddScoped<IVendorRegisterService, VendorRegisterService>();
            services.AddScoped<SearchProductService>();
            services.AddScoped<FrontendOrderService>();
            services.AddScoped<IShoppingCartService, ShoppingCartExtendService>();
            services.AddScoped<ICustomerService, FrontendApiCustomerService>();
            services.AddTransient<IFakeAddressService, FrontendApiFakeAddressService>();
            services.AddTransient<FrontendProductService>();
            services.AddTransient<FrontendCategoryService>();
            services.AddTransient<FrontendApiMessageTemplateService>();
            services.AddTransient<IPasswordRecoveryService, SendLinkPasswordRecoveryService>();
            services.AddTransient<IPasswordRecoveryService, GenerateNewPasswordRecoveryService>();
            services.AddTransient<PasswordRecoveryStrategyResolver>();
            services.AddTransient<CurrencyFactory>();
            services.AddTransient<CategoryFactory>();
            services.AddTransient<FrontendFileService>();
            services.AddTransient<CountryFactory>();
        }

        public int Order => 1;
    }
}