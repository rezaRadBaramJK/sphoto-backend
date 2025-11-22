using System;
using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Baramjk.Banner.Domain
{
    public class BannerRecord : BaseEntity, ILocalizedEntity
    {
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
        public string AltText { get; set; }
        public string Tag { get; set; }
        public int DisplayOrder { get; set; }
        public BannerType BannerType { get; set; }
        
        public DateTime? ExpirationDateTime { get; set; }
    }
    
}