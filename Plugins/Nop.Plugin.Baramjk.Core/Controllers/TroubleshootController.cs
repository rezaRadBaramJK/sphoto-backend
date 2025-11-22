using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Baramjk.Core.Controllers
{
    [Area(AreaNames.Admin)]
    public class TroubleshootController : BasePluginController
    {
        private readonly ILogger _logger;
        private readonly IPluginService _pluginService;

        public TroubleshootController(ILogger logger, IPluginService pluginService)
        {
            _logger = logger;
            _pluginService = pluginService;
        }

        public async Task<IActionResult> Troubleshoot()
        {
            var troubleshoots = EngineContext.Current.ResolveAll<ITroubleshoot>();
            var success = true;
            foreach (var troubleshoot in troubleshoots)
            {
                try
                {
                    await troubleshoot.TroubleshootAsync();
                }
                catch (Exception e)
                {
                    await _logger.ErrorAsync(e.ToString());
                    success = false;
                }
            }

            if (success)
                return Ok("ok");

            return Ok("Check Log");
        }

        [HttpPost("/Admin/BackendApi/Core/Troubleshoot/{systemName}")]
        public async Task<IActionResult> TroubleshootBySystemNameAsync(string systemName)
        {
            var pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(systemName);
            if (pluginDescriptor == null)
                return ApiResponseFactory.NotFound("plugin not found.");
            
            var troubleshootType = pluginDescriptor.ReferencedAssembly.GetTypes()
                .FirstOrDefault(t => 
                    typeof(ITroubleshoot).IsAssignableFrom(t) && 
                    t.IsInterface == false &&
                    t.IsClass && 
                    t.IsAbstract == false);

            if (troubleshootType == null)
                return ApiResponseFactory.NotFound();

            var troubleshootObject = EngineContext.Current.ResolveUnregistered(troubleshootType);

            if (troubleshootObject is not ITroubleshoot troubleshoot)
                return ApiResponseFactory.NotFound("Troubleshoot interface not found.");

            await troubleshoot.TroubleshootAsync();
            return ApiResponseFactory.Success();
        }
    }
}