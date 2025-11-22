using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Frontend
{
    public class OrderApiController : BaseBaramjkApiController
    {
        private readonly IOrderService _orderService;
        private readonly PhotoPlatformOrderFactory _photoPlatformOrderFactory;
        private readonly IWorkContext _workContext;
        private readonly ReservationItemService _reservationItemService;

        public OrderApiController(IOrderService orderService,
            PhotoPlatformOrderFactory photoPlatformOrderFactory,
            IWorkContext workContext,
            ReservationItemService reservationItemService)
        {
            _orderService = orderService;
            _photoPlatformOrderFactory = photoPlatformOrderFactory;
            _workContext = workContext;
            _reservationItemService = reservationItemService;
        }

        [HttpGet("/FrontendApi/PhotoPlatform/Order/{orderId:int}/Details")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            if (orderId <= 0)
                return ApiResponseFactory.BadRequest("Invalid order id.");

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return ApiResponseFactory.BadRequest("Order not found");
            }

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            if (order.CustomerId != currentCustomer.Id)
            {
                return ApiResponseFactory.Unauthorized("You are not authorized to view this order");
            }

            if (order.Deleted)
            {
                return ApiResponseFactory.Unauthorized("Order is not valid");
            }

            var reservationWithDetails = await _reservationItemService.GetReservationDetailsByOrderIdAsync(order.Id);

            var result = await _photoPlatformOrderFactory.PrepareOrderDetailDtoAsync(order, reservationWithDetails);

            return ApiResponseFactory.Success(result);
        }

        [HttpGet("/FrontendApi/PhotoPlatform/Order/Details")]
        public async Task<IActionResult> GetOrderDetailsByGuid([FromQuery] string orderGuid)
        {
            if (string.IsNullOrEmpty(orderGuid))
            {
                return ApiResponseFactory.BadRequest("OrderGuid is not provided");
            }

            if (Guid.TryParse(orderGuid, out var orderId) == false)
            {
                return ApiResponseFactory.BadRequest("Invalid order guid");
            }

            var order = await _orderService.GetOrderByGuidAsync(orderId);
            if (order == null)
            {
                return ApiResponseFactory.BadRequest("Order not found");
            }

            if (order.Deleted)
            {
                return ApiResponseFactory.Unauthorized("Order is not valid");
            }

            var reservationWithDetails = await _reservationItemService.GetReservationDetailsByOrderIdAsync(order.Id);

            var result = await _photoPlatformOrderFactory.PrepareOrderDetailDtoAsync(order, reservationWithDetails);

            return ApiResponseFactory.Success(result);
        }

        [HttpGet("/FrontendApi/PhotoPlatform/Order/")]
        public async Task<IActionResult> GetCustomerOrders()
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var reservationsWithDetails = await _reservationItemService.GetCustomerOrdersReservationsAsync(currentCustomer.Id);
            var result = await _photoPlatformOrderFactory.PrepareCustomerOrderListAsync(reservationsWithDetails, false);

            return ApiResponseFactory.Success(result);
        }
    }
}