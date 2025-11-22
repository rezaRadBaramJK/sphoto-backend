using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class CustomerProductReviewsModelDto : ModelDto
    {
        public IList<CustomerProductReviewModelDto> ProductReviews { get; set; }

        public PagerModelDto PagerModel { get; set; }
    }
}