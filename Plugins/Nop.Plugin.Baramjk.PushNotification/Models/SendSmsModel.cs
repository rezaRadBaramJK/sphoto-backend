using System;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PushNotification.Models
{
    public class SendSmsModel : BaseEntity
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public DateTime DateTime { get; set; }
        public SendSmsStatus Status { get; set; }
        public string Receptor { get; set; }
        public string Text { get; set; }
    }
    public class SendSmsDomainModel : IDomainModel
    {
        [NopResourceDisplayName("Baramjk.PushNotification.OnDateTime")]

        public DateTime DateTime { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Status")]

        public SendSmsStatus Status { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Receptor")]

        public string Receptor { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Text")]

        public string Text { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Id")]

        public int Id { get; set; }
    }
    public enum SendSmsStatus
    {
        Pending=1,
        Sent=2,
        Failed=3,
        Scheduled=4
    }
}