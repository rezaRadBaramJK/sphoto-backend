using System;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PushNotification.Models
{
    public class ScheduleNotificationModel : BaseEntity
    {
        public int Id { get; set; }
        public string JobId { get; set; }
        public string? Title { get; set; }
        public DateTime OnDateTime { get; set; }
    }
    public class ScheduleNotificationDomainModel: IDomainModel
    {
        public int Id { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.JobId")]

        public string JobId { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Title")]

        public string? Title { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.OnDateTime")]

        public DateTime OnDateTime { get; set; }
    }

    public class ScheduleSearchModel : ExtendedSearchModel
    {
        
    }
}