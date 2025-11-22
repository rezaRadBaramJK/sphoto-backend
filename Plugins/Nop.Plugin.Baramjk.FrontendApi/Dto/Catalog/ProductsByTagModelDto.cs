using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class ProductsByTagModelDto : ModelWithIdDto
    {
        public string TagName { get; set; }

        public string TagSeName { get; set; }

        public CatalogProductsModelDto CatalogProductsModel { get; set; }
    }
}