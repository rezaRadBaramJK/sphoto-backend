using System;
using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Orders;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ReturnRequests
{
    public class SubmitReturnRequestModelDto : ModelDto
    {
        public int OrderId { get; set; }

        public string CustomOrderNumber { get; set; }

        public IList<ReturnRequestOrderItemModelDto> Items { get; set; }

        public int ReturnRequestReasonId { get; set; }

        public IList<ReturnRequestReasonModelDto> AvailableReturnReasons { get; set; }

        public int ReturnRequestActionId { get; set; }

        public IList<ReturnRequestActionModelDto> AvailableReturnActions { get; set; }

        public string Comments { get; set; }

        public bool AllowFiles { get; set; }

        public Guid UploadedFileGuid { get; set; }

        public string Result { get; set; }

        #region Nested Classes

        public class ReturnRequestOrderItemModelDto : ModelWithIdDto
        {
            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string AttributeInfo { get; set; }

            public string UnitPrice { get; set; }

            public int Quantity { get; set; }
        }

        #endregion
    }
}