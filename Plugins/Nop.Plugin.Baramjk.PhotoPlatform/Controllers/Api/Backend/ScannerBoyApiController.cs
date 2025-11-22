using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains.Types;
using Nop.Plugin.Baramjk.PhotoPlatform.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reservations;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Backend
{
    public class ScannerBoyApiController : BaseBaramjkApiController
    {
        private readonly ReservationItemService _reservationItemService;
        private readonly IOrderService _orderService;
        private readonly PhotoPlatformOrderFactory _photoPlatformOrderFactory;
        private readonly IWorkContext _workContext;
        private readonly ActorEventService _actorEventService;
        private readonly EventFactory _eventFactory;
        private readonly ActorService _actorService;
        private readonly TimeSlotService _timeSlotService;


        public ScannerBoyApiController(ReservationItemService reservationItemService,
            IOrderService orderService,
            PhotoPlatformOrderFactory photoPlatformOrderFactory,
            IWorkContext workContext,
            ActorEventService actorEventService,
            EventFactory eventFactory,
            ActorService actorService, TimeSlotService timeSlotService)
        {
            _reservationItemService = reservationItemService;
            _orderService = orderService;
            _photoPlatformOrderFactory = photoPlatformOrderFactory;
            _workContext = workContext;
            _actorEventService = actorEventService;
            _eventFactory = eventFactory;
            _actorService = actorService;
            _timeSlotService = timeSlotService;
        }

        [AuthorizeApi(PermissionProvider.ScannerBoyName)]
        [HttpPut("/BackendApi/PhotoPlatform/ScannerBoy/Reservation/")]
        public async Task<IActionResult> UpdateReservationsAsync(UpdateReservationApiParams apiParams)
        {
            if (apiParams.Items.Any() == false)
                return ApiResponseFactory.BadRequest("No items were provided to update");
            var result = await _reservationItemService.UpdateReservationUsedCountsAsync(apiParams);
            return result.IsSuccess ? ApiResponseFactory.Success() : ApiResponseFactory.BadRequest(result.Message);
        }


        [AuthorizeApi(PermissionProvider.ScannerBoyName)]
        [HttpGet("/BackendApi/PhotoPlatform/ScannerBoy/Order/{orderGuid:guid}/Details")]
        public async Task<IActionResult> GetOrderDetails(Guid orderGuid)
        {
            var order = await _orderService.GetOrderByGuidAsync(orderGuid);
            if (order == null)
            {
                return ApiResponseFactory.BadRequest("Order not found");
            }

            if (order.Deleted)
            {
                return ApiResponseFactory.Unauthorized("Order is not valid");
            }

            var reservationWithDetails = await _reservationItemService.GetReservationDetailsByOrderIdAsync(order.Id);


            var result = await _photoPlatformOrderFactory.PrepareOrderDetailDtoAsync(order, reservationWithDetails,
                await _workContext.GetCurrentCustomerAsync(), includeUsed: false);

            return ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.ScannerBoyName)]
        [HttpGet("/BackendApi/PhotoPlatform/ScannerBoy/Order/Details")]
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

            return await GetOrderDetails(orderId);
        }

        [AuthorizeApi(PermissionProvider.ScannerBoyName)]
        [HttpGet("/BackendApi/PhotoPlatform/ScannerBoy/Event/{eventId:int}/Actors")]
        public async Task<IActionResult> GetEventActorsAsync(int eventId)
        {
            if (eventId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid eventId");
            }

            var actorEvents = await _actorEventService.GetEventActorsDetailsAsync(eventId);
            if (actorEvents.Any() == false)
            {
                return ApiResponseFactory.BadRequest("No actors were found");
            }

            var result = await _eventFactory.PrepareActorEventDtosAsync(actorEvents);

            return ApiResponseFactory.Success(result);
        }

        [AuthorizeApi(PermissionProvider.ScannerBoyName)]
        [HttpGet("/BackendApi/PhotoPlatform/ScannerBoy/Event/{eventId:int}/TimeSlots")]
        public async Task<IActionResult> GetEventTimeSlotsAsync(int eventId)
        {
            if (eventId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid eventId");
            }

            var actorEvents = await _timeSlotService.GetEventTimeSlotsAsync(eventId);
            if (actorEvents.Any() == false)
            {
                return ApiResponseFactory.BadRequest("No actors were found");
            }

            var result = await _eventFactory.PrepareGroupedTimeSlotsDtosAsync(actorEvents);

            return ApiResponseFactory.Success(result);
        }


        [AuthorizeApi(PermissionProvider.ScannerBoyName)]
        [HttpPut("/BackendApi/PhotoPlatform/ScannerBoy/Reservation/SwitchActor")]
        public async Task<IActionResult> SwitchReservationActorAsync(SwitchReservationActorApiParams apiParams)
        {
            var reservation = await _reservationItemService.GetByIdAsync(apiParams.ReservationId);

            if (reservation == null)
            {
                return ApiResponseFactory.BadRequest("Reservation was not found");
            }


            var actor = await _actorService.GetByIdAsync(apiParams.ActorId);

            if (actor == null)
            {
                return ApiResponseFactory.BadRequest("Actor was not found");
            }


            if (reservation.ReservationStatusId is (int)ReservationStatus.Complete or (int)ReservationStatus.Cancelled)
            {
                return ApiResponseFactory.BadRequest("You can't do this operation on a completed or cancelled reservation");
            }

            if (reservation.ActorId == apiParams.ActorId)
            {
                return ApiResponseFactory.BadRequest("This actor is already assigned to this reservation");
            }


            var actorEvent = await _actorEventService.GetActorEventAsync(reservation.EventId, apiParams.ActorId);
            if (actorEvent == null)
            {
                return ApiResponseFactory.BadRequest("Provided ActorId is not associated with the reservation event");
            }

            var otherReservation = await _reservationItemService.GetOtherActiveReservationAsync(reservation, apiParams.ActorId);

            if (otherReservation != null)
            {
                otherReservation.CameraManPhotoCount += reservation.CameraManPhotoCount - reservation.UsedCameraManPhotoCount;
                otherReservation.CustomerMobilePhotoCount += reservation.CustomerMobilePhotoCount - reservation.UsedCustomerMobilePhotoCount;
                await _reservationItemService.UpdateAsync(otherReservation);
                await _reservationItemService.DeleteAsync(reservation);
            }
            else
            {
                reservation.ActorId = apiParams.ActorId;
                await _reservationItemService.UpdateAsync(reservation);
            }


            return ApiResponseFactory.Success();
        }


        [AuthorizeApi(PermissionProvider.ScannerBoyName)]
        [HttpPut("/BackendApi/PhotoPlatform/ScannerBoy/Reservation/ChangeTime")]
        public async Task<IActionResult> ChangeReservationTimeAsync(ChangeReservationTimeApiParams apiParams)
        {
            var reservation = await _reservationItemService.GetByIdAsync(apiParams.ReservationId);

            if (reservation == null)
            {
                return ApiResponseFactory.BadRequest("Reservation was not found");
            }


            var timeSlot = await _timeSlotService.GetByIdAsync(apiParams.TimeSlotId);

            if (timeSlot == null)
            {
                return ApiResponseFactory.BadRequest("TimeSlot was not found");
            }

            if (timeSlot.Deleted || timeSlot.EventId != reservation.EventId || timeSlot.Active == false ||
                timeSlot.Date + timeSlot.EndTime < DateTime.UtcNow)
            {
                return ApiResponseFactory.BadRequest("time slot is not valid");
            }


            if (reservation.ReservationStatusId is (int)ReservationStatus.Complete or (int)ReservationStatus.Cancelled)
            {
                return ApiResponseFactory.BadRequest("You can't do this operation on a completed or refunded reservation");
            }

            if (reservation.TimeSlotId == apiParams.TimeSlotId)
            {
                return ApiResponseFactory.BadRequest("This timeSlot is already assigned to this reservation");
            }

            var otherAssociatedReservations = await _reservationItemService.GetOtherActiveReservationsAsync(reservation);

            if (otherAssociatedReservations.Any())
            {
                foreach (var reservationItem in otherAssociatedReservations)
                {
                    reservationItem.TimeSlotId = timeSlot.Id;
                }

                await _reservationItemService.UpdateAsync(otherAssociatedReservations);
            }

            reservation.TimeSlotId = apiParams.TimeSlotId;
            await _reservationItemService.UpdateAsync(reservation);


            await _orderService.InsertOrderNoteAsync(
                new OrderNote
                {
                    OrderId = reservation.OrderId,
                    CreatedOnUtc = DateTime.UtcNow,
                    DisplayToCustomer = false,
                    Note = $"Reservation time was Changed to {timeSlot.Date + timeSlot.StartTime:yyyy-MM-dd HH:mm:ss}"
                });


            return ApiResponseFactory.Success();
        }


        [AuthorizeApi(PermissionProvider.ScannerBoyName)]
        [HttpPut("/BackendApi/PhotoPlatform/ScannerBoy/Reservation/ChangePhotographyDetails")]
        public async Task<IActionResult> ChangeReservationPhotographyDetailsAsync(ChangePhotographyDetailsApiParams apiParams)
        {
            if (apiParams.ReservationId < 1)
            {
                return ApiResponseFactory.BadRequest("Invalid reservationId");
            }

            var reservation = await _reservationItemService.GetByIdAsync(apiParams.ReservationId);

            if (reservation == null)
            {
                return ApiResponseFactory.BadRequest("Reservation was not found");
            }

            if (reservation.ReservationStatusId is (int)ReservationStatus.Complete or (int)ReservationStatus.Cancelled)
            {
                return ApiResponseFactory.BadRequest("You can't do this operation on a completed or refunded reservation");
            }

            var result = await _reservationItemService.ChangeReservationPhotographyDetailsAsync(reservation, apiParams);
            return result.IsSuccess ? ApiResponseFactory.Success() : ApiResponseFactory.BadRequest(result.Message);
        }
    }
}