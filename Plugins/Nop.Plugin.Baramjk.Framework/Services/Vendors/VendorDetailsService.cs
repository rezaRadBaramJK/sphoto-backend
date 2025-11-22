using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Domain.Vendors;
using Nop.Plugin.Baramjk.Framework.Services.Vendors.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.Vendors
{
    public class VendorDetailsService
    {
        private readonly IRepository<VendorDetail> _vendorDetailRepository;

        public VendorDetailsService(
            IRepository<VendorDetail> vendorDetailsRepository)
        {
            _vendorDetailRepository = vendorDetailsRepository;
        }
        
        public async Task SubmitVendorDetailsAsync(SubmitVendorDetailsParams submitParams)
        {
            var vendorDetail = await GetDetailsByVendorIdAsync(submitParams.Vendor.Id);
            if (vendorDetail == null)
            {
                vendorDetail = new VendorDetail
                {
                    VendorId = submitParams.Vendor.Id,
                    StartTime = submitParams.StartTime,
                    EndTime = submitParams.EndTime,
                    OffDaysOfWeekIds = string.Join(",", submitParams.OffDaysOfWeekIds),
                    IsAvailable = submitParams.IsAvailable
                };

                await _vendorDetailRepository.InsertAsync(vendorDetail);
                return;
            }
            
            vendorDetail.StartTime = submitParams.StartTime;
            vendorDetail.EndTime = submitParams.EndTime;
            vendorDetail.OffDaysOfWeekIds = string.Join(",", submitParams.OffDaysOfWeekIds);
            vendorDetail.IsAvailable = submitParams.IsAvailable;
            await _vendorDetailRepository.UpdateAsync(vendorDetail);
        }

        public Task<VendorDetail> GetDetailsByVendorIdAsync(int vendorId)
        {
            return _vendorDetailRepository.Table.FirstOrDefaultAsync(vd => vd.VendorId == vendorId);
        }
        
        public Task DeleteByVendorIdAsync(int vendorId)
        {
            return _vendorDetailRepository.DeleteAsync(detail => detail.VendorId == vendorId);
        }
    }
}