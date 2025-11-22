using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Plugin.Baramjk.PushNotification.Domain;
using Nop.Services.Customers;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public class TokenService : IPushNotificationTokenService
    {
        private readonly IRepository<PushNotificationToken> _repository;
        private readonly ICustomerService _customerService;
        private readonly IRepository<CustomerCustomerRoleMapping> _repositoryCustomerCustomerRoleMapping;
        private readonly ILogger _logger;

        public TokenService(IRepository<PushNotificationToken> repository, ICustomerService customerService,
            IRepository<CustomerCustomerRoleMapping> repositoryCustomerCustomerRoleMapping, ILogger logger)
        {
            _repository = repository;
            _customerService = customerService;
            _repositoryCustomerCustomerRoleMapping = repositoryCustomerCustomerRoleMapping;
            _logger = logger;
        }

        public async Task AddOrUpdateAsync(int customerId, string token, NotificationPlatform platform)
        {
            var notificationTokens = await _repository.Table
                .Where(item => item.Token == token)
                .ToListAsync();

            await _logger.InformationAsync(
                $"AddOrUpdateAsync query result : {notificationTokens.Count} customerId={customerId} token={token} platform={platform}");
            if (notificationTokens.Count > 1)
            {
                await _logger.InformationAsync(
                    $"Warning AddOrUpdateAsync query result has too many records : {notificationTokens.Count} customerId={customerId} token={token} platform={platform}");
            }

            if (notificationTokens.Any()) // we have matching tokens
            {
                foreach (var pushNotificationToken in
                         notificationTokens.Where(x =>
                             x.CustomerId != customerId)) // token belongs to other user . most likely guest
                {
                    pushNotificationToken.CustomerId = customerId;
                    pushNotificationToken.LastModify = DateTime.Now;

                    await _repository.UpdateAsync(notificationTokens);
                }
            }
            else // no matching records, inserting new 
            {
                var notificationToken = new PushNotificationToken
                {
                    Token = token,
                    Platform = platform,
                    CustomerId = customerId,
                    LastModify = DateTime.Now,
                    IsActive = true
                };
                await _repository.InsertAsync(notificationToken);
            }

            var dbTokens = await _repository.Table
                .Where(item => item.Token == token)
                .OrderByDescending(x => x.LastModify)
                .ToListAsync();
            if (dbTokens.Count > 1)
            {
                await _repository.DeleteAsync(dbTokens.Skip(1).ToList());
            }
        }

        public async Task<List<string>> GetTokensAsync(int? customerId = null, IEnumerable<int> customerIds = null,
            NotificationPlatform? platform = null)
        {
            var tokens = await _repository.Table
                .Where(item => customerId == null || item.CustomerId == customerId)
                .Where(item => platform == null || item.Platform == platform)
                .Where(item => customerIds == null || customerIds.Contains(item.CustomerId))
                .Where(item => item.IsActive)
                .Select(item => item.Token)
                .ToListAsync();

            return tokens;
        }

        public async Task<List<PushNotificationToken>> GetCustomerTokensAsync(int customerId)
        {
            var tokens = await _repository.Table
                .Where(item => item.CustomerId == customerId)
                .ToListAsync();

            return tokens;
        }

        public async Task<List<IPushNotificationToken>> GetPushNotificationTokensAsync(IEnumerable<string> tokens)
        {
            var customerPushNotificationTokens = await _repository.Table
                .Where(item => tokens == null || tokens.Contains(item.Token))
                .ToListAsync();

            return new List<IPushNotificationToken>(customerPushNotificationTokens);
        }

        public async Task DeleteAsync(int customerId, string token)
        {
            var notificationTokens = await _repository.Table
                .Where(item => item.CustomerId == customerId && item.Token == token)
                .ToListAsync();

            await _repository.DeleteAsync(notificationTokens);
        }

        public async Task SetActiveStatusAsync(int customerId, bool status)
        {
            var pushNotificationTokens = await GetCustomerTokensAsync(customerId);
            foreach (var item in pushNotificationTokens)
                item.IsActive = status;

            await _repository.UpdateAsync(pushNotificationTokens);
        }

        public async Task<List<string>> GetTokensByRoleNameAsync(string name)
        {
            var role = await _customerService.GetCustomerRoleBySystemNameAsync(name);

            var customersIds = _repositoryCustomerCustomerRoleMapping.Table
                .Where(item => item.CustomerRoleId == role.Id)
                .Select(item => item.CustomerId).ToArray();

            return await GetTokensAsync(customerIds: customersIds);
        }
    }
}