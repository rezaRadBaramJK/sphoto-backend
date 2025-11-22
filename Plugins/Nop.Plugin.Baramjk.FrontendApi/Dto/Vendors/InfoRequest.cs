namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Vendors
{
    public class InfoRequest : BaseModelDtoRequest<VendorInfoModelDto>
    {
        public byte[] PictureBinary { get; set; }
    }
}