using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events
{
    public class EventDetailsLocalizedModel: ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.TermsAndConditions")]
        public string TermsAndConditions { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.Note")]
        public string Note { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.LocationUrlTitle")]
        public string LocationUrlTitle { get; set; }
    }
}