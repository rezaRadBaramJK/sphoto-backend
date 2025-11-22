using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.CashierEvents
{
    public record CashierEventsItemModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.EventId")]
        public int EventId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.CustomerId")]
        public int CustomerId { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.CashierEmail")]
        public string CashierEmail { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.CashierPictureUrl")]
        public string CashierPictureUrl { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.CommissionAmount")]
        public decimal CommissionAmount { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.OpeningFundBalanceAmount")]
        public decimal OpeningFundBalanceAmount { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.Active")]
        public bool Active { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.IsRefundPermitted")]
        public bool IsRefundPermitted { get; set; }
        
        
    }
}