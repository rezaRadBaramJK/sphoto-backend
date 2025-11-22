using System.ComponentModel;
using Nop.Plugin.Baramjk.Banner.Domain;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Ui.DataTables;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Banner.Models
{
    public record BannerListItemModel : BaseNopEntityModel, IDomainModel
    {
        public int Id { get; set; }

        [TableField(Visible = false)]
        public BannerType BannerType { get; set; }

        [DisplayName("Banner type")]
        public string BannerTypeName => BannerType.ToString();
        
        [TableField(RenderCustom = "RenderFile",Title = "File")]
        public string FileName { get; set; }

        public string Title { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }

        [DisplayName("Alt Text")]
        public string AltText { get; set; }

        public string Tag { get; set; }

        [DisplayName("Display Order")]
        public int DisplayOrder { get; set; }

        public int? EntityId { get; set; }
        public string EntityName { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Banner.Admin.Banner.ExpirationDateTime")]
        public string ExpirationDateTime { get; set; }
    }
}