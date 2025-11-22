using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Framework.Models.Pictures
{
    public record DefaultPictureDto
    {
        public DefaultPictureDto()
        {
            CustomProperties = new Dictionary<string, object>();
        }
        
        public int Id { get; set; }
        
        public string ImageUrl { get; set; }

        public string ThumbImageUrl { get; set; }

        public string FullSizeImageUrl { get; set; }

        public string Title { get; set; }

        public string AlternateText { get; set; }
        
        public Dictionary<string, object> CustomProperties { get; set; }
    }
}