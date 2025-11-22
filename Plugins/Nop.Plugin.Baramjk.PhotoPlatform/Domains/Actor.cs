using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class Actor : BaseEntity, ISoftDeletedEntity, ILocalizedEntity
    {
        public string Name { get; set; }
        
        public bool Deleted { get; set; }

        public decimal DefaultCameraManEachPictureCost { get; set; }

        public decimal DefaultCustomerMobileEachPictureCost { get; set; }
        
        public int CustomerId { get; set; }
        
        public string CardNumber { get; set; }
        
        public string CardHolderName { get; set; }
    }
}