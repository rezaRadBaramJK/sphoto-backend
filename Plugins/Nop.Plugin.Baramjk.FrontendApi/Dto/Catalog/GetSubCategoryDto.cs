using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class GetSubCategoryDto
    {
        public CategoryDto Category { get; set; }

        public List<CategoryDto> SubCategories { get; set; }
    }
}