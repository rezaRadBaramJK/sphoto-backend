using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class EditAddressResponse<T> : BaseDto
        where T : ModelDto
    {
        public string Redirect { get; set; }
        public int SelectedId { get; set; }
        public UpdateSectionJsonModelDto<T> UpdateSectionModel { get; set; }
    }
}