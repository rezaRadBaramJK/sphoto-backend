using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Baramjk.FrontendApi.Controllers;
using Nop.Plugin.Baramjk.FrontendApi.Models.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class FrontendOrderService
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Address> _addressRepository;

        public FrontendOrderService(
            ICustomerService customerService,
            IOrderService orderService,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IGenericAttributeService genericAttributeService, IRepository<Order> orderRepository,
            IRepository<Address> addressRepository)
        {
            _customerService = customerService;
            _orderService = orderService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _genericAttributeService = genericAttributeService;
            _orderRepository = orderRepository;
            _addressRepository = addressRepository;
        }

        public virtual async Task<ReOrderServiceResults> ReOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            var results = new ReOrderServiceResults
            {
                Success = true
            };

            //move shopping cart items (if possible)
            foreach (var orderItem in await _orderService.GetOrderItemsAsync(order.Id))
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                var warnings = await _shoppingCartService.AddToCartAsync(customer, product,
                    ShoppingCartType.ShoppingCart, order.StoreId,
                    orderItem.AttributesXml, orderItem.UnitPriceExclTax,
                    orderItem.RentalStartDateUtc, orderItem.RentalEndDateUtc,
                    orderItem.Quantity, false);

                if (warnings.Any())
                {
                    results.FailedProducts.Add(product);
                    results.Success = false;
                }
                else
                    results.AddedProducts.Add(product);
            }

            //set checkout attributes
            //comment the code below if you want to disable this functionality
            await _genericAttributeService.SaveAttributeAsync(
                customer,
                NopCustomerDefaults.CheckoutAttributes,
                order.CheckoutAttributesXml,
                order.StoreId);

            return results;
        }

        public virtual async Task<OrderTrackServiceResult> TrackOrderAsync(string identifier, int orderId)
        {
            if (string.IsNullOrWhiteSpace(identifier) || orderId <= 0)
                return new OrderTrackServiceResult { Success = false, ErrorMessage = "Invalid input" };
            
            // Clean up input identifier if it's a phone number
            if (!identifier.Contains("@"))
            {
                if (!identifier.All(c => char.IsDigit(c) || c == ' ' || c == '+'))
                    return new OrderTrackServiceResult
                        { Success = false, ErrorMessage = "Invalid phone number format" };
                identifier = NormalizePhoneNumber(identifier);
            }
            
            else if (!CommonHelper.IsValidEmail(identifier))
                return new OrderTrackServiceResult { Success = false, ErrorMessage = "Invalid email format" };
            

            var order = await (
                from o in _orderRepository.Table
                where o.Id == orderId && !o.Deleted && o.ShippingAddressId.HasValue
                join a in _addressRepository.Table on o.ShippingAddressId equals a.Id
                select new { Order = o, Address = a }).FirstOrDefaultAsync();

            if (order == null)
                return new OrderTrackServiceResult
                    { Success = false, ErrorMessage = $"Order Id = {orderId} not found" };

            bool isMatch;
            if (identifier.Contains("@"))
            {
                isMatch = order.Address.Email == identifier;
            }
            else
            {
                var addressPhone = NormalizePhoneNumber(order.Address.PhoneNumber);
                isMatch = addressPhone == identifier;
            }

            if (!isMatch)
                return new OrderTrackServiceResult
                    { Success = false, ErrorMessage = $"Order Id = {orderId} not found" };

            return new OrderTrackServiceResult
            {
                Success = true,
                Order = order.Order
            };
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            var cleaned = string.Join("", phoneNumber.Where(c => char.IsDigit(c) || c == '+'));

            if (cleaned.StartsWith("+965"))
                cleaned = cleaned.Substring(4);

            return cleaned;
        }
    }
}