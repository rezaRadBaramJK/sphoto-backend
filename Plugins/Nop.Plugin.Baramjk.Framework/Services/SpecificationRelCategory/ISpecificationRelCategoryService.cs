using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Services.SpecificationRelCategory.Models;
using Nop.Plugin.Baramjk.Framework.Services.SpecificationRelCategory.Models.Domains;

namespace Nop.Plugin.Baramjk.Framework.Services.SpecificationRelCategory
{
    public interface ISpecificationRelCategoryService
    {
        Task<List<SpecificationCategoryRelModel>> GetAllCategoryAsync();
        Task<List<SpecificationCategoryRelModel>> GetSpecificationCategories(int specificationId);
        Task<List<int>> GetSpecificationIdsByCategoryIdAsync(int categoryId);

        Task<List<ISpecificationRelCategoryRecord>> UpdateSpeRelCategoryAsync(int specificationId,
            IEnumerable<string> categoryNames);

        Task<List<ISpecificationRelCategoryRecord>> UpdateSpeRelCategoryAsync(int categoryId,
            IEnumerable<int> specificationIds);

        Task<List<Category>> GetCategoriesByAttributeIdAsync(int attributeId);
    }
}