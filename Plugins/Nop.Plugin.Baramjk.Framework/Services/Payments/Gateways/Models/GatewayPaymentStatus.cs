using System;
using OrderPaymentStatus = Nop.Core.Domain.Payments.PaymentStatus;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models
{
    public enum GatewayPaymentStatus
    {
        Unknown = 0,

        Pending = 10,
        Authorized = 20,
        Paid = 30,
        PartiallyRefunded = 35,
        Refunded = 40,
        Voided = 50,

        Canceled = 1,
        Failed = 2,
        PartialPaid = 3,
        InProgress = 4,
        Invalid = 5
    }

    //INITIATED, IN_PROGRESS, ABANDONED, CANCELLED, FAILED, DECLINED, RESTRICTED, CAPTURED, VOID,TIMEDOUT, UNKNOWN


    public static class PaymentStatusEx
    {
        public static GatewayPaymentStatus FromOrderPaymentStatus(this OrderPaymentStatus status)
        {
            Enum.TryParse(status.ToString(), true, out GatewayPaymentStatus result);
            return result;
            // return (PaymentStatus)((int)status);
        }
    }
}