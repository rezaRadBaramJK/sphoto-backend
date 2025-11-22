using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Product
{
    public class ProductBreadcrumbModelDto : ModelDto
    {
        public bool Enabled { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public IList<CategorySimpleModelDto> CategoryBreadcrumb { get; set; }
    }
}