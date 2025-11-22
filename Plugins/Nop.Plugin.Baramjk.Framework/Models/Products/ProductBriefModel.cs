using System;
using System.Collections.Generic;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.Framework.Models.Products
{
    public class ProductBriefModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? AvailableStartDateTimeUtc { get; set; }
        public DateTime? AvailableEndDateTimeUtc { get; set; }
        public int VendorId { get; set; }
        public ProductOverviewModel.ProductPriceModel ProductPrice { get; set; }
        public PictureModel PictureModel { get; set; }
        public Dictionary<string, object> CustomProperty { get; set; } = new();
    }
}