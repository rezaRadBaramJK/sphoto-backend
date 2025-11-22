using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class SubjectEntity : BaseEntity, ILocalizedEntity, ISoftDeletedEntity
    {
        public string Name { get; set; }


        public bool Deleted { get; set; }
    }
}