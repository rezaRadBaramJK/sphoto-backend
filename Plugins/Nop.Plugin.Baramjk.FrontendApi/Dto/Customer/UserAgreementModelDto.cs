using System;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Customer
{
    public class UserAgreementModelDto : ModelDto
    {
        public Guid OrderItemGuid { get; set; }

        public string UserAgreementText { get; set; }
    }
}