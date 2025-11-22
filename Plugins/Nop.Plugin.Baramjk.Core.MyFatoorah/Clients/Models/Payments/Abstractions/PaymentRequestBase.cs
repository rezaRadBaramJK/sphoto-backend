using System;
using System.Collections.Generic;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.SendPayment;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.Suppliers;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.Abstractions
{
    public abstract class PaymentRequestBase
    {
        public string CustomerName { get; set; }
        
        public string DisplayCurrencyIso { get; set; }
        
        public string MobileCountryCode { get; set; }
        
        public string CustomerMobile { get; set; }
        
        public string CustomerEmail { get; set; }
        
        public decimal InvoiceValue { get; set; }
        
        public string CallBackUrl { get; set; }
        
        public string ErrorUrl { get; set; }
        
        public string CustomerReference { get; set; }

        public string CustomerCivilId { get; set; }
        
        public string UserDefinedField { get; set; }

        public Customeraddress CustomerAddress { get; set; }
        
        public IList<Invoiceitem> InvoiceItems { get; set; }
        
        public List<MyFatoorahSupplier> Suppliers { get; set; }
        
        public DateTime ExpiryDate { get; set; }
    }
}