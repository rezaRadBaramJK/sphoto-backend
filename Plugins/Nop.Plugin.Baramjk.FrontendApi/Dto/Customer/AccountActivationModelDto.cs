using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Customer
{
    public class AccountActivationModelDto : ModelDto
    {
        public string Result { get; set; }

        public string ReturnUrl { get; set; }
    }
}