using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Services.License;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

#pragma warning disable CS1998

namespace Nop.Plugin.Baramjk.Core.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class LicensePanelController : BasePluginController
    {
        private readonly ILicenseManagerService _licenseManagerService;
        private readonly ILicenseParser _licenseParser;

        public LicensePanelController(ILicenseManagerService licenseManagerService, ILicenseParser licenseParser)
        {
            _licenseManagerService = licenseManagerService;
            _licenseParser = licenseParser;
        }

        public IActionResult AddLicense()
        {
            return View("~/Plugins/Baramjk.Core/Views/LicensePanel/Add.cshtml", new AddLicenseModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddLicense(AddLicenseModel model)
        {
            var pluginLicense = _licenseParser.Pars(model.Token);
            foreach (var pluginName in pluginLicense.Plugins)
                await _licenseManagerService.SaveLicense(pluginName, pluginLicense);

            return RedirectToAction("List");
        }

        public async Task<IActionResult> List(string pluginName)
        {
            ViewBag.PluginName = pluginName;
            var licenses = _licenseManagerService.GetPluginLicenses();
            return View("~/Plugins/Baramjk.Core/Views/LicensePanel/List.cshtml", licenses);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _licenseManagerService.Delete(id);
            return RedirectToAction("List");
        }
    }

    public class AddLicenseModel
    {
        public string Token { get; set; }
    }
}