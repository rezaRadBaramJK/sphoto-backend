using System.Linq;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Services.CleanUps.Abstractions;

namespace Nop.Plugin.Baramjk.Framework.Services.CleanUps.NopEntities
{
    public class VendorNopCleanup : INopCleanup
    {
        private readonly IRepository<Vendor> _vendorRepository;

        public VendorNopCleanup(IRepository<Vendor> vendorRepository)
        {
            _vendorRepository = vendorRepository;
        }

        public IQueryable<int> GetPictureIdsQuery()
        {
            return _vendorRepository.Table
                .Where(v => v.Deleted == false && v.PictureId != 0)
                .Select(v => v.PictureId);
        }
    }
}