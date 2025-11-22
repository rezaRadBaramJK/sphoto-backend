using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class CheckoutAttributeModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string DefaultValue { get; set; }

        public string TextPrompt { get; set; }

        public bool IsRequired { get; set; }

        public int? SelectedDay { get; set; }

        public int? SelectedMonth { get; set; }

        public int? SelectedYear { get; set; }

        public IList<string> AllowedFileExtensions { get; set; }

        public int AttributeControlType { get; set; }

        public IList<CheckoutAttributeValueModelDto> Values { get; set; }
    }
}