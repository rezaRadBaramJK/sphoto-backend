using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Services.SpecificationRelCategory.Models.Domains;

namespace Nop.Plugin.Baramjk.Framework.Services.SpecificationRelCategory
{
    public interface ISpecificationOptionRelCategoryService
    {
        Task<List<ISpecificationOptionRelCategoryRecord>> GeRecordsByCategoryIdAsync(int categoryId);
        Task<List<int>> GetSpecificationOptionIdsByCategoryIdAsync(int categoryId);

        Task<List<ISpecificationOptionRelCategoryRecord>> UpdateOptionRelCategoryAsync(int categoryId,
            List<ISpecificationOptionRelCategoryRecord> records);

        Task<List<Category>> GetCategoriesByOptionIdAsync(int optionId);
    }
}