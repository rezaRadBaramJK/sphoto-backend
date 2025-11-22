using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Customer
{
    public class CustomerAttributeModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>
        ///     Default value
        /// </summary>
        public string DefaultValue { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<CustomerAttributeValueModelDto> Values { get; set; }
    }
}