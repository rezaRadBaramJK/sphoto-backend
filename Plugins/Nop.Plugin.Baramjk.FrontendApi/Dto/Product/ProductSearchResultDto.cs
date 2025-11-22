using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Models.Products;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Product
{
    public class ProductSearchResultDto : ProductListDto
    {
        public ProductSearchResultDto(IList<ProductOverviewDto> source, int pageIndex, int pageSize,
            int? totalCount = null)
            : base(source, pageIndex, pageSize, totalCount)
        {
        }

        public List<CategoryItemModel> Category { get; set; }
    }
}