using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.BackInStockSubscription
{
    public class BackInStockSubscribeModelDto : ModelDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public bool IsCurrentCustomerRegistered { get; set; }

        public bool SubscriptionAllowed { get; set; }

        public bool AlreadySubscribed { get; set; }

        public int MaximumBackInStockSubscriptions { get; set; }

        public int CurrentNumberOfBackInStockSubscriptions { get; set; }
    }
}