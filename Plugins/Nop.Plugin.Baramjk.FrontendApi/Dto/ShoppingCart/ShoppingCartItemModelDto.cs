using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Models.BuyXGetY.Abstractions;
using Nop.Plugin.Baramjk.Framework.Models.BuyXGetY.Dto;
using Nop.Plugin.Baramjk.Framework.Models.Categories;
using Nop.Plugin.Baramjk.Framework.Models.ShoppingCart;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Product;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class ShoppingCartItemModelDto : ModelWithIdDto, IBuyXGetYBase
    {
        public string Sku { get; set; }

        public VendorBriefInfoModel Vendor { get; set; }

        public PictureModelDto Picture { get; set; }
        
        public IList<CategoryItemDto> Categories { get; set; } = new List<CategoryItemDto>();

        public BuyXGetYDto BuyXGetY { get; set; } = new();
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public string UnitPrice { get; set; }

        public decimal OldPrice { get; set; } = 0;

        public decimal DiscountPercentage { get; set; } = 0;
        
        

        public string SubTotal { get; set; }

        public string Discount { get; set; }

        public int? MaximumDiscountedQty { get; set; }

        public int Quantity { get; set; }

        public List<SelectListItemDto> AllowedQuantities { get; set; }

        public string AttributeInfo { get; set; }

        public string RecurringInfo { get; set; }

        public string RentalInfo { get; set; }

        public bool AllowItemEditing { get; set; }

        public bool DisableRemoval { get; set; }

        public List<ShoppingCartItemAttributesModelDto> ProductAttributes = new();
        
        public IList<string> Warnings { get; set; }
    }
        
    public class ShoppingCartItemAttributesModelDto : ModelDto
    {
        public ShoppingCartItemAttributesModelDto()
        {
            CustomProperties = new Dictionary<string, object>();
            Values = new List<ShoppingCartItemAttributesValueDto>();
        }
        
        public int ProductAttributeId { get; set; }
        public string ProductAttributeName { get; set; }
        public AttributeControlType AttributeControlType { get; set; }
        public List<int> SelectedValueIds { get; set; }
        public List<string> SelectedValueNames { get; set; }
        public int SelectedValueId { get; set; }
        public string TextValue { get; set; }
        
        public IList<ShoppingCartItemAttributesValueDto> Values { get; set; }
    }
}