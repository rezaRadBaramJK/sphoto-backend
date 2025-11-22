using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains.PaymentFeeRule;


namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Services
{
    public class PaymentFeeRuleService
    {
        private readonly IRepository<PaymentFeeRule> _paymentFeeRuleRepository;

        public PaymentFeeRuleService(IRepository<PaymentFeeRule> paymentFeeRuleRepository)
        {
            _paymentFeeRuleRepository = paymentFeeRuleRepository;
        }


        public Task<IPagedList<PaymentFeeRule>> GetAllPagedAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return _paymentFeeRuleRepository.Table
                .Where(pfr => pfr.Deleted == false)
                .ToPagedListAsync(pageIndex, pageSize);
        }


        public Task<PaymentFeeRule> GetByIdAsync(int paymentFeeRuleId)
        {
            return _paymentFeeRuleRepository.GetByIdAsync(paymentFeeRuleId, includeDeleted: false);
        }


        public Task InsertAsync(PaymentFeeRule paymentFeeRule)
        {
            return _paymentFeeRuleRepository.InsertAsync(paymentFeeRule);
        }

        public Task UpdateAsync(PaymentFeeRule paymentFeeRule)
        {
            return _paymentFeeRuleRepository.UpdateAsync(paymentFeeRule);
        }

        public Task DeleteAsync(PaymentFeeRule paymentFeeRule)
        {
            return _paymentFeeRuleRepository.DeleteAsync(paymentFeeRule);
        }

        public Task<PaymentFeeRule> GetByMethodIdAndCountryId(int paymentMethodId, int countryId)
        {
            return _paymentFeeRuleRepository.Table
                .Where(pfr => pfr.PaymentMethodId == paymentMethodId && pfr.CountryId == countryId && pfr.Deleted == false)
                .FirstOrDefaultAsync();
        }
    }
}