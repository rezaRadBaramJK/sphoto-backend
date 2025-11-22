using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Factories;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services.Verify;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;


namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            //services
            services.AddScoped<MyFatoorahPaymentClient>();
            services.AddScoped<IMyFatoorahPaymentClient, MyFatoorahPaymentClient>();
            services.AddScoped<ITranslationService, TranslationService>();
            services.AddScoped<IMyFatoorahTranslationVerifyPaymentService, MyFatoorahTranslationVerifyService>();
            services.AddScoped<ICheckoutVerifyPaymentService, CheckoutVerifyPaymentService>();
            services.AddScoped<GatewayFactory>();
            services.AddTransient<PaymentFeeRuleService>();
            services.AddTransient<SupplierService>();
            services.AddTransient<TransactionService>();


            //factories 
            services.AddTransient<PaymentFeeRuleFactory>();
            services.AddTransient<SupplierAdminFactory>();
            services.AddTransient<SupplierFactory>();
        }

        public int Order => 1;
    }
}