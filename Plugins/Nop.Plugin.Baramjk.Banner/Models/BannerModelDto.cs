using System;
using Nop.Plugin.Baramjk.Banner.Controllers;
using Nop.Plugin.Baramjk.Banner.Domain;
using Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments;

namespace Nop.Plugin.Baramjk.Banner.Models
{
    public class BannerModelDto : IEntityAttachmentModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
        public string AltText { get; set; }
        public string Tag { get; set; }
        public int DisplayOrder { get; set; }
        public AttachmentType AttachmentType => (AttachmentType)BannerType;
        public BannerType BannerType { get; set; }
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public string FileUrl { get; set; }
        public string DownloadUrl => FileUrl;
        
        public DateTime? ExpirationDateTime { get; set; }
        // public GifMetaData? MetaData { get; set; }
    }
}