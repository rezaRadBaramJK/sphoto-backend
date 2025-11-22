using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Catalog;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class ConditionAttributeService
    {
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;

        public ConditionAttributeService(IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser)
        {
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
        }

        public async Task<List<ConditionAttribute>> GetConditionAttributeAsync(int productId)
        {
            var conditionAttributes = new List<ConditionAttribute>();

            var mappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);

            foreach (var mapping in mappings)
            {
                if (string.IsNullOrEmpty(mapping.ConditionAttributeXml))
                    continue;

                var items =
                    await _productAttributeParser.ParseProductAttributeValuesAsync(mapping.ConditionAttributeXml);

                var productAttributeValue = items.FirstOrDefault();
                if (productAttributeValue == null)
                    continue;

                var conditionAttribute = new ConditionAttribute
                {
                    ProductAttributeMappingId = mapping.Id,
                    DependToOptionId = productAttributeValue.Id,
                    DependToProductAttributeMappingId = productAttributeValue.ProductAttributeMappingId
                };

                conditionAttributes.Add(conditionAttribute);
            }

            return conditionAttributes;
        }
    }

    public class ConditionAttribute
    {
        public int ProductAttributeMappingId { get; set; }
        public int DependToProductAttributeMappingId { get; set; }
        public int DependToOptionId { get; set; }
    }
}