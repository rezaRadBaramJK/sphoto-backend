using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Core.Models;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.BaramjkPlugins;
using Nop.Plugin.Baramjk.Framework.Services.License;
using Nop.Plugin.Baramjk.Framework.Services.Ui.DataTables;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.Core.Controllers
{
    //[AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class BaramjkPluginController : BaseBaramjkPluginController
    {
        private readonly IBaramjkPluginService _baramjkPluginService;
        private readonly IDemoLicenseService _demoLicenseService;
        private readonly INotificationService _notificationService;
        protected readonly IDataTablesBuilders _dataTablesBuilders;

        public BaramjkPluginController(INotificationService notificationService,
            IBaramjkPluginService baramjkPluginService, IDemoLicenseService demoLicenseService,
            IDataTablesBuilders dataTablesBuilders)
        {
            _notificationService = notificationService;
            _baramjkPluginService = baramjkPluginService;
            _demoLicenseService = demoLicenseService;
            _dataTablesBuilders = dataTablesBuilders;
        }

        [HttpGet("/Admin/BaramjkPlugin/CreateDemoLicense/{pluginName}")]
        public async Task<IActionResult> CreateDemoLicense(string pluginName)
        {
            var result = await _demoLicenseService.CreateDemoPluginLicenseAsync(pluginName);
            if (result.IsSuccessStatusCode)
                _notificationService.SuccessNotification("License is Created");
            else
                _notificationService.ErrorNotification(result?.Body?.Message ?? "License is not Created");

            return RedirectToAction("List");
        }

        [HttpGet]
        public async Task<IActionResult> ListAsync()
        {
            SetActiveMenuByMenuSystemName($"{MenuUtils.BaramjkMenuSystemName}_Plugins_Information");
            var viewModel = new DefaultListViewModel("BaramjkPlugin", SystemName, typeof(PluginDescriptorModel))
            {
                Tools = new List<ToolsItem>()
                {
                    new LinkButton("Troubleshoot")
                    {
                        Href = "/admin/Troubleshoot/Troubleshoot",
                        ClassName = "btn btn-info",
                        Icon = "fas fa-cogs",
                        Attributes = "target='_parent'"
                    }
                },
                JsPath = "~/Plugins/Baramjk.Core/Content/Troubleshoot.js",
                DeleteAction = null,
                UpdateAction = null,
                AddAction = null,
            };

            viewModel.DataTablesModel = await _dataTablesBuilders.BuildDataTablesModelAsync(viewModel);
            return ViewBase(viewModel.ViewFullPath, viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ListAsync(ExtendedSearchModel searchModel)
        {
            var plugins = await _baramjkPluginService.GetBaramjkPluginDescriptorsAsync(LoadPluginsMode.All);
            var pluginDescriptorModels = MapUtils.Map<List<PluginDescriptorModel>>(plugins);
            pluginDescriptorModels.AutoIncrement();

            var model = new DataTableRecordsModel<PluginDescriptorModel>()
                .PrepareToGrid(pluginDescriptorModels, searchModel.NextDraw, plugins.Count);

            return Json(model);
        }
    }
}