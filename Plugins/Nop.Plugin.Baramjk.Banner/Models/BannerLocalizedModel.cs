using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Banner.Models
{
    public class BannerLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }
        
        public string Title { get; set; }
        public string Text { get; set; }
        
    }
}