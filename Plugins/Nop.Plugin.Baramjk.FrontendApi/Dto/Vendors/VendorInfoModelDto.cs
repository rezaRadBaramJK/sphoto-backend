using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Vendors
{
    public class VendorInfoModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }

        public string PictureUrl { get; set; }

        public int PictureId { get; set; }

        public IList<VendorAttributeModelDto> VendorAttributes { get; set; }
    }
}