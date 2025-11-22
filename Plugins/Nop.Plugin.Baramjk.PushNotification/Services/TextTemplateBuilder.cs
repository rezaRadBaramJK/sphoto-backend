using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public class TextTemplate
    {
        public Order Order { get; set; }
        public string OrderNote { get; set; }

        public Customer Customer { get; set; }
        public Dictionary<string, string> Keywords = new Dictionary<string, string>();
        public static string TOKEN_PREFIX = "%";
        public static Dictionary<string, string> KeywordDescription = new Dictionary<string, string>()
        {
            { $"order.Id","Order's id"},
            { $"order.OrderStatus","Order's Status Code"},
            { $"order.OrderDiscount","Order's Discount"},
            { $"order.OrderGuid","Order's guid"},
            { $"order.OrderTax","Order's Tax"},
            { $"order.OrderTotal","Order's Total"},
            { $"order.OrderShippingExclTax","Order's shipping excluding tax"},
            { $"order.OrderShippingInclTax","Order's shipping including tax"},
            { $"order.OrderSubtotalExclTax","Order's subtotal including tax"},
            { $"order.OrderSubtotalInclTax","Order's subtotal including tax"},
            
            { $"orderNote.Note","Order node's text"},
            { $"orderNote.Id","Order node's Id"},
            
            { $"customer.SystemName","Customer's System name"},
            { $"customer.Active","Customer's Active Status"},
            { $"customer.CustomerGuid","Customer's Guid"},
            { $"customer.Id","Customer's Id"},
            { $"customer.Email","Customer's Email"},
            { $"customer.FailedLoginAttempts","Customer's FailedLoginAttempts"},
            { $"customer.AdminComment","Customer's AdminComment"},
            { $"customer.IsTaxExempt","Customer's IsTaxExempt"},
            { $"customer.Username","Customer's Username"},
            { $"customer.LastLoginDateUtc","Customer's LastLoginDateUtc"},
            { $"customer.CannotLoginUntilDateUtc","Customer's CannotLoginUntilDateUtc"},
            
        };

        public string Tokenize(string template)
        {
            foreach (var keyValuePair in Keywords)
            {
                template = template.Replace($"{TOKEN_PREFIX}{keyValuePair.Key}", keyValuePair.Value);
            }

            return template;
        }
    }
    public class Builder
    {
        private Order _order;
        private string _orderNote;
        private Customer _customer;
        public Dictionary<string, string> Keywords = new Dictionary<string, string>();
        private readonly ILogger _logger;

        public Builder(ILogger logger)
        {
            _logger = logger;
        }

        private void Add(string key, string value)
        {
            if (string.IsNullOrEmpty(key) ||string.IsNullOrEmpty(value))
            {
                Task.Run(()=>_logger.ErrorAsync($"adding failed for {key}"));
                return;
            }

            Keywords[key] = value;
        }
        public Builder WithOrder(Order order)
        {
            if (order==default)
            {
                Task.Run(()=>_logger.ErrorAsync("WithOrder is null"));
                return this;

            }
            _order = order;
            Add($"order.{nameof(order.Id)}", order.Id.ToString()); 
            Add($"order.{nameof(order.OrderStatus)}", order.OrderStatus.ToString()); 
            Add($"order.{nameof(order.OrderDiscount)}", order.OrderDiscount.ToString()); 
            Add($"order.{nameof(order.OrderGuid)}", order.OrderGuid.ToString()); 
            Add($"order.{nameof(order.OrderTax)}", order.OrderTax.ToString()); 
            Add($"order.{nameof(order.OrderTotal)}", order.OrderTotal.ToString()); 
            Add($"order.{nameof(order.OrderShippingExclTax)}", order.OrderShippingExclTax.ToString()); 
            Add($"order.{nameof(order.OrderShippingInclTax)}", order.OrderShippingInclTax.ToString()); 
            Add($"order.{nameof(order.OrderSubtotalExclTax)}", order.OrderSubtotalExclTax.ToString()); 
            Add($"order.{nameof(order.OrderSubtotalInclTax)}", order.OrderSubtotalExclTax.ToString()); 
            return this;
        }

        public Builder WithOrderNote(string orderNote)
        {
            if (orderNote==default)
            {
                Task.Run(()=>_logger.ErrorAsync("WithOrderNote is null"));
                return this;
            }
            _orderNote = orderNote;
            Add($"orderNote.{nameof(orderNote)}", orderNote); 
            return this;
        }

        public Builder WithCustomer(Customer customer)
        {
            if (customer==default)
            {
                Task.Run(()=>_logger.ErrorAsync("WithCustomer is null"));
                return this;

            }
            _customer = customer;
            Add($"customer.{nameof(customer.SystemName)}", customer.SystemName?.ToString()); 
            Add($"customer.{nameof(customer.Active)}", customer.Active.ToString()); 
            Add($"customer.{nameof(customer.Id)}", customer.Id.ToString()); 
            Add($"customer.{nameof(customer.CustomerGuid)}", customer.CustomerGuid.ToString()); 
            Add($"customer.{nameof(customer.Email)}", customer.Email?.ToString()); 
            Add($"customer.{nameof(customer.FailedLoginAttempts)}", customer.FailedLoginAttempts.ToString()); 
            Add($"customer.{nameof(customer.AdminComment)}", customer.AdminComment?.ToString()); 
            Add($"customer.{nameof(customer.IsTaxExempt)}", customer.IsTaxExempt.ToString()); 
            Add($"customer.{nameof(customer.Username)}", customer.Username?.ToString()); 
            Add($"customer.{nameof(customer.LastLoginDateUtc)}", customer.LastLoginDateUtc?.ToString()); 
            Add($"customer.{nameof(customer.CannotLoginUntilDateUtc)}", customer.CannotLoginUntilDateUtc?.ToString()); 

            return this;
        }

        public TextTemplate Build()
        {
            var tt =  new TextTemplate
            {
                Order = _order,
                OrderNote = _orderNote,
                Customer = _customer,
                Keywords = Keywords
            };
            return tt;
        }
    }
   
}