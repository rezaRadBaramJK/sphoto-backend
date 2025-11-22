using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Types;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models
{
    public record PhotoPlatformSettingsModel : BaseNopModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.PhotoPlatformSettingsModel.TimeSlotLabelTypeId")]
        public int TimeSlotLabelTypeId { get; set; } = (int)TimeSlotLabelType.Brief;

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.PhotoPlatformSettingsModel.ShowTicketsInProductionReports")]
        public bool ShowTicketsInProductionReports { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.PhotoPlatformSettingsModel.OrderDetailsFrontendBaseUrl")]
        public string OrderDetailsFrontendBaseUrl { get; set; }
        
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.PhotoPlatformSettingsModel.TicketSmsMessage")]
        public string TicketSmsMessage { get; set; }
        
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.PhotoPlatformSettingsModel.BirthDayRewardPoints")]
        public int BirthDayRewardPoints { get; set; }
        
        

        public List<SelectListItem> AvailableTimeSlotLabelTypes { get; set; } =
            Enum
                .GetValues<TimeSlotLabelType>()
                .Select
                (labelType => new SelectListItem
                {
                    Text = labelType.ToString(),
                    Value = ((int)labelType).ToString()
                })
                .ToList();
    }
}