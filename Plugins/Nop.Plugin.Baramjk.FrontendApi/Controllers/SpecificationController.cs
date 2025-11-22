using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class SpecificationController : BaseNopWebApiFrontendAllowAnonymousController
    {
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ILocalizationService _localizationService;

        public SpecificationController(ISpecificationAttributeService specificationAttributeService,
            ILocalizationService localizationService)
        {
            _specificationAttributeService = specificationAttributeService;
            _localizationService = localizationService;
        }

        [ProducesResponseType(typeof(IList<SpecificationAttributeOption>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<IActionResult> GetSpecificationOptionsByAttributeName(string specificationAttributeName)
        {
            var attributes = await _specificationAttributeService.GetSpecificationAttributesWithOptionsAsync();
            var specificationAttribute = attributes.FirstOrDefault(item => item.Name == specificationAttributeName);

            if (specificationAttribute == null)
                return ApiResponseFactory.BadRequest($"{specificationAttributeName} specification Attribute not found");

            var options = await _specificationAttributeService
                .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(specificationAttribute.Id);
            
            foreach (var option in options)
                option.Name = await _localizationService.GetLocalizedAsync(option, x => x.Name);

            return ApiResponseFactory.Success(options);
        }

        [ProducesResponseType(typeof(IList<SpecificationAttributeOption>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<IActionResult> GetSpecificationOptionsByAttributeId(int specificationAttributeId)
        {
            var options = await _specificationAttributeService
                .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(specificationAttributeId);

            foreach (var option in options)
                option.Name = await _localizationService.GetLocalizedAsync(option, x => x.Name);
            
            return ApiResponseFactory.Success(options);
        }

        [ProducesResponseType(typeof(IList<SpecificationAttributeWithOptionsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<IActionResult> GetSpecificationsByGroupName(string groupName)
        {
            var groups = await _specificationAttributeService.GetSpecificationAttributeGroupsAsync();
            var attributeGroup = groups.FirstOrDefault(item => item.Name == groupName);

            if (attributeGroup == null)
                return ApiResponseFactory.BadRequest($"{groupName} specification attribute group not found");

            var specificationAttributes = await _specificationAttributeService
                .GetSpecificationAttributesByGroupIdAsync(attributeGroup.Id);

            var specificationList = new List<SpecificationAttributeWithOptionsDto>();
            foreach (var attribute in specificationAttributes)
            {
                var options = await _specificationAttributeService
                    .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(attribute.Id);

                var specificationAttributeOptionDtos =await options
                    .SelectAwait(async item => new SpecificationAttributeOptionModel
                    {
                        Id = item.Id,
                        SpecificationAttributeId = item.SpecificationAttributeId,
                        Name =await _localizationService.GetLocalizedAsync(item, x => x.Name),
                        ColorSquaresRgb = item.ColorSquaresRgb,
                        DisplayOrder = item.DisplayOrder
                    }).ToListAsync();

                var specificationAttributeDto = new SpecificationAttributeWithOptionsDto
                {
                    SpecificationAttributeId = attribute.Id,
                    Name = await _localizationService.GetLocalizedAsync(attribute, x => x.Name),
                    DisplayOrder = attribute.DisplayOrder,
                    SpecificationAttributeGroupId = attribute.SpecificationAttributeGroupId,
                    SpecificationAttributeOptions = specificationAttributeOptionDtos
                };

                specificationList.Add(specificationAttributeDto);
            }

            return ApiResponseFactory.Success(specificationList);
        }
    }
}