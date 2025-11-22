using System;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PushNotification.Models.Search
{
    public class SmsSearchModel :ExtendedSearchModel
    {
        [NopResourceDisplayName("Admin.Plugins.PushNotification.Menu.Sms.From")]

        public DateTime? From { get; set; }
        [NopResourceDisplayName("Admin.Plugins.PushNotification.Menu.Sms.To")]

        public DateTime? To { get; set; }
        [NopResourceDisplayName("Admin.Plugins.PushNotification.Menu.Sms.Receptor")]

        public string Receptor { get; set; }
        [NopResourceDisplayName("Admin.Plugins.PushNotification.Menu.Sms.Status")]

        public SendSmsStatus Status { get; set; }
        
    }
}