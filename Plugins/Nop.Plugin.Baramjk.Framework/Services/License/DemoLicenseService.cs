using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Plugin.Baramjk.Framework.Services.License.Models.DemoLicenseServices;
using Nop.Plugin.Baramjk.Framework.Services.Networks;

namespace Nop.Plugin.Baramjk.Framework.Services.License
{
    public class DemoLicenseService : IDemoLicenseService
    {
        private const string URL = "https://localhost:5001/LicenseApi/CreateDemoPluginLicense";

        private readonly HttpClientHelper _httpClientHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILicenseManagerService _licenseManagerService;

        public DemoLicenseService(HttpClientHelper httpClientHelper,
            IHttpContextAccessor httpContextAccessor, ILicenseManagerService licenseManagerService)
        {
            _httpClientHelper = httpClientHelper;
            _httpContextAccessor = httpContextAccessor;
            _licenseManagerService = licenseManagerService;
        }

        public async Task<HttpResponse<DemoLicenseResponse>> CreateDemoPluginLicenseAsync(string pluginName)
        {
            var requestHost = _httpContextAccessor.HttpContext.Request.Host.Host;
            var request = new DemoLicenseRequest
            {
                PluginName = pluginName,
                Domain = requestHost
            };

            var result = await _httpClientHelper.PostJsonAsync<DemoLicenseRequest, DemoLicenseResponse>(URL, request);
            if (result.IsSuccessStatusCode == false)
                return result;

            await _licenseManagerService.SaveLicense(pluginName, result.Body.Data);
            return result;
        }
    }
}