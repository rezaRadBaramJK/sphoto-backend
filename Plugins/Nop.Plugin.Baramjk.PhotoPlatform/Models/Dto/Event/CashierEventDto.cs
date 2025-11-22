namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Event
{
    public class CashierEventDto : EventDto
    {
        public string CashierName { get; set; }

        public int EventStatusId { get; set; }

        public EventStatus EventStatus
        {
            get => (EventStatus)EventStatusId;
            set => EventStatusId = (int)value;
        }

        public decimal OpeningFundBalanceAmount { get; set; }

        public string OpeningFundBalance { get; set; }
    }
}