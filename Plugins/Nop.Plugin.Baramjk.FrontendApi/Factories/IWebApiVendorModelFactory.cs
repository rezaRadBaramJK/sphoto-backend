using System.Threading.Tasks;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Vendors;

namespace Nop.Plugin.Baramjk.FrontendApi.Factories
{
    public interface IWebApiVendorModelFactory
    {
        Task<VendorInfoModelDto> PrepareVendorInfoModelAsync(VendorInfoModelDto model, bool excludeProperties, string overriddenVendorAttributesXml = "");
    }
}