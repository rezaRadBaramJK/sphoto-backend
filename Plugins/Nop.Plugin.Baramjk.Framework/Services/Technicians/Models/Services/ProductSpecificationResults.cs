using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services
{
    public class ProductSpecificationResults
    {
        public SpecificationAttribute Attribute { get; set; }
        
        public SpecificationAttributeOption Option { get; set; }
        
        public ProductSpecificationAttribute ProductSpecificationAttribute { get; set; }
        
    }
}