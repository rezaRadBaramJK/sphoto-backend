using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Product
{
    public class SetProductReviewHelpfulnessResponse : BaseDto
    {
        public string Result { get; set; }

        public int TotalYes { get; set; }

        public int TotalNo { get; set; }
    }
}