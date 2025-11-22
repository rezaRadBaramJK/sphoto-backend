using System.Threading.Tasks;
using Nop.Plugin.Baramjk.FrontendApi.Services.Models.CustomerVendor;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public interface IVendorRegisterService
    {
        Task<AddVendorResponse> AddVendorAsync(AddVendorModel model);
        Task<AddVendorResponse> UpdateCustomerToVendorAsync(UpdateCustomerToVendorModel model);
    }
}