using System;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Banners
{
    public class BannerDto: CamelCaseModelWithIdDto
    {
        public string EntityName { get; set; }
        
        public int EntityId { get; set; }
        
        public string FileName { get; set; }
        
        public string Title { get; set; }
        
        public string Text { get; set; }
        
        public string Link { get; set; }
        
        public string AltText { get; set; }
        
        public string Tag { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public AttachmentType AttachmentType { get; set; }
        
        public string DownloadUrl { get; set; }
        
        public DateTime? ExpirationDateTime { get; set; }
        
    }
}