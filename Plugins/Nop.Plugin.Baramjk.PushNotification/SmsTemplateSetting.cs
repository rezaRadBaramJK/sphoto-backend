using Nop.Core.Configuration;

namespace Nop.Plugin.Baramjk.PushNotification
{
    public class SmsTemplateSetting : ISettings
    {
        // public bool IsOrderPlacedEnabled { get; set; }
        // public string OrderPlaced { get; set; }
        public bool IsOrderNoteEnabled { get; set; }

        public string OrderNote { get; set; }
        

        //order payment status
        public string OrderPaymentStatusPending { get; set; }
        public bool IsOrderPaymentStatusPendingEnabled { get; set; }
        
        public string OrderPaymentStatusAuthorized { get; set; }
        public bool IsOrderPaymentStatusAuthorizedEnabled { get; set; }
        
        public string OrderPaymentStatusPaid { get; set; }
        public bool IsOrderPaymentStatusPaidEnabled { get; set; }
        
        public string OrderPaymentStatusPartiallyRefunded { get; set; }
        public bool IsOrderPaymentStatusPartiallyRefundedEnabled { get; set; }
        
        public string OrderPaymentStatusRefunded { get; set; }
        public bool IsOrderPaymentStatusRefundedEnabled { get; set; }
        
        public string OrderPaymentStatusVoided { get; set; }
        public bool IsOrderPaymentStatusVoidedEnabled { get; set; }
        // end order payment status
        
        // order shipping status
        public bool IsOrderShippingNotRequiredStatusEnabled { get; set; }

        public string OrderShippingNotRequiredStatus { get; set; }
        
        public bool IsOrderShippingNotYetShippedStatusEnabled { get; set; }

        public string OrderShippingNotYetShippedStatus { get; set; }
        
        public bool IsOrderShippingPartiallyShippedStatusEnabled { get; set; }

        public string OrderShippingPartiallyShippedStatus { get; set; }
        
        public bool IsOrderShippingShippedStatusEnabled { get; set; }

        public string OrderShippingShippedStatus { get; set; }
        
        public bool IsOrderShippingDeliveredStatusEnabled { get; set; }

        public string OrderShippingDeliveredStatus { get; set; }
        //end order shipping status
        
        public bool IsOrderStatusPendingEnabled { get; set; }

        public string OrderStatusPending { get; set; }
        
        public bool IsOrderStatusProcessingEnabled { get; set; }

        public string OrderStatusProcessing { get; set; }
        public bool IsOrderStatusCompleteEnabled { get; set; }

        public string OrderStatusComplete { get; set; }
        public bool IsOrderStatusCancelledEnabled { get; set; }

        public string OrderStatusCancelled { get; set; }
        
        // public bool IsOrderPaidEnabled { get; set; }
        //
        // public string OrderPaid { get; set; }
    }
}