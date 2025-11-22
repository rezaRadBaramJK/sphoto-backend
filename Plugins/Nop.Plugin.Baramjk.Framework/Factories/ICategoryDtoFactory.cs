using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Baramjk.Framework.Models.Categories;

namespace Nop.Plugin.Baramjk.Framework.Factories
{
    public interface ICategoryDtoFactory
    {
        Task<CategoryDto> CreateCategoryDtoAsync(Category category, int pictureSize = 0,
            Language language = null, Store store = null, bool featuredProduct = false, int productCount = 0,
            int subCategoriesLevel = 0);

        Task<List<CategoryDto>> CreateCategoryDtoAsync(IList<Category> categories, int pictureSize = 0,
            Language language = null, Store store = null, bool featuredProduct = false, int productCount = 0,
            int subCategoriesLevel = 0);
    }
}