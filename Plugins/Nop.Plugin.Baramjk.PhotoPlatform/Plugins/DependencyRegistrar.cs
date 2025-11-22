using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.PhotoPlatform.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Factories.ContactUs;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Plugin.Baramjk.PhotoPlatform.Services.ContactUs;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Plugins
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            //services
            services.AddTransient<EventDetailsService>();
            services.AddTransient<TimeSlotService>();
            services.AddTransient<ActorService>();
            services.AddTransient<ActorEventService>();
            services.AddTransient<PhotoPlatformShoppingCartService>();
            services.AddTransient<PhotoPlatformOrderService>();
            services.AddTransient<ReservationItemService>();
            services.AddTransient<IOrderTotalCalculationService, PhotoPlatformOrderTotalCalculationService>();
            services.AddTransient<ReservationHistoryService>();
            services.AddTransient<EventService>();
            services.AddTransient<CashierEventService>();
            services.AddTransient<CashierBalanceService>();
            services.AddTransient<PhotoPlatformContactInfoService>();
            services.AddTransient<PhotoPlatformContactUsMessageTemplateService>();
            services.AddTransient<PhotoPlatformSubjectService>();
            services.AddTransient<PhotoPlatformContactUsNotifyService>();
            services.AddTransient<PhotoPlatformContactUsPaymentService>();
            services.AddTransient<ReportsService>();
            services.AddTransient<ExportService>();
            services.AddTransient<SupervisorEventService>();
            services.AddTransient<ProductionEventService>();
            services.AddTransient<PhotoPlatformSmsService>();
            services.AddTransient<ActorEventTimeSlotService>();
            services.AddTransient<PhotoPlatformCustomerService>();
            //factories
            services.AddTransient<PhotoPlatformShoppingCartFactory>();
            services.AddTransient<EventFactory>();
            services.AddTransient<PhotoPlatformOrderFactory>();
            services.AddTransient<SubjectFactory>();
            services.AddTransient<PaymentFactory>();
            services.AddTransient<ReportsFactory>();
        }

        public int Order => 1;
    }
}