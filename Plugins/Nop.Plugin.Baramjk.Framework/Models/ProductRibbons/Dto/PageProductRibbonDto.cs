using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.Framework.Models.Pictures;

namespace Nop.Plugin.Baramjk.Framework.Models.ProductRibbons.Dto
{
    public class PageProductRibbonDto: CamelCaseModelWithIdDto
    {
        
        public PageProductRibbonDto()
        {
            CustomProperties = new Dictionary<string, object>();
        }
        
        public bool Enabled { get; set; }

        public string Name { get; set; }

        public string Position { get; set; }

        public string TextStyle { get; set; }

        public string ImageStyle { get; set; }

        public string ContainerStyle { get; set; }

        public int ProductId { get; set; }

        public int ProductRibbonId { get; set; }

        public int? PictureId { get; set; }

        public string ProductSeName { get; set; }

        public string Text { get; set; }
        
        public DefaultPictureDto Picture { get; set; }
    }
}