using Nop.Plugin.Baramjk.Framework.Exceptions;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.SendPayment
{
    public class MyFatoorahCustomerInfo
    {
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerEmail { get; set; }

        public bool Validate()
        {
            string msg = null;
            if (string.IsNullOrEmpty(CustomerName))
                msg = "CustomerName Required";
            if (string.IsNullOrEmpty(CustomerMobile))
                msg = "CustomerMobile Required";
            if (string.IsNullOrEmpty(CustomerEmail))
                msg = "CustomerEmail Required";


            if (msg != null)
                throw new ValidateBusinessException(msg);

            return true;
        }
    }
}