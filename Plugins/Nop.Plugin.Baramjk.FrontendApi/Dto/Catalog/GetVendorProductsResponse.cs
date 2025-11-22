using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class GetVendorProductsResponse : BaseDto
    {
        public string TemplateViewPath { get; set; }

        public CatalogProductsModelDto CatalogProductsModel { get; set; }
    }
}