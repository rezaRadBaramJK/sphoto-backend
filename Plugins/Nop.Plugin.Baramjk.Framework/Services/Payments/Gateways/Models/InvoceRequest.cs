namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models
{
    public class InvoiceRequest : IInvoiceRequest
    {
        public string PaymentMethod { get; set; }
        public string NotificationOption { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string ClientReferenceId { get; internal set; }
        public decimal Amount { get; set; }
        public string CurrencyIsoCode { get; set; }
        public string Description { get; set; }
        public string SuccessCallBackUrl { get; private set; }
        public string ErrorUrl { get;  private set;}

        public virtual string GetFullName()
        {
            return string.IsNullOrEmpty(FullName) ? $"{FirstName} {LastName}" : FullName;
        }

        public void SetClientReferenceId(string clientReferenceId)
        {
            ClientReferenceId = clientReferenceId;
        }

        public void SetCallBackUrl(string successCallBackUrl, string errorUrl)
        {
            SuccessCallBackUrl = successCallBackUrl;
            ErrorUrl = errorUrl;
        }
    }
}