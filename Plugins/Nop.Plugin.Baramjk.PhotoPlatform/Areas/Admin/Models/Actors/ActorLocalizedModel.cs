using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Actors
{
    public class ActorLocalizedModel: ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }
        
        public string Name { get; set; }
    }
}