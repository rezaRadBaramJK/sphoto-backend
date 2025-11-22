using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Models.ShoppingCart;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Product;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class MiniShoppingCartModelDto : ModelDto
    {
        public IList<MiniShoppingCartItemModelDto> Items { get; set; }

        public int TotalProducts { get; set; }

        public string SubTotal { get; set; }

        public bool DisplayShoppingCartButton { get; set; }

        public bool DisplayCheckoutButton { get; set; }

        public bool CurrentCustomerIsGuest { get; set; }

        public bool AnonymousCheckoutAllowed { get; set; }

        public bool ShowProductImages { get; set; }

        #region

        public class MiniShoppingCartItemModelDto : ModelWithIdDto
        {
            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public int Quantity { get; set; }

            public string UnitPrice { get; set; }

            public string AttributeInfo { get; set; }

            public PictureModelDto Picture { get; set; }

            public List<MiniShoppingCartItemAttributeModelDto> ProductAttributes { get; set; } =
                new List<MiniShoppingCartItemAttributeModelDto>();

        }
        
        public class MiniShoppingCartItemAttributeModelDto : ModelDto
        {
            public MiniShoppingCartItemAttributeModelDto()
            {
                CustomProperties = new Dictionary<string, object>();
                Values = new List<ShoppingCartItemAttributesValueDto>();
            }
            
            public int ProductAttributeId { get; set; }
            public string ProductAttributeName { get; set; }
            public AttributeControlType AttributeControlType { get; set; }
            public List<string> SelectedValueNames { get; set; }
            public List<int> SelectedValueIds { get; set; }
            public int SelectedValueId { get; set; }
            public string TextValue { get; set; }
            
            public IList<ShoppingCartItemAttributesValueDto> Values { get; set; }
        }

        #endregion
    }
}