using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            //factories
            services.AddTransient<EventDetailsAdminFactory>();
            services.AddTransient<TimeSlotAdminFactory>();
            services.AddTransient<ActorAdminFactory>();
            services.AddTransient<ActorEventAdminFactory>();
            services.AddTransient<CashierEventAdminFactory>();
            services.AddTransient<ReportAdminFactory>();
            services.AddTransient<SubjectAdminFactory>();
            services.AddTransient<ContactInfoAdminFactory>();
            services.AddTransient<SupervisorEventAdminFactory>();
            services.AddTransient<ProductionEventAdminFactory>();
            services.AddTransient<ActorEventTimeSlotsAdminFactory>();
        }

        public int Order => 1;
    }
}