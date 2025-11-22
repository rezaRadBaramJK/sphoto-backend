using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Customers;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    /// <summary>
    /// Customer service
    /// </summary>
    public class FrontendApiCustomerService : CustomerService
    {
        private readonly CustomerSettings _customerSettings;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Address> _customerAddressRepository;
        private readonly IRepository<CustomerAddressMapping> _customerAddressMappingRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task DeleteCustomerAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (customer.IsSystemAccount)
                throw new NopException($"System customer account ({customer.SystemName}) could not be deleted");

            customer.Deleted = true;

            if (_customerSettings.SuffixDeletedCustomers)
            {
                if (!string.IsNullOrEmpty(customer.Email))
                    customer.Email += $"-DELETED-{DateTime.Now:yy-MM-dd HH:mm:ss}";
                if (!string.IsNullOrEmpty(customer.Username))
                    customer.Username += $"-DELETED-{DateTime.Now:yy-MM-dd HH:mm:ss}";
            }

            await _customerRepository.UpdateAsync(customer, false);
            await _customerRepository.DeleteAsync(customer);
        }

        public FrontendApiCustomerService(CustomerSettings customerSettings,
            IGenericAttributeService genericAttributeService, INopDataProvider dataProvider,
            IRepository<Address> customerAddressRepository, IRepository<BlogComment> blogCommentRepository,
            IRepository<Customer> customerRepository,
            IRepository<CustomerAddressMapping> customerAddressMappingRepository,
            IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
            IRepository<CustomerPassword> customerPasswordRepository, IRepository<CustomerRole> customerRoleRepository,
            IRepository<ForumPost> forumPostRepository, IRepository<ForumTopic> forumTopicRepository,
            IRepository<GenericAttribute> gaRepository, IRepository<NewsComment> newsCommentRepository,
            IRepository<Order> orderRepository, IRepository<ProductReview> productReviewRepository,
            IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
            IRepository<PollVotingRecord> pollVotingRecordRepository,
            IRepository<ShoppingCartItem> shoppingCartRepository, IStaticCacheManager staticCacheManager,
            IStoreContext storeContext, ShoppingCartSettings shoppingCartSettings) : base(customerSettings,
            genericAttributeService, dataProvider, customerAddressRepository, blogCommentRepository, customerRepository,
            customerAddressMappingRepository, customerCustomerRoleMappingRepository, customerPasswordRepository,
            customerRoleRepository, forumPostRepository, forumTopicRepository, gaRepository, newsCommentRepository,
            orderRepository, productReviewRepository, productReviewHelpfulnessRepository, pollVotingRecordRepository,
            shoppingCartRepository, staticCacheManager, storeContext, shoppingCartSettings)
        {
            _customerSettings = customerSettings;
            _customerAddressRepository = customerAddressRepository;
            _customerRepository = customerRepository;
            _customerAddressMappingRepository = customerAddressMappingRepository;
            _staticCacheManager = staticCacheManager;
        }
        
        public override async Task<IList<Address>> GetAddressesByCustomerIdAsync(int customerId)
        {
            var query = from address in _customerAddressRepository.Table
                join cam in _customerAddressMappingRepository.Table on address.Id equals cam.AddressId
                where cam.CustomerId == customerId
                select address;

            var key = _staticCacheManager.PrepareKeyForShortTermCache(NopCustomerServicesDefaults.CustomerAddressesCacheKey, customerId);

            return await _staticCacheManager.GetAsync(key, async () =>
            {
                return await query
                    .OrderByDescending(a => a.CreatedOnUtc)
                    .ToListAsync();
            });
        }
    }
}