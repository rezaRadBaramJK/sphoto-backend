using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Wallet.Controllers;
using Nop.Plugin.Baramjk.Wallet.Factories;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Services.Orders;
using Nop.Web.Factories;

namespace Nop.Plugin.Baramjk.Wallet.Plugins
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IInternalWalletService, WalletService>();
            services.AddScoped<WalletChargeService>();
            services.AddScoped<WalletProcessPaymentService>();
            services.AddScoped<WalletPackageService>();
            services.AddScoped<WithdrawRequestService>();
            services.AddScoped<WalletHistoryService>();
            services.AddScoped<IOrderProcessingService, WalletOrderProcesssingService>();
            services.AddScoped<OrderProcessingService, WalletOrderProcesssingService>();
            //Factories
            services.AddTransient<IShoppingCartModelFactory, WalletShoppingCartModelFactory>();
            services.AddTransient<CustomerWalletFactory>();
            services.AddTransient<PackageAdminFactory>();
        }

        public int Order => int.MaxValue - 2;
    }
}