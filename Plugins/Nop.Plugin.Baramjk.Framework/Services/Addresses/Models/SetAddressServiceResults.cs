using Nop.Core.Domain.Common;

namespace Nop.Plugin.Baramjk.Framework.Services.Addresses.Models
{
    public class SetAddressServiceResults
    {
        public Address BillingAddress { get; set; }
        
        public Address ShippingAddress { get; set; }
    }
}