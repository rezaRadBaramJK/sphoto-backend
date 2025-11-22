using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.Framework.Models.Products
{
    public class ProductDetailsAttributeModelDto: ModelWithIdDto
    {
        public int ProductId { get; set; }

        public int ProductAttributeId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string TextPrompt { get; set; }

        public bool IsRequired { get; set; }
        
        public string DefaultValue { get; set; }
        
        public int? SelectedDay { get; set; }
        
        public int? SelectedMonth { get; set; }
        
        public int? SelectedYear { get; set; }
        
        public bool HasCondition { get; set; }
        
        public IList<string> AllowedFileExtensions { get; set; }

        public int AttributeControlType { get; set; }

        public IList<ProductAttributeValueModelDto> Values { get; set; }
    }
}