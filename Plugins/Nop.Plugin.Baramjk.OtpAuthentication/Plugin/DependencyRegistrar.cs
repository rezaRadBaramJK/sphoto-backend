using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.OtpAuthentication.Factories;
using Nop.Plugin.Baramjk.OtpAuthentication.Methods;
using Nop.Plugin.Baramjk.OtpAuthentication.Methods.Abstractions;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Settings;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Types;
using Nop.Plugin.Baramjk.OtpAuthentication.Services;
using Nop.Plugin.Baramjk.OtpAuthentication.Services.Abstractions;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Plugin
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IOtpAuthenticationService, OtpAuthenticationService>();
            services.AddScoped<IMobileOtpService, MobileOtpService>();
            services.AddScoped<IAuthorizationUserService, AuthorizationUserService>();
            services.AddScoped<IOtpVendorService, OtpVendorService>();
            services.AddTransient<OtpMessageTemplateService>();
            //factories
            services.AddScoped<SettingFactory>();
            //methods
            services.AddTransient<SmsSendMethod>();
            services.AddTransient<WhatsAppSendMethod>();
            services.AddTransient<EmailSendMethod>();
            services.AddTransient<EmailOrSmsSendMethod>();
            services.AddTransient(provider =>
            {
                var methods = new Dictionary<SendMethodType, Func<ISendMethod>>
                {
                    { SendMethodType.Sms, provider.GetService<SmsSendMethod> },
                    { SendMethodType.Whatsapp, provider.GetService<WhatsAppSendMethod> },
                    { SendMethodType.Email, provider.GetService<EmailSendMethod> },
                    { SendMethodType.EmailOrSms, provider.GetService<EmailOrSmsSendMethod> }
                };
                var settings = provider.GetService<OtpAuthenticationSettings>();
                return methods[settings.SendMethod].Invoke();
            });
        }

        public int Order => 1;
    }
}