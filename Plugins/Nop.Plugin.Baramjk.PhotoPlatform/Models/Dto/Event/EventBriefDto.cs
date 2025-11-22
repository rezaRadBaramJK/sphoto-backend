using System;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Picture;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Event
{
    public class EventBriefDto : CamelCaseModelWithIdDto
    {
        public string Name { get; set; }

        public string Description { get; set; }
        
        public PictureDto PictureModel { get; set; }

        public string TermsAndConditions { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int TimeSlotDuration { get; set; }
        
        public bool IsInWishList { get; set; }
    }
}