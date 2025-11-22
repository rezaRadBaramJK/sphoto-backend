using System;
using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.ActorEvent;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Picture;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.TimeSlot;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Event
{
    public class EventDto : CamelCaseModelWithIdDto
    {
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public List<PictureDto> PictureModel { get; set; }

        public string TermsAndConditions { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int TimeSlotDuration { get; set; }
        
        public bool IsInWishList { get; set; }
        
        public string LocationUrl { get; set; }
        
        public string LocationUrlTitle { get; set; }
        
        public string Note { get; set; }
        
        public List<GroupedTimeSlotsDto> GroupedTimeSlots { get; set; }

        public List<ActorEventDto> ActorsDetails { get; set; }
    }
}