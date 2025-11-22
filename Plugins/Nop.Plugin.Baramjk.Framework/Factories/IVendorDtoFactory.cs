using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Baramjk.Framework.Models.Vendors;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Baramjk.Framework.Factories
{
    public interface IVendorDtoFactory
    {
        Task<VendorDto> PrepareVendorDtoAsync(int id, bool attributes = true, bool favCount = true,
            bool rating = true, bool genericAttribute = true,bool checkFavourite = true);

        Task<VendorDto> PrepareVendorDtoAsync(Vendor vendor, bool attributes = true, bool favCount = true,
            bool rating = true, bool genericAttribute = true,bool checkFavourite = true);

        Task<List<VendorDto>> PrepareVendorDtoAsync(IEnumerable<Vendor> vendors, bool attributes = true,
            bool favCount = true, bool rating = true, bool genericAttribute = true, bool checkFavourite = true);

        Task<VendorBriefInfoModel> VendorBriefInfoModelAsync(int id);
    }
}