using Nop.Plugin.Baramjk.PushNotification.Domain;
using Nop.Plugin.Baramjk.PushNotification.Models;

namespace Nop.Plugin.Baramjk.PushNotification.Factories.Api
{
    public class CustomerNotifyProfileFactory
    {
        public CustomerNotifyProfileDto PrepareDto(CustomerNotifyProfileEntity customerNotifyProfile)
        {
            if (customerNotifyProfile == null)
                return new CustomerNotifyProfileDto
                {
                    Sale = true,
                    Discount = true,
                    BackInStock = true
                };


            return new CustomerNotifyProfileDto
            {
                Sale = customerNotifyProfile.Sale,
                Discount = customerNotifyProfile.Discount,
                BackInStock = customerNotifyProfile.Discount
            };

        } 
    }
}