using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.FrontendApi.Factories
{
    public interface IWebApiCatalogModelFactory
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();

        Task<List<CategoryDto>> GetHomepageCategoriesAsync(bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0);

        Task<List<CategoryDto>> GetRootCategoriesAsync(bool featuredProduct = false, int productCount = 0,
            int subCategoriesLevel = 0);

        Task<List<CategoryDto>> GetCategoriesAsync(int[] ids, bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0);

        Task<GetSubCategoryDto> GetSubCategoriesAsync(string parentName, bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0);

        Task<List<CategoryDto>> GetSubCategoriesAsync(int parentId, bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0);

        Task<List<CategoryDto>> GetCategoriesByProductIdsAsync(int[] productIds);
        Task<List<CategoryDto>> GetCategoriesByVendorIdAsync(int vendorId, int productCount = 0);
        Task<List<CategoryBreadCrumbDto>> GetCategoryBreadCrumbsAsync(int? parentCategoryId = null);

        Task<CategoryDto> CreateCategoryDtoAsync(Category category, int pictureSize = 0,
            Language language = null, Store store = null, bool featuredProduct = false, int productCount = 0,
            int subCategoriesLevel = 0);

        Task<List<CategoryDto>> CreateCategoryDtoAsync(IList<Category> categories, int pictureSize = 0,
            Language language = null, Store store = null, bool featuredProduct = false, int productCount = 0,
            int subCategoriesLevel = 0);

        Task<List<CategoryDto>> GetCategoriesAsync(List<string> name, bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0);

        Task<IList<SubCategoryModelDto>> PrepareSubCategoriesLevelDtoAsync(int parentId, int subCategoryLevel);

        Task<PictureModel> PreparePictureModel(Category category);

        Task<IList<ProductTagModelDto>> PrepareProductTagDtoAsync(int productId);

        Task<VendorBriefInfoModelDto> PrepareVendorBriefInfoModelAsync(int vendorId);
    }
}