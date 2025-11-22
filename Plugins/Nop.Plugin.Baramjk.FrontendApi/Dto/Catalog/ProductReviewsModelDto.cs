using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class ProductReviewsModelDto : ModelDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public IList<ProductReviewModelDto> Items { get; set; }

        public AddProductReviewModelDto AddProductReview { get; set; }

        public IList<ReviewTypeModelDto> ReviewTypeList { get; set; }

        public IList<AddProductReviewReviewTypeMappingModelDto> AddAdditionalProductReviewList { get; set; }
    }
}