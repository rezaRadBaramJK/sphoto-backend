using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Orders
{
    public class RecurringOrderModelDto : ModelWithIdDto
    {
        public string StartDate { get; set; }

        public string CycleInfo { get; set; }

        public string NextPayment { get; set; }

        public int TotalCycles { get; set; }

        public int CyclesRemaining { get; set; }

        public int InitialOrderId { get; set; }

        public bool CanRetryLastPayment { get; set; }

        public string InitialOrderNumber { get; set; }

        public bool CanCancel { get; set; }
    }
}