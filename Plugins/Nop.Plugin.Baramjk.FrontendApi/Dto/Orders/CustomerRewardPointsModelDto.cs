using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Orders
{
    public class CustomerRewardPointsModelDto : ModelDto
    {
        public IList<RewardPointsHistoryModelDto> RewardPoints { get; set; }

        public PagerModelDto PagerModel { get; set; }

        public int RewardPointsBalance { get; set; }

        public string RewardPointsAmount { get; set; }

        public int MinimumRewardPointsBalance { get; set; }

        public string MinimumRewardPointsAmount { get; set; }
    }
}