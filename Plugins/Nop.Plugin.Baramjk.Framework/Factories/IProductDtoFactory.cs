using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Models.Categories;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.Framework.Factories
{
    public interface IProductDtoFactory
    {
        Task<List<ProductOverviewDto>> PrepareProductOverviewAsync(int[] productIds);

        Task<List<ProductOverviewDto>> PrepareProductOverviewAsync(
            IEnumerable<Product> products, bool preparePrice = true, bool preparePicture = true,
            int? thumbSize = null, bool prepareSpecifications = true, bool prepareTag = true,
            bool prepareCategories = true, bool prepareAttributes = false);

        Task<ProductBriefModel> PrepareProductBriefModelAsync(int id);
        Task<ProductBriefModel> PrepareProductBriefModelAsync(Product product);

        Task<ProductOverviewModel.ProductPriceModel> PrepareProductOverviewPriceModelAsync(
            Product product, bool forceRedirectionAfterAddingToCart = false);

        Task<PictureModel> PrepareProductOverviewPictureModelAsync(Product product,
            int? productThumbPictureSize = null);

        Task<IList<ProductTag>> GetProductTagsAsync(Product product);
        Task<List<CategoryItemDto>> GetProductCategoriesAsync(Product product);
        Task<CategoryItemDto> GetHighestOrderProductCategoryAsync(int productId);
        Task<ProductSpecificationModel> PrepareProductSpecificationModelAsync(Product product);
    }
}