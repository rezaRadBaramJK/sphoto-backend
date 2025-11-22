using System.Threading.Tasks;
using LinqToDB;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Data;

namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public class PushNotificationCustomerService
    {
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;

        public PushNotificationCustomerService(IRepository<GenericAttribute> genericAttributeRepository)
        {
            _genericAttributeRepository = genericAttributeRepository;
        }

        public async Task<string> GetCustomerPhoneNumberAsync(int customerId)
        {
            var result = await _genericAttributeRepository.Table
                .FirstOrDefaultAsync(ga => ga.Key == NopCustomerDefaults.PhoneAttribute &&
                                           ga.KeyGroup == nameof(Customer) &&
                                           ga.EntityId == customerId);
            return result == null ? string.Empty : result.Value;
        }
    }
}