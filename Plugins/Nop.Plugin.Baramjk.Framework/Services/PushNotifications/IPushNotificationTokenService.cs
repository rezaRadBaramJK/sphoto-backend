using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.Framework.Services.PushNotifications
{
    public interface IPushNotificationTokenService
    {
        Task AddOrUpdateAsync(int customerId, string token, NotificationPlatform platform);

        Task<List<string>> GetTokensAsync(int? customerId = null, IEnumerable<int> customerIds = null,
            NotificationPlatform? platform = null);

        Task<List<IPushNotificationToken>> GetPushNotificationTokensAsync(IEnumerable<string> tokens);

        Task DeleteAsync(int customerId, string token);
        Task SetActiveStatusAsync(int customerId, bool status);
        Task<List<string>> GetTokensByRoleNameAsync(string name);
    }
}