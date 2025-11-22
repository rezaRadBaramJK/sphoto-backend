using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Domains
{
    public class Technician : BaseEntity, ILocalizedEntity
    {
        public string Name { get; set; }
        public int StoreId { get; set; }
        public string Description { get; set; }
        public int CustomerId { get; set; }
        public int VendorId { get; set; }
    }
}