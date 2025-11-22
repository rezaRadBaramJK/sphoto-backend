using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Ui.DataTables;

namespace Nop.Plugin.Baramjk.PushNotification.Models
{
    public class PushNotificationItemRelCustomerModel : IDomainModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }

        [TableField(Visible = false)]
        public int CustomerId { get; set; }

        public bool IsRead { get; set; }
    }
}