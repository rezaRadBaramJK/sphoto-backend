using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.PushNotification.Domain;

namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public class CustomerNotifyProfileService
    {
        private readonly IRepository<CustomerNotifyProfileEntity> _customerNotifyProfileRepository;

        public CustomerNotifyProfileService(IRepository<CustomerNotifyProfileEntity> customerNotifyProfileRepository)
        {
            _customerNotifyProfileRepository = customerNotifyProfileRepository;
        }


        public Task<CustomerNotifyProfileEntity> GetByCustomerIdAsync(int customerId)
        {
            return _customerNotifyProfileRepository.Table.FirstOrDefaultAsync(cnp => cnp.CustomerId == customerId);
        }

        public Task InsertAsync(CustomerNotifyProfileEntity customerNotifyProfile)
        {
            return _customerNotifyProfileRepository.InsertAsync(customerNotifyProfile);
        }

        public Task UpdateAsync(CustomerNotifyProfileEntity customerNotifyProfile)
        {
            return _customerNotifyProfileRepository.UpdateAsync(customerNotifyProfile);
        }
        
    }
}