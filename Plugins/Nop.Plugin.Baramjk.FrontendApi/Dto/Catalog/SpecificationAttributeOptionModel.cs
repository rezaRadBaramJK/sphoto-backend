namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class SpecificationAttributeOptionModel
    {
        public int Id { get; set; }

        public int SpecificationAttributeId { get; set; }

        public string Name { get; set; }

        public string ColorSquaresRgb { get; set; }

        public int DisplayOrder { get; set; }

        public string FileUrl { get; set; } = null;
    }
}