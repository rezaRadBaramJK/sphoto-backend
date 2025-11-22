using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Baramjk.FrontendApi.Models.Orders
{
    public class ReOrderServiceResults
    {
        public bool Success { get; set; }

        public List<Product> FailedProducts { get; set; } = new();
        
        public List<Product> AddedProducts { get; set; } = new();
    }
}