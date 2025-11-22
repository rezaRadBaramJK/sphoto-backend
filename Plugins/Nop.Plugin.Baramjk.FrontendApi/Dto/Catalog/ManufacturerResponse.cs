using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class ManufacturerResponse : BaseDto
    {
        public string TemplateViewPath { get; set; }

        public ManufacturerModelDto ManufacturerModel { get; set; }
    }
}