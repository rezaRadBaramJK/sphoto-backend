using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog;
using Nop.Services.Catalog;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class FilterController : BaseNopWebApiFrontendController
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly IRepository<ProductCategory> _repositoryProductCategory;
        private readonly IRepository<ProductManufacturer> _repositoryProductManufacturer;
        private readonly IRepository<ProductSpecificationAttribute> _repositoryProductSpecificationAttribute;
        private readonly ISpecificationAttributeService _specificationAttributeService;

        public FilterController(IRepository<ProductCategory> repositoryProductCategory,
            IRepository<ProductManufacturer> repositoryProductManufacturer,
            IRepository<ProductSpecificationAttribute> repositoryProductSpecificationAttribute,
            IManufacturerService manufacturerService, ISpecificationAttributeService specificationAttributeService)
        {
            _repositoryProductCategory = repositoryProductCategory;
            _repositoryProductManufacturer = repositoryProductManufacturer;
            _repositoryProductSpecificationAttribute = repositoryProductSpecificationAttribute;
            _manufacturerService = manufacturerService;
            _specificationAttributeService = specificationAttributeService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Manufacturer), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetBrandsByCategoryIds([FromBody] IEnumerable<int> categoryIds)
        {
            var productIds = await GetProductIds(categoryIds);
            var manufacturerIds = await _repositoryProductManufacturer.Table
                .Where(item => productIds.Contains(item.ProductId))
                .Select(item => item.ManufacturerId)
                .Distinct()
                .ToArrayAsync();

            var manufacturers = await _manufacturerService.GetManufacturersByIdsAsync(manufacturerIds);
            var enumerable = manufacturers.OrderBy(item => item.DisplayOrder).ThenBy(item => item.Name)
                .Select(item => new
                {
                    item.Id,
                    item.Name,
                    item.Description,
                    item.DisplayOrder
                }).ToList();

            return ApiResponseFactory.Success(enumerable);
        }

        [ProducesResponseType(typeof(IList<SpecificationAttributeWithOptionsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> GetSpecificationsByGroupName(
            [FromBody] GetSpecificationsByGroupNameRequest request)
        {
            var productIds = await GetProductIds(request.CategoryIds);
            var optionsIds = await _repositoryProductSpecificationAttribute.Table
                .Where(item => productIds.Contains(item.ProductId))
                .Select(item => item.SpecificationAttributeOptionId)
                .Distinct()
                .ToArrayAsync();

            var groups = await _specificationAttributeService.GetSpecificationAttributeGroupsAsync();
            var attributeGroup = groups.FirstOrDefault(item => item.Name == request.GroupName);

            if (attributeGroup == null)
                return ApiResponseFactory.BadRequest($"{request.GroupName} specification attribute group not found");

            var specificationAttributes = await _specificationAttributeService
                .GetSpecificationAttributesByGroupIdAsync(attributeGroup.Id);

            var specificationList = new List<SpecificationAttributeWithOptionsDto>();
            foreach (var attribute in specificationAttributes)
            {
                var options = await _specificationAttributeService
                    .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(attribute.Id);
                options = options.Where(item => optionsIds.Contains(item.Id)).ToList();

                if (options.Any() == false)
                    continue;

                var specificationAttributeOptionDtos = options
                    .Select(item => new SpecificationAttributeOptionModel
                    {
                        Id = item.Id,
                        SpecificationAttributeId = item.SpecificationAttributeId,
                        Name = item.Name,
                        ColorSquaresRgb = item.ColorSquaresRgb,
                        DisplayOrder = item.DisplayOrder
                    })
                    .ToList();

                var specificationAttributeDto = new SpecificationAttributeWithOptionsDto
                {
                    SpecificationAttributeId = attribute.Id,
                    Name = attribute.Name,
                    DisplayOrder = attribute.DisplayOrder,
                    SpecificationAttributeGroupId = attribute.SpecificationAttributeGroupId,
                    SpecificationAttributeOptions = specificationAttributeOptionDtos
                };

                specificationList.Add(specificationAttributeDto);
            }

            return ApiResponseFactory.Success(specificationList);
        }

        private async Task<List<int>> GetProductIds(IEnumerable<int> categoryIds)
        {
            return await _repositoryProductCategory.Table
                .Where(item => categoryIds.Contains(item.CategoryId))
                .Select(item => item.ProductId)
                .Distinct()
                .ToListAsync();
        }
    }

    public class GetSpecificationsByGroupNameRequest
    {
        public string GroupName { get; set; }
        public IEnumerable<int> CategoryIds { get; set; }
    }
}