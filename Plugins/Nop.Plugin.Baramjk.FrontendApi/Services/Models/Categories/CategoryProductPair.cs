using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.Models.Categories
{
    public class CategoryProductPair
    {
        public Category Category { get; set; }

        public IList<Product> Products { get; set; } 
    }
}