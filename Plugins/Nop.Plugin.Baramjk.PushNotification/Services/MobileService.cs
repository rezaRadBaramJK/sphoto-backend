namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public interface IMobileService
    {
        bool IsValidMobileNumber(string text);
    }

    public class MobileService : IMobileService
    {
        public bool IsValidMobileNumber(string text)
        {
            //todo : validate
            return true;
        }
    }
}