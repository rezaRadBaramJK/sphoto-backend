using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Services.Common;
using Nop.Web.Areas.Admin.Models.Localization;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class FavoriteCategoryController : BaseNopWebApiFrontendController
    {
        private const string FavoriteCategory = "FavoriteCategory";

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;

        public FavoriteCategoryController(IGenericAttributeService genericAttributeService, IWorkContext workContext)
        {
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<LanguageListModel>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> List()
        {
            var catIds = await GetCatIds();
            return ApiResponseFactory.Success(catIds);
        }

        [HttpPost]
        [ProducesResponseType(typeof(IList<LanguageListModel>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Add([FromQuery] int id)
        {
            var catIds = await GetCatIds();
            if (catIds.Contains(id))
                return ApiResponseFactory.Success(catIds);

            catIds.Add(id);
            await UpdateCatIds(catIds);

            return ApiResponseFactory.Success(catIds);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(IList<LanguageListModel>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Delete([FromQuery] int id)
        {
            var catIds = await GetCatIds();
            if (catIds.Contains(id) == false)
                return ApiResponseFactory.Success(catIds);

            catIds.Remove(id);
            await UpdateCatIds(catIds);

            return ApiResponseFactory.Success(catIds);
        }
        
        [HttpDelete]
        public virtual async Task<IActionResult> DeleteAll()
        {
            await UpdateCatIds(new List<int>());
            return ApiResponseFactory.Success();
        }

        private async Task<List<int>> GetCatIds()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var ids = await _genericAttributeService.GetAttributeAsync(customer, FavoriteCategory, 0, "[]");
            var catIds = JsonConvert.DeserializeObject<List<int>>(ids);
            return catIds;
        }

        private async Task UpdateCatIds(List<int> ids)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var value = JsonConvert.SerializeObject(ids);
            await _genericAttributeService.SaveAttributeAsync(customer, FavoriteCategory, value);
        }
    }
}