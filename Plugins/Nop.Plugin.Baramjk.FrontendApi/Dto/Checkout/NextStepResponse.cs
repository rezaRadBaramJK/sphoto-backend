using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class NextStepResponse<T> : BaseDto
        where T : ModelDto
    {
        public UpdateSectionJsonModelDto<T> UpdateSectionModel { get; set; }

        public bool WrongBillingAddress { get; set; }

        public string GotoSection { get; set; }
    }
}