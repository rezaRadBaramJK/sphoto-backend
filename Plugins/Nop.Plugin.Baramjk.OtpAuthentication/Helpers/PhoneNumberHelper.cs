namespace Nop.Plugin.Baramjk.OtpAuthentication.Helpers
{
    public static class PhoneNumberHelper
    {
        public static string RemoveFirstBelongAtTheBeginningOfThePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return phoneNumber;
            
            if (phoneNumber.StartsWith("+"))
            {
                phoneNumber = phoneNumber.Replace("+", "");
            }

            return phoneNumber;
        }
    }
}