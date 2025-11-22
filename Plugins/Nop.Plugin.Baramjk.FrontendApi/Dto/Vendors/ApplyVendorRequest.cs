namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Vendors
{
    public class ApplyVendorRequest : BaseModelDtoRequest<ApplyVendorModelDto>
    {
        public byte[] PictureBinary { get; set; }
        public int PictureId { get; set; }
    }
}