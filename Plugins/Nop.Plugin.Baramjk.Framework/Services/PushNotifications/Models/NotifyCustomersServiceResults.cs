using System.Collections.Generic;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Baramjk.Framework.Services.PushNotifications.Models
{
    public class NotifyCustomersServiceResults
    {
        public IList<Customer> Customers { get; set; }
        
        public string Title { get; set; }
        
        public string Body { get; set; }
        
    }
}