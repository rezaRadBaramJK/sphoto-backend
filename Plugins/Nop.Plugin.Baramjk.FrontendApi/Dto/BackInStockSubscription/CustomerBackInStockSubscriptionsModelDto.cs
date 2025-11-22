using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.BackInStockSubscription
{
    public class CustomerBackInStockSubscriptionsModelDto : ModelDto
    {
        public IList<BackInStockSubscriptionModelDto> Subscriptions { get; set; }

        public PagerModelDto PagerModel { get; set; }

        #region Nested classes

        public class BackInStockSubscriptionModelDto : ModelWithIdDto
        {
            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string SeName { get; set; }
        }

        #endregion
    }
}