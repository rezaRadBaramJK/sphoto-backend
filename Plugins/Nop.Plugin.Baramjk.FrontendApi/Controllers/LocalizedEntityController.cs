using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Services.Localization;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class LocalizedEntityController : BaseNopWebApiFrontendController
    {
        public LocalizedEntityController(ILocalizedEntityService localizedEntityService,
            IRepository<LocalizedProperty> localizedPropertyRepository)
        {
            _localizedEntityService = localizedEntityService;
            _localizedPropertyRepository = localizedPropertyRepository;
        }

        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;

        protected virtual async Task<IList<LocalizedProperty>> GetLocalizedPropertiesAsync(int entityId,
            string localeKeyGroup)
        {
            if (entityId == 0 || string.IsNullOrEmpty(localeKeyGroup))
                return new List<LocalizedProperty>();

            var query = from lp in _localizedPropertyRepository.Table
                orderby lp.Id
                where lp.EntityId == entityId &&
                      lp.LocaleKeyGroup == localeKeyGroup
                select lp;

            var props = await query.ToListAsync();

            return props;
        }

        protected virtual async Task DeleteLocalizedPropertyAsync(LocalizedProperty localizedProperty)
        {
            await _localizedPropertyRepository.DeleteAsync(localizedProperty);
        }

        protected virtual async Task InsertLocalizedPropertyAsync(LocalizedProperty localizedProperty)
        {
            await _localizedPropertyRepository.InsertAsync(localizedProperty);
        }

        protected virtual async Task UpdateLocalizedPropertyAsync(LocalizedProperty localizedProperty)
        {
            await _localizedPropertyRepository.UpdateAsync(localizedProperty);
        }

        [HttpGet("{languageId}/{entityId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetLocalizedValue(int languageId,
            int entityId,
            [FromQuery] [Required] string localeKeyGroup,
            [FromQuery] [Required] string localeKey)
        {
            var localizedValue = await _localizedEntityService.GetLocalizedValueAsync(languageId, entityId,
                localeKeyGroup, localeKey);

            return ApiResponseFactory.Success(localizedValue);
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetLocalizedValues([FromBody] GetLocalizedValueRequest request)
        {
            var valueItems = new List<LocalizedValueItem>();
            foreach (var entityId in request.EntityIds)
            {
                var localizedValue = await _localizedEntityService.GetLocalizedValueAsync
                    (request.LanguageId, entityId, request.LocaleKeyGroup, request.LocaleKey);

                valueItems.Add(new LocalizedValueItem
                {
                    Value = localizedValue,
                    EntityId = entityId
                });
            }


            return ApiResponseFactory.Success(valueItems);
        }

        [HttpPost("{languageId}/{entityId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> SaveLocalizedValue(int languageId,
            int entityId,
            [FromQuery] [Required] string localeKeyGroup,
            [FromQuery] [Required] string localeKey,
            [FromBody] string localeValue)
        {
            if (languageId <= 0)
                return ApiResponseFactory.BadRequest();

            var props = await GetLocalizedPropertiesAsync(entityId, localeKeyGroup);
            var prop = props.FirstOrDefault(lp => lp.LanguageId == languageId &&
                                                  lp.LocaleKey.Equals(localeKey,
                                                      StringComparison
                                                          .InvariantCultureIgnoreCase)); //should be culture invariant

            if (prop != null)
            {
                if (string.IsNullOrWhiteSpace(localeValue))
                    //delete
                {
                    await DeleteLocalizedPropertyAsync(prop);
                }
                else
                {
                    //update
                    prop.LocaleValue = localeValue;
                    await UpdateLocalizedPropertyAsync(prop);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(localeValue))
                    return ApiResponseFactory.Success();

                //insert
                prop = new LocalizedProperty
                {
                    EntityId = entityId,
                    LanguageId = languageId,
                    LocaleKey = localeKey,
                    LocaleKeyGroup = localeKeyGroup,
                    LocaleValue = localeValue
                };

                await InsertLocalizedPropertyAsync(prop);
            }

            return ApiResponseFactory.Success();
        }
    }

    public class LocalizedValueItem
    {
        public int EntityId { get; set; }
        public string Value { get; set; }
    }

    public class GetLocalizedValueRequest
    {
        public int LanguageId { get; set; }
        public string LocaleKeyGroup { get; set; }
        public string LocaleKey { get; set; }
        public List<int> EntityIds { get; set; }
    }
}