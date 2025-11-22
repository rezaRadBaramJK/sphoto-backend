using System;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Customer
{
    public class DownloadableProductsModelDto : ModelDto
    {
        public Guid OrderItemGuid { get; set; }

        public int OrderId { get; set; }

        public string CustomOrderNumber { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public string ProductAttributes { get; set; }

        public int DownloadId { get; set; }

        public int LicenseId { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}