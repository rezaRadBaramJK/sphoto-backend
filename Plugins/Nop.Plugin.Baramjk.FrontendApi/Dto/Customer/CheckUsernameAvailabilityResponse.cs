using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Customer
{
    public class CheckUsernameAvailabilityResponse : BaseDto
    {
        public bool Available { get; set; }

        public string Text { get; set; }
    }
}