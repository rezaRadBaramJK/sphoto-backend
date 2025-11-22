using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ShoppingCart
{
    public class SubmitShoppingCartItemApiModel
    {
        public SubmitShoppingCartItemApiModel(List<PhotographyDetailApiModel> photographyDetails)
        {
            PhotographyDetails = photographyDetails;
        }

        public int TimeSlotId { get; set; }
        public int EventId { get; set; }
        public List<PhotographyDetailApiModel> PhotographyDetails { get; set; }
    }
}