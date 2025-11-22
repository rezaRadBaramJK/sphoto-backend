using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Attributes;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reservation;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Backend
{
    public class ReservationApiController : BaseBaramjkApiController
    {
        private readonly ReservationItemService _reservationItemService;
        private readonly ReservationHistoryService _reservationHistoryService;
        private readonly IWorkContext _workContext;

        public ReservationApiController(ReservationItemService reservationItemService,
            ReservationHistoryService reservationHistoryService, IWorkContext workContext)
        {
            _reservationItemService = reservationItemService;
            _reservationHistoryService = reservationHistoryService;
            _workContext = workContext;
        }


        private static Dictionary<string, PropertyChange> GetChanges(ReservationItem original, ReservationItem updated, int customerId)
        {
            var changes = new Dictionary<string, PropertyChange>();

            void AddChange<T>(string propName, T oldVal, T newVal)
            {
                if (!EqualityComparer<T>.Default.Equals(oldVal, newVal))
                {
                    changes[propName] = new PropertyChange { Old = oldVal, New = newVal, ChangedBy = customerId, ModifiedDate = DateTime.Now };
                }
            }

            AddChange(nameof(original.CameraManPhotoCount), original.CameraManPhotoCount, updated.CameraManPhotoCount);
            AddChange(nameof(original.CustomerMobilePhotoCount), original.CustomerMobilePhotoCount, updated.CustomerMobilePhotoCount);
            AddChange(nameof(original.ReservationStatusId), original.ReservationStatusId, updated.ReservationStatusId);

            return changes;
        }


        [AuthorizeApi(PermissionProvider.ManagementName)]
        [HttpPut("/BackendApi/PhotoPlatform/Reservation/")]
        public async Task<IActionResult> UpdateReservationAsync([FromBody] ReservationUpdateApiParams apiParams)
        {
            var reservationItem = await _reservationItemService.GetByIdAsync(apiParams.ReservationItemId);

            if (reservationItem == null)
            {
                return ApiResponseFactory.NotFound("Reservation not found");
            }


            var newReservationItem = new ReservationItem
            {
                Id = reservationItem.Id,
                OrderId = reservationItem.OrderId,
                EventId = reservationItem.EventId,
                ActorId = reservationItem.ActorId,
                TimeSlotId = reservationItem.TimeSlotId,
                CameraManPhotoCount = reservationItem.CameraManPhotoCount,
                CustomerMobilePhotoCount = reservationItem.CustomerMobilePhotoCount,
                ReservationStatusId = reservationItem.ReservationStatusId,
                Queue = reservationItem.Queue
            };

            if (apiParams.ReservationStatusId > 0)
            {
                newReservationItem.ReservationStatusId = apiParams.ReservationStatusId;
            }

            if (apiParams.CustomerMobilePhotoCount > 0)
            {
                newReservationItem.CustomerMobilePhotoCount = apiParams.CustomerMobilePhotoCount;
            }

            if (apiParams.CameraManPhotoCount > 0)
            {
                newReservationItem.CameraManPhotoCount = apiParams.CameraManPhotoCount;
            }

            var customerId = (await _workContext.GetCurrentCustomerAsync()).Id;

            var newChanges = GetChanges(reservationItem, newReservationItem, customerId);

            if (newChanges.Any())
            {
                var reservationHistory = await _reservationHistoryService.GetReservationHistoryAsync(reservationItem.Id, customerId);

                if (reservationHistory != null)
                {
                    var existingChanges = string.IsNullOrWhiteSpace(reservationHistory.Changes)
                        ? new Dictionary<string, List<PropertyChange>>()
                        : JsonConvert.DeserializeObject<Dictionary<string, List<PropertyChange>>>(reservationHistory.Changes)
                          ?? new Dictionary<string, List<PropertyChange>>();

                    foreach (var change in newChanges)
                    {
                        if (!existingChanges.ContainsKey(change.Key))
                            existingChanges[change.Key] = new List<PropertyChange>();

                        existingChanges[change.Key].Add(change.Value);
                    }

                    reservationHistory.LastModifiedDate = DateTime.Now;
                    reservationHistory.Changes = JsonConvert.SerializeObject(existingChanges);

                    await _reservationHistoryService.UpdateAsync(reservationHistory);
                }
                else
                {
                    var newReservationHistory = new ReservationHistory
                    {
                        ReservationId = reservationItem.Id,
                        LastChangedBy = customerId,
                        LastModifiedDate = DateTime.Now,
                    };

                    var changes = new Dictionary<string, List<PropertyChange>>();

                    foreach (var change in newChanges)
                    {
                        if (!changes.ContainsKey(change.Key))
                            changes[change.Key] = new List<PropertyChange>();

                        changes[change.Key].Add(change.Value);
                    }

                    newReservationHistory.Changes = JsonConvert.SerializeObject(changes);
                    await _reservationHistoryService.InsertAsync(newReservationHistory);
                }
            }


            await _reservationItemService.UpdateAsync(newReservationItem);


            return ApiResponseFactory.Success();
        }
    }
}