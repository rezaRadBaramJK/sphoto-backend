using Nop.Core;
using Nop.Core.Domain.Common;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Domains.PaymentFeeRule
{
    public class PaymentFeeRule : BaseEntity, ISoftDeletedEntity
    {
        public int PaymentMethodId { get; set; }
        public int CountryId { get; set; }
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFeePercentage { get; set; }
        public bool Active { get; set; }

        public bool Deleted { get; set; }
    }
}