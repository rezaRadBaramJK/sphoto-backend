using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Extensions;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    [Route("Admin/PhotoPlatform/[controller]/[action]")]
    public class EventDetailController : BaseBaramjkPluginAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly EventDetailsService _eventDetailsService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ISettingService _settingService;

        public EventDetailController(
            IPermissionService permissionService,
            EventDetailsService eventDetailsService,
            ILocalizedEntityService localizedEntityService,
            ISettingService settingService)
        {
            _permissionService = permissionService;
            _eventDetailsService = eventDetailsService;
            _localizedEntityService = localizedEntityService;
            _settingService = settingService;
        }

        [HttpPost]
        [AuthorizeViewAccess(PermissionProvider.ManagementName)]
        public async Task<IActionResult> SubmitAsync(EventDetailsViewModel viewModel)
        {
            if (await _permissionService.AuthorizeAsync(PermissionProvider.ManagementRecord) == false)
                return Unauthorized();


            if (viewModel.StartDate > viewModel.EndDate)
                return ApiResponseFactory.BadRequest("Invalid start date.");

            if (viewModel.StartTime >= viewModel.EndTime)
                return ApiResponseFactory.BadRequest("Invalid start time.");

            if (viewModel.PhotoPrice < viewModel.ActorShare + viewModel.ProductionShare + viewModel.PhotoShootShare)
                return ApiResponseFactory.BadRequest("Total shares must not exceed photo price.");

            if (viewModel.PhotoPrice < 0 || viewModel.ActorShare < 0 || viewModel.ProductionShare < 0 || viewModel.PhotoShootShare < 0)
                return ApiResponseFactory.BadRequest("Invalid price");


            if (viewModel.CountryIds == null || viewModel.CountryIds.Count == 0)
            {
                await _eventDetailsService.DeleteEventCountryMappings(viewModel.EventId);
            }
            else
            {
                await _eventDetailsService.SubmitCountryMappingsAsync(viewModel.EventId, viewModel.CountryIds.ToList());
            }


            var eventDetails = viewModel.Map<EventDetail>();

            await _eventDetailsService.SubmitAsync(eventDetails);

            await UpdateLocalizedAsync(eventDetails, viewModel.Locales);


            var settings = await _settingService.LoadSettingAsync<PhotoPlatformSettings>();
            settings.TimeSlotLabelTypeId = viewModel.TimeSlotLabelTypeId;
            await _settingService.SaveSettingAsync(settings);
            return ApiResponseFactory.Success();
        }

        private async Task UpdateLocalizedAsync(EventDetail eventDetail, IList<EventDetailsLocalizedModel> locales)
        {
            foreach (var local in locales)
            {
                await Task.WhenAll(
                    _localizedEntityService.SaveLocalizedValueAsync(eventDetail, ed => ed.TermsAndConditions, local.TermsAndConditions,
                        local.LanguageId),
                    _localizedEntityService.SaveLocalizedValueAsync(eventDetail, ed => ed.Note, local.Note, local.LanguageId),
                    _localizedEntityService.SaveLocalizedValueAsync(eventDetail, ed => ed.LocationUrlTitle, local.LocationUrlTitle,
                        local.LanguageId));
            }
        }
    }
}