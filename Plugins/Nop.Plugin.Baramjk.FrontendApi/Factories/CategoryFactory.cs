using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models.PagedLists;
using Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Banners;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog;
using Nop.Plugin.Baramjk.FrontendApi.Services.Models.Categories;
using Nop.Services.Localization;

namespace Nop.Plugin.Baramjk.FrontendApi.Factories
{
    public class CategoryFactory
    {
        private readonly ILocalizationService _localizationService;
        private readonly IProductDtoFactory _productDtoFactory;
        private readonly IEntityAttachmentService _attachmentService;
        

        public CategoryFactory(ILocalizationService localizationService, IProductDtoFactory productDtoFactory)
        {
            _localizationService = localizationService;
            _productDtoFactory = productDtoFactory;
            //!Fayyaz: DO NOT INJECT BY CONSTRUCTOR
            _attachmentService = EngineContext.Current.Resolve<IEntityAttachmentService>();
        }

        public async Task<CamelCasePagedList<CategoryProductsDto>> 
            PrepareCategoryProductsAsync(IPagedList<CategoryProductPair> categoriesProducts, bool prepareProductAttributes)
        {
            var result = await categoriesProducts
                .SelectAwait(async pairs =>
                {
                    return new CategoryProductsDto
                    {
                        Id = pairs.Category.Id,
                        Name = await _localizationService.GetLocalizedAsync(pairs.Category, c => c.Name),
                        Products = await _productDtoFactory.PrepareProductOverviewAsync(pairs.Products, prepareAttributes: prepareProductAttributes),
                        Banners = await PrepareBannersAsync(pairs.Category.Id)
                    };
                })
                .ToListAsync();

            var pageNumber = categoriesProducts.PageIndex + 1;
            return new CamelCasePagedList<CategoryProductsDto>(
                result, pageNumber,
                categoriesProducts.PageSize,
                categoriesProducts.TotalCount);
        }

        public async Task<List<BannerDto>> PrepareBannersAsync(int categoryId)
        {
            var attachmentModels = _attachmentService == null
                ? new List<IEntityAttachmentModel>()
                : await _attachmentService.GetAttachmentsAsync(nameof(Category), categoryId);

            return attachmentModels.Select(am => new BannerDto
            {
                Id = am.Id,
                EntityName = am.EntityName,
                EntityId = am.EntityId,
                FileName = am.FileName,
                Title = am.FileName,
                Text = am.Text,
                Link = am.Link,
                AltText = am.AltText,
                Tag = am.Tag,
                DisplayOrder = am.DisplayOrder,
                AttachmentType = am.AttachmentType,
                DownloadUrl = am.DownloadUrl,
                ExpirationDateTime = am.ExpirationDateTime,
                CustomProperties = new Dictionary<string, object>()
            }).ToList();

        }
        
    }
}