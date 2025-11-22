using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Baramjk.ContactUs.Domains
{
    public class SubjectEntity : BaseEntity, ISoftDeletedEntity, ILocalizedEntity
    {
        public string Name { get; set; }
        
        public decimal Price { get; set; }

        public bool Deleted { get; set; }
    }
}