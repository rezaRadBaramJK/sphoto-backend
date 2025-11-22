using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Country;
using Nop.Plugin.Baramjk.FrontendApi.Factories;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Directory;
using Nop.Web.Factories;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class CountryController : BaseNopWebApiFrontendController
    {
        #region Ctor

        public CountryController(
            ICountryModelFactory countryModelFactory,
            ICountryService countryService,
            CountryFactory countryFactory)
        {
            _countryModelFactory = countryModelFactory;
            _countryService = countryService;
            _countryFactory = countryFactory;
        }

        #endregion

        #region Fields

        private readonly ICountryModelFactory _countryModelFactory;
        private readonly ICountryService _countryService;
        private readonly CountryFactory _countryFactory;

        #endregion

        #region States / provinces

        [HttpGet("")]
        [ProducesResponseType(typeof(IList<StateProvinceModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCountries()
        {
            var countries = await _countryService.GetAllCountriesAsync();
            var results = await _countryFactory.PrepareCountriesAsync(countries);
            return ApiResponseFactory.Success(results);
        }

        [HttpGet("{countryId}")]
        [ProducesResponseType(typeof(IList<StateProvinceModelDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetStatesByCountryId(int countryId,
            [FromQuery] [Required] bool addSelectStateItem)
        {
            //TODO: We need to change the parameters of the GetStatesByCountryIdAsync method to pass the int value
            var model = await _countryModelFactory.GetStatesByCountryIdAsync(countryId.ToString(), addSelectStateItem);
            var modelDto = model.Select(p => p.ToDto<StateProvinceModelDto>()).ToList();

            return ApiResponseFactory.Success(modelDto);
        }

        [HttpGet("")]
        public IActionResult CountryCodes()
        {
            return Redirect("/Plugins/Baramjk.FrontendApi/Content/CountryCodes.json");
        }

        #endregion
    }
}