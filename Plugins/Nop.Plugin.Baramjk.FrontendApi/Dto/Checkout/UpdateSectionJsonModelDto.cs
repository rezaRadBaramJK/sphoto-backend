using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class UpdateSectionJsonModelDto<T> : BaseDto
        where T : ModelDto
    {
        public string Name { get; set; }
        public string ViewName { get; set; }
        public T Model { get; set; }
    }
}