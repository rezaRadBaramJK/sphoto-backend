using System;
using System.Collections.Generic;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Common;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.GetPaymentStatus
{
    public class GetPaymentStatusResponse
    {
        public int InvoiceId { get; set; }
        public string Message { get; set; }
        public string InvoiceStatus { get; set; }
        public string InvoiceReference { get; set; }
        public string CustomerReference { get; set; }
        public string MessageSummary { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ExpiryDate { get; set; }
        public decimal InvoiceValue { get; set; }
        public string Comments { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerEmail { get; set; }
        public string UserDefinedField { get; set; }
        public string InvoiceDisplayValue { get; set; }
        public PaymentData Data { get; set; }
        public List<InvoiceItem> InvoiceItems { get; set; }
        public List<InvoiceTransaction> InvoiceTransactions { get; set; }
        public List<GetPaymentStatusSupplier> Suppliers { get; set; }
        
        public bool IsPaid => InvoiceStatus == "Paid";
        public GatewayPaymentStatus PaymentStatus
        {
            get
            {
                Enum.TryParse(InvoiceStatus, true, out GatewayPaymentStatus result);
                return result;
            }
        }
    }
    public class PaymentData
    {
        public long InvoiceId { get; set; }

        public string InvoiceStatus { get; set; }

        public string InvoiceReference { get; set; }

        public string CustomerReference { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ExpiryDate { get; set; }

        public decimal InvoiceValue { get; set; }

        public string Comments { get; set; }

        public string CustomerName { get; set; }

        public string CustomerMobile { get; set; }

        public string CustomerEmail { get; set; }

        public string UserDefinedField { get; set; }

        public string InvoiceDisplayValue { get; set; }

        public IList<Invoiceitem> InvoiceItems { get; set; }

        public IList<InvoiceTransaction> InvoiceTransactions { get; set; }
    }

}