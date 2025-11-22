using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains.PaymentFeeRule;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Extensions;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models.PaymentFeeRule;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services;
using Nop.Services.Directory;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Factories
{
    public class PaymentFeeRuleFactory
    {
        private readonly PaymentFeeRuleService _paymentFeeRuleService;
        private readonly ICountryService _countryService;
        private readonly MyFatoorahPaymentClient _myFatoorahPaymentClient;


        public PaymentFeeRuleFactory(PaymentFeeRuleService paymentFeeRuleService,
            ICountryService countryService,
            MyFatoorahPaymentClient myFatoorahPaymentClient)
        {
            _paymentFeeRuleService = paymentFeeRuleService;
            _countryService = countryService;
            _myFatoorahPaymentClient = myFatoorahPaymentClient;
        }

        public PaymentFeeRuleSearchModel PrepareSearchModel()
        {
            var model = new PaymentFeeRuleSearchModel();
            model.SetGridPageSize();
            return model;
        }

        private async Task<List<MyFatoorahPaymentMethods>> PreparePaymentMethodsAsync()
        {
            var initiatePaymentRequest = new InitiatePaymentRequest
            {
                CurrencyIso = "KWD",
                InvoiceAmount = 100
            };
            return await _myFatoorahPaymentClient.GetPaymentMethodsAsync(initiatePaymentRequest);
        }


        public async Task<PaymentFeeRuleViewModel> PrepareViewModelAsync(PaymentFeeRule paymentFeeRule = null)
        {
            if (paymentFeeRule == null)
            {
                return new PaymentFeeRuleViewModel
                {
                    AvailableCountries = (await _countryService.GetAllCountriesAsync())
                        .Select(c => new SelectListItem
                        {
                            Text = c.Name,
                            Value = c.Id.ToString()
                        })
                        .ToList(),
                    AvailablePaymentMethods = (await PreparePaymentMethodsAsync())
                        .Select(pm => new SelectListItem
                        {
                            Text = pm.PaymentMethodEn,
                            Value = pm.PaymentMethodId.ToString()
                        })
                        .ToList(),
                };
            }

            var model = paymentFeeRule.Map<PaymentFeeRuleViewModel>();

            model.AvailableCountries = (await _countryService.GetAllCountriesAsync())
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
                .ToList();
            model.AvailablePaymentMethods = (await PreparePaymentMethodsAsync())
                .Select(pm => new SelectListItem
                {
                    Text = pm.PaymentMethodEn,
                    Value = pm.PaymentMethodId.ToString()
                })
                .ToList();

            return model;
        }


        public async Task<PaymentFeeRuleListModel> PrepareListModelAsync(PaymentFeeRuleSearchModel searchModel)
        {
            var pageIndex = searchModel.Page - 1;
            var paymentFeeRules = await _paymentFeeRuleService.GetAllPagedAsync(pageIndex: pageIndex,
                pageSize: searchModel.PageSize);


            var countries = await _countryService.GetCountriesByIdsAsync(paymentFeeRules.Select(pfr => pfr.CountryId).ToArray());

            var methods = await PreparePaymentMethodsAsync();


            return new PaymentFeeRuleListModel().PrepareToGrid(searchModel, paymentFeeRules, () =>
            {
                return paymentFeeRules.Select(paymentFeeRule => new PaymentFeeRuleItemModel
                {
                    Id = paymentFeeRule.Id,
                    PaymentMethodName = methods.FirstOrDefault(m => m.PaymentMethodId == paymentFeeRule.PaymentMethodId)?.PaymentMethodEn,
                    CountryName = countries.FirstOrDefault(c => c.Id == paymentFeeRule.CountryId)?.Name,
                    Active = paymentFeeRule.Active,
                    AdditionalFee = paymentFeeRule.AdditionalFee,
                    AdditionalFeePercentage = paymentFeeRule.AdditionalFeePercentage,
                });
            });
        }
    }
}