using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Product
{
    public class ProductDetailsResponse : BaseDto
    {
        /// <summary>
        ///     The product template view path
        /// </summary>
        public string ProductTemplateViewPath { get; set; }

        /// <summary>
        ///     The product details model
        /// </summary>
        public ProductDetailsModelDto ProductDetailsModel { get; set; }
    }
}