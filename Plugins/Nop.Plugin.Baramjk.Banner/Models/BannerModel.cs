using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Baramjk.Banner.Domain;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Banner.Models
{
    public class BannerModel :
        ILocalizedModel<BannerLocalizedModel>,
        IEntityAttachmentModel,
        IDomainModel
    {
        public int Id { get; set; }

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.Type")]
        public BannerType BannerType { get; set; }

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.FileUrl")]
        public string FileUrl { get; set; }

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.FileName")]
        public string FileName { get; set; }

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.Text")]
        public string Text { get; set; }

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.Link")]
        public string Link { get; set; }

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.AltText")]
        public string AltText { get; set; }

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.Tag")]
        public string Tag { get; set; }

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.EntityId")]
        public int EntityId { get; set; }

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.EntityName")]
        public string EntityName { get; set; }

        public AttachmentType AttachmentType => (AttachmentType)BannerType;

        public string DownloadUrl => FileUrl;

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.Picture")]
        [UIHint("Picture")]
        public int FileId { get; set; }

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.Url")]
        public string Url { get; set; }

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.ExpirationDateTime")]
        [UIHint("DateTimeNullable")]
        public DateTime? ExpirationDateTime { get; set; }

        public IList<BannerLocalizedModel> Locales { get; set; } = new List<BannerLocalizedModel>();

        [NopResourceDisplayName("Admin.Plugins.Banner.Fields.AvailableTypes")]
        public IList<SelectListItem> AvailableTypes { get; set; }
    }
}