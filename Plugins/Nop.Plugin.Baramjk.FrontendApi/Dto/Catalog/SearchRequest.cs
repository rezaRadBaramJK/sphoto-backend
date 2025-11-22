using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class SearchRequest : BaseDto
    {
        public SearchModelDto Model { get; set; }

        public CatalogProductsCommandDto Command { get; set; }
    }
}