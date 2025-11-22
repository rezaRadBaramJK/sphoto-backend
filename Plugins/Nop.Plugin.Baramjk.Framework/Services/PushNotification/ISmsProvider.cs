using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.Framework.Services.PushNotification
{
    public interface ISmsProvider
    {
        Task SendMessageAsync(string phoneNumber, string message);
        Task SetSetting(SmsProviderMode mode);
    }
}