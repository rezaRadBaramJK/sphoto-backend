using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Factories;
using Nop.Plugin.Baramjk.ContactUs.Factories;
using Nop.Plugin.Baramjk.ContactUs.Services;

namespace Nop.Plugin.Baramjk.ContactUs.Plugin
{
    public class DependencyRegistrar: IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            //services
            services.AddTransient<SubjectService>();
            services.AddTransient<ContactInfoService>();
            services.AddTransient<ContactUsPaymentService>();
            services.AddTransient<ContactUsNotifyService>();
            services.AddTransient<ContactUsMessageTemplateService>();
            //factories
            services.AddTransient<SubjectAdminFactory>();
            services.AddTransient<ContactInfoAdminFactory>();
            services.AddTransient<SubjectFactory>();
            services.AddTransient<PaymentFactory>();
        }

        public int Order => 1;
    }
}