using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class SearchTermAutoCompleteResponse : BaseDto
    {
        public string Label { get; set; }

        public string Producturl { get; set; }

        public string Productpictureurl { get; set; }

        public bool Showlinktoresultsearch { get; set; }
    }
}