using Nop.Core.Caching;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Plugins
{
    public static class DefaultValues
    { 
        public const string TableNamePrefix = "PhotoPlatform_";
        
        public const string SystemName = "Baramjk.PhotoPlatform";

        public const string CustomMoveShoppingCartItemsEvent = "PhotoPlatformMoveShoppingCartItemsToOrderItemsAsync";
        
        public static CacheKey ActorPictureModelKey => new("Nop.pres.actor.picture-{0}-{1}-{2}-{3}");
        
        public static readonly CacheKey PaymentInfoCacheKey = new("Baramjk.PhotoPlatform.PaymentInfo-{0}-{1}");
        
        public const string ActorRoleName = "Actor";
        
        public const string CashierRoleName = "Cashier";

        public const string ScannerBoyRoleName = "ScannerBoy";
        
        public const string SupervisorRoleName = "Supervisor";
        
        public const string ProductionRoleName = "Production";
        
        public const string PaidWithAttributeKey = "PaidWith";
        
        public const string DefaultPaymentMethodSystemName = "Payments.MyFatoorah";
        
        public const string OrderPrintCountAttributeKey = "OrderPrintCount";
        
        public const string CustomerPhoneForCashierOrderAttributeKey = "CustomerPhoneForCashierOrder";
        
        public const string OrderPlacedByCashierAttributeKey = "OrderPlacedByCashier";
        
        public const string DefaultCheckoutAttributeDescription = "Items Total Price";
        
        public const string ConsumerName = "ContactUs";
        
        public const string PaymentCallbackUrlPaymentIdName = "{paymentId}";
        
        public const string DefaultCountryTwoLetterIsoCode = "KW";
        
        public const string CustomerLastRewardPointsReceivedDateKey = "Customer.LastBirthDayRewardPointsReceivedDate";

    }
}