using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Customer
{
    public class CustomerDownloadableProductsModelDto : ModelDto
    {
        public IList<DownloadableProductsModelDto> Items { get; set; }
    }
}