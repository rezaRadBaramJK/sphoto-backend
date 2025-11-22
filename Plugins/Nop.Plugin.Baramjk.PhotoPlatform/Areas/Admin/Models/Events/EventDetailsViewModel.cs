using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events
{
    public record EventDetailsViewModel : BaseNopEntityModel, ILocalizedModel<EventDetailsLocalizedModel>
    {
        public EventDetailsViewModel()
        {
            Locales = new List<EventDetailsLocalizedModel>();
            AvailableTimeSlotLabelTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.TermsAndConditions")]
        public string TermsAndConditions { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.StartDate")]
        [UIHint("Date")]
        public DateTime StartDate { get; set; }


        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.EndDate")]
        [UIHint("Date")]
        public DateTime EndDate { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.StartTime")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.EndTime")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.TimeSlotDuration")]
        public int TimeSlotDuration { get; set; }

        public int EventId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.ProductionShare")]
        public decimal ProductionShare { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.ActorShare")]
        public decimal ActorShare { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.PhotoShootShare")]
        public decimal PhotoShootShare { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.PhotoPrice")]
        public decimal PhotoPrice { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.TimeSlotLabelTypeId")]
        public int TimeSlotLabelTypeId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.CountryIds")]
        public IList<int> CountryIds { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.Note")]
        public string Note { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.LocationUrl")]
        public string LocationUrl { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.LocationUrlTitle")]
        public string LocationUrlTitle { get; set; }

        public IList<EventDetailsLocalizedModel> Locales { get; set; }
        public List<SelectListItem> AvailableTimeSlotLabelTypes { get; set; }

        public List<SelectListItem> AvailableCountries { get; set; }
    }
}