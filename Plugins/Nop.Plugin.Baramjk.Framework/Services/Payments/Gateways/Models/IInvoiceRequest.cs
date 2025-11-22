namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models
{
    public interface IInvoiceRequest
    {
        string PaymentMethod { get; set; }
        string NotificationOption { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string FullName { get; set; }
        string PhoneNumber { get; set; }
        string Email { get; set; }
        string ClientReferenceId { get; }
        decimal Amount { get; set; }
        string CurrencyIsoCode { get; set; }
        string Description { get; set; }
        string SuccessCallBackUrl { get; }
        string ErrorUrl { get; }

        string GetFullName();
    }
}