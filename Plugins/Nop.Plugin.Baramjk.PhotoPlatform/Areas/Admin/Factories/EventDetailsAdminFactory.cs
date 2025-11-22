using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Types;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Enum = System.Enum;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories
{
    public class EventDetailsAdminFactory
    {
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly ICountryService _countryService;

        public EventDetailsAdminFactory(
            ILanguageService languageService,
            ILocalizationService localizationService,
            ISettingService settingService,
            ICountryService countryService)
        {
            _languageService = languageService;
            _localizationService = localizationService;
            _settingService = settingService;
            _countryService = countryService;
        }


        private async Task<List<SelectListItem>> PrepareCountriesAsync()
        {
            var countries = await _countryService.GetAllCountriesAsync();


            return countries.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }).ToList();
        }

        public async Task<EventDetailsViewModel> PrepareDetailsViewModelAsync(EventDetail eventDetail, int eventId, List<Country> eventCountries)
        {
            var languages = await _languageService.GetAllLanguagesAsync();
            if (eventDetail == null)
                return new EventDetailsViewModel
                {
                    EventId = eventId,
                    Locales = languages
                        .Select(lang => new EventDetailsLocalizedModel
                        {
                            LanguageId = lang.Id,
                        })
                        .ToList(),
                    TimeSlotLabelTypeId = (await _settingService.LoadSettingAsync<PhotoPlatformSettings>()).TimeSlotLabelTypeId,
                    AvailableTimeSlotLabelTypes =
                        Enum.GetValues<TimeSlotLabelType>()
                            .Select(e => new SelectListItem
                            {
                                Text = e.ToString(),
                                Value = ((int)e).ToString()
                            })
                            .ToList(),
                    AvailableCountries = await PrepareCountriesAsync()
                };


            var viewModel = new EventDetailsViewModel
            {
                Id = eventDetail.Id,
                TermsAndConditions = eventDetail.TermsAndConditions,
                Note = eventDetail.Note,
                LocationUrl = eventDetail.LocationUrl,
                LocationUrlTitle = eventDetail.LocationUrlTitle,
                StartDate = eventDetail.StartDate,
                EndDate = eventDetail.EndDate,
                StartTime = eventDetail.StartTime,
                EndTime = eventDetail.EndTime,
                EventId = eventDetail.EventId,
                ActorShare = eventDetail.ActorShare,
                ProductionShare = eventDetail.ProductionShare,
                PhotoShootShare = eventDetail.PhotoShootShare,
                PhotoPrice = eventDetail.PhotoPrice,
                TimeSlotDuration = eventDetail.TimeSlotDuration,
                TimeSlotLabelTypeId = (await _settingService.LoadSettingAsync<PhotoPlatformSettings>()).TimeSlotLabelTypeId,
                CountryIds = eventCountries.Select(x => x.Id).ToList(),
                AvailableTimeSlotLabelTypes =
                    Enum.GetValues<TimeSlotLabelType>()
                        .Select(e => new SelectListItem
                        {
                            Text = e.ToString(),
                            Value = ((int)e).ToString()
                        })
                        .ToList(),
                Locales = await languages
                    .SelectAwait(async lang => new EventDetailsLocalizedModel
                    {
                        LanguageId = lang.Id,
                        TermsAndConditions =
                            await _localizationService.GetLocalizedAsync(eventDetail, e => e.TermsAndConditions, lang.Id, false, false),
                        Note = await _localizationService.GetLocalizedAsync(eventDetail, e => e.Note, lang.Id, false, false),
                        LocationUrlTitle = await _localizationService.GetLocalizedAsync(eventDetail, e => e.LocationUrlTitle, lang.Id, false, false),
                    })
                    .ToListAsync(),
                AvailableCountries = await PrepareCountriesAsync()
            };


            return viewModel;
        }
    }
}