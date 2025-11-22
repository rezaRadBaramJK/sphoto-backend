using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Customer
{
    public class PasswordRecoveryConfirmModelDto : ModelDto
    {
        public string NewPassword { get; set; }

        public string ConfirmNewPassword { get; set; }

        public bool DisablePasswordChanging { get; set; }

        public string Result { get; set; }

        public string ReturnUrl { get; set; }
    }
}