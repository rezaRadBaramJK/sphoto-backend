using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Currencies;
using Nop.Plugin.Baramjk.FrontendApi.Factories;
using Nop.Services.Directory;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class CurrencyController : BaseNopWebApiFrontendController
    {
        private readonly ICurrencyService _currencyService;
        private readonly ICurrencyTools _currencyTools;
        private readonly CurrencyFactory _currencyFactory;

        public CurrencyController(
            ICurrencyService currencyService,
            ICurrencyTools currencyTools,
            CurrencyFactory currencyFactory)
        {
            _currencyService = currencyService;
            _currencyTools = currencyTools;
            _currencyFactory = currencyFactory;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(IList<Currency>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCurrencies()
        {
            var currencies = await _currencyService.GetAllCurrenciesAsync();
            var dtoList = await _currencyFactory.PrepareCurrencyDtoListAsync(currencies);
            return ApiResponseFactory.Success(dtoList);
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(IList<Currency>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertPrimaryToCustomerCurrencyAsync(decimal amount)
        {
            var currencies = await _currencyTools.ConvertPrimaryToCustomerCurrencyAsync(amount);
            return ApiResponseFactory.Success(currencies);
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(IList<Currency>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertCurrencyAsync([FromBody] CurrencyConvertRequest request)
        {
            var currencies = await _currencyTools.ConvertCurrencyAsync(request);
            return ApiResponseFactory.Success(currencies);
        }
    }
}