using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.Models.AddProductServices
{
    public class ProductDataModel
    {
        public int[] PictureIds { get; set; }
        public int[] CategoryIds { get; set; }
        public string ProductName { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerQuantity { get; set; }
        public int? BrandId { get; set; }
        public string Sku { get; set; }
        public DateTime? AvailableStartDateTimeUtc { get; set; }
        public DateTime? AvailableEndDateTimeUtc { get; set; }
        
        public string[] Tags { get; set; }
        public List<KeyValuePair<string, string>> GenericAttributes { get; set; }
        public List<ProductSpecificationAttribute> SpecificationAttributes { get; set; }
        public List<ProductAttributeMappingModel> Attributes { get; set; }
    }
}

