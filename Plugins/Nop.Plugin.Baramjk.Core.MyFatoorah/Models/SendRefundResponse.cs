using System.Linq;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.SendPayment;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.Validations;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models
{
    
    public class SendRefundResponse
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public ValidationError[] ValidationErrors { get; set; }

        public SendRefundResponseData Data { get; set; }

        public string MessageSummary
        {
            get
            {
                if (Message == "Unauthorized")
                {
                    Message += "-Invalid Access Code 103 !. Please contact MyFatoorah customer support to Activate your API Account.";
                }
                return (ValidationErrors != null) ? (Message + " - " + string.Join(",", ValidationErrors.Select(x => x.Error))) : Message;
            }
        }
    }
    public class SendRefundResponseData
    {
        public string Key { get; set; }

        public long RefundId { get; set; }
        public string RefundReference { get; set; }
        public long RefundInvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; }

    }
}