using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.PushNotifications
{
    public interface IPushNotificationCustomerProvider
    {
        Task<IList<NotifyCustomersServiceResults>> GetCustomersAsync();
    }
}