using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Product
{
    public class CompareProductsModelDto : ModelWithIdDto
    {
        public IList<ProductOverviewModelDto> Products { get; set; }

        public bool IncludeShortDescriptionInCompareProducts { get; set; }

        public bool IncludeFullDescriptionInCompareProducts { get; set; }
    }
}