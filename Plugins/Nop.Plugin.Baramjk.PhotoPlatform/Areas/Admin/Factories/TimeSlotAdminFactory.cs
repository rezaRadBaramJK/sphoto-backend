using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.TimeSlots;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Extensions;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories
{
    public class TimeSlotAdminFactory
    {
        private readonly TimeSlotService _timeSlotService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;

        public TimeSlotAdminFactory(TimeSlotService timeSlotService,
            ILanguageService languageService,
            ILocalizationService localizationService)
        {
            _timeSlotService = timeSlotService;
            _languageService = languageService;
            _localizationService = localizationService;
        }

        public EventTimeSlotsViewModel PrepareEventTimeSlotsViewModel(int eventId)
        {
            var viewModel = new EventTimeSlotsViewModel
            {
                EventId = eventId
            };
            viewModel.SetGridPageSize();

            return viewModel;
        }

        public async Task<TimeSlotViewModel> PrepareTimeSlotViewModel(int eventId)
        {
            var languages = await _languageService.GetAllLanguagesAsync();
            return new TimeSlotViewModel
            {
                Active = true,
                Date = DateTime.Today.Date,
                EventId = eventId,
                Locales = languages
                    .Select(lang => new TimeSlotLocalizedModel
                    {
                        LanguageId = lang.Id,
                    })
                    .ToList()
            };
        }

        public async Task<TimeSlotViewModel> PrepareTimeSlotViewModel(TimeSlot timeSlot)
        {
            var languages = await _languageService.GetAllLanguagesAsync();
            var viewModel = timeSlot.Map<TimeSlotViewModel>();
            viewModel.Locales = await languages
                .SelectAwait(async lang => new TimeSlotLocalizedModel
                {
                    LanguageId = lang.Id,
                    Note = await _localizationService.GetLocalizedAsync(timeSlot, t => t.Note, lang.Id, false, false),
                })
                .ToListAsync();

            return viewModel;
        }

        public async Task<TimeSlotListModel> PrepareTimeSlotListModelAsync(EventTimeSlotsViewModel viewModel)
        {
            var pageIndex = viewModel.Page - 1;
            var timeSlots = await _timeSlotService.GetByEventIdAsync(viewModel.EventId, pageIndex, viewModel.PageSize);

            return await new TimeSlotListModel().PrepareToGridAsync(viewModel, timeSlots, () =>
            {
                return timeSlots.SelectAwait(async ts => new TimeSlotItemModel
                {
                    Id = ts.Id,
                    Date = ts.Date.ToString("MM/dd/yyyy"),
                    StartTime = ts.Date.Add(ts.StartTime).ToString("hh:mm tt"),
                    EndTime = ts.EndTime.HasValue ? ts.Date.Add(ts.EndTime.Value).ToString("hh:mm tt") : string.Empty,
                    Active = ts.Active,
                    Note = await _localizationService.GetLocalizedAsync(ts, t => t.Note),
                });
            });
        }
    }
}