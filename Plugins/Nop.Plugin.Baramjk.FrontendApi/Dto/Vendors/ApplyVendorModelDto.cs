using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Vendors
{
    public class ApplyVendorModelDto : ModelDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }

        public IList<VendorAttributeModelDto> VendorAttributes { get; set; }

        public bool DisplayCaptcha { get; set; }

        public bool TermsOfServiceEnabled { get; set; }

        public bool TermsOfServicePopup { get; set; }

        public bool DisableFormInput { get; set; }

        public string Result { get; set; }
    }
}