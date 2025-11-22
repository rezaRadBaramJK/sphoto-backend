using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.Framework.Services.Sms
{
    public interface ISmsService
    {
        Task SendSms(int customerId, string message);
        Task SendSms(string receptor, string message, int customerId = default);
    }
}