using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models.Api.Gateways;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Factories
{
    public class GatewayFactory
    {
        private readonly IWorkContext _workContext;

        public GatewayFactory(IWorkContext workContext)
        {
            _workContext = workContext;
        }

        public async Task<IList<GatewayApiResults>> PrepareApiResultAsync(IList<MyFatoorahPaymentMethods> methods)
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            return methods.Select(m => new GatewayApiResults
            {
                Id = m.PaymentMethodId,
                Name = language.UniqueSeoCode == "ar" ? m.PaymentMethodAr : m.PaymentMethodEn,
                Code = m.PaymentMethodCode,
                CurrencyIso = m.CurrencyIso,
                PaymentCurrencyIso = m.PaymentCurrencyIso,
                ImageUrl = m.ImageUrl
            }).ToList();
        }
    }
}