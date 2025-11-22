using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Abstractions
{
    public interface ITechnicianSpecificationAttributeService
    {
        Task<List<ProductSpecificationResults>> GetProductSpecificationAsync(int productId);
    }
}