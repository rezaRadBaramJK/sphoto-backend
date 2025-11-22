using System.ComponentModel.DataAnnotations;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.SocialLinks.Domain
{
    public class SocialLink : BaseEntity
    {
        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.Name"))]

        public string Name { get; set; }

        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.Priority"))]

        public int Priority { get; set; }

        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.Link"))]

        public string? Link { get; set; }

        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.Image"))]
        [UIHint("Picture")]
        public int ImageId { get; set; }
        
        
        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.ShowInFooter"))]
        public bool ShowInFooter { get; set; }
        
        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.ShowInWidget"))]
        public bool ShowInWidget { get; set; }
        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.ShowInProductDetails"))]
        public bool ShowInProductDetails { get; set; }
        public SocialLinkCategory Category { get; set; }
        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.SocialSharePrefix"))]

        public string? SocialSharePrefix { get; set; }
    }

    public record SocialLinkDto : BaseNopEntityModel
    {
        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.Name"))]

        public string Name { get; set; }

        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.Priority"))]

        public int Priority { get; set; }

        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.Link"))]

        public string? Link { get; set; }

        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.Image"))]
        [UIHint("Picture")]
        public int ImageId { get; set; }
        
        
        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.ShowInFooter"))]
        public bool ShowInFooter { get; set; }
        
        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.ShowInWidget"))]
        public bool ShowInWidget { get; set; }
        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.ShowInProductDetails"))]
        public bool ShowInProductDetails { get; set; }
        public SocialLinkCategory Category { get; set; }
        [NopResourceDisplayName(("Admin.Plugins.SocialLinks.SocialLink.SocialSharePrefix"))]

        public string? SocialSharePrefix { get; set; }
        
        public string ImageUrl { get; set; }
    }
    
    public record SocialLinkList : DataTableRecordsModel<SocialLinkDto>
    {
        
    }

    public class SocialLinkSearchModel : ExtendedSearchModel
    {
        public string Name { get; set; }
        public int Priority { get; set; }

    }
}