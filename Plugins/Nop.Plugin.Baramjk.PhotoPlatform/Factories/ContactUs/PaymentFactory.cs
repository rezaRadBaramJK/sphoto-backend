using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ContactUs.Payments;
using Nop.Services.Localization;
using Nop.Services.Payments;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Factories.ContactUs
{
    public class PaymentFactory
    {
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly PaymentSettings _paymentSettings;
        private readonly IPaymentPluginManager _paymentPluginManager;

        public PaymentFactory(
            IWorkContext workContext,
            ILocalizationService localizationService,
            PaymentSettings paymentSettings,
            IPaymentPluginManager paymentPluginManager)
        {
            _workContext = workContext;
            _localizationService = localizationService;
            _paymentSettings = paymentSettings;
            _paymentPluginManager = paymentPluginManager;
        }

        public async Task<IList<PaymentDto>> PreparePaymentDtoListAsync(
            IList<IPaymentMethod> paymentMethods)
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            return await paymentMethods
                .SelectAwait(async pm => new PaymentDto
                {
                    Name = await _localizationService.GetLocalizedFriendlyNameAsync(pm, language.Id),
                    Description = _paymentSettings.ShowPaymentMethodDescriptions
                        ? await pm.GetPaymentMethodDescriptionAsync()
                        : string.Empty,
                    PaymentMethodSystemName = pm.PluginDescriptor.SystemName,
                    LogoUrl = await _paymentPluginManager.GetPluginLogoUrlAsync(pm)
                }).ToListAsync();
        }
    }
}