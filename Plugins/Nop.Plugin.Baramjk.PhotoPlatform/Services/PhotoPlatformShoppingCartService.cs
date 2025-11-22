using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ShoppingCart;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Services;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class PhotoPlatformShoppingCartService
    {
        private readonly IRepository<ShoppingCartItemTimeSlot> _shoppingCartItemTimeSlotRepository;
        private readonly IRepository<TimeSlot> _timeSlotRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Actor> _actorRepository;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IRepository<ActorEvent> _actorEventRepository;
        private readonly IShoppingCartService _shoppingCartService;

        public PhotoPlatformShoppingCartService(
            IRepository<ShoppingCartItemTimeSlot> shoppingCartItemTimeSlotRepository,
            IRepository<TimeSlot> timeSlotRepository,
            IWorkContext workContext,
            IStoreContext storeContext,
            IRepository<ShoppingCartItem> shoppingCartItemRepository,
            IRepository<Product> productRepository,
            IRepository<Actor> actorRepository,
            IRepository<ActorEvent> actorEventRepository,
            IShoppingCartService shoppingCartService)
        {
            _shoppingCartItemTimeSlotRepository = shoppingCartItemTimeSlotRepository;
            _workContext = workContext;
            _timeSlotRepository = timeSlotRepository;
            _storeContext = storeContext;
            _shoppingCartItemRepository = shoppingCartItemRepository;
            _productRepository = productRepository;
            _actorRepository = actorRepository;
            _actorEventRepository = actorEventRepository;
            _shoppingCartService = shoppingCartService;
        }

        private async Task<BaseServiceResult> CheckSelectedTimeSlotAndActorsAsync(List<TimeSlot> timeSlots, SubmitShoppingCartItemApiModel item,
            bool allDayTimeSlotsValid = false)
        {
            var timeSlot = timeSlots.FirstOrDefault(timeSlot => timeSlot.Id == item.TimeSlotId);

            if (timeSlot is null)
            {
                return new BaseServiceResult($"Time slot is not valid");
            }

            if (timeSlot.EventId != item.EventId)
            {
                return new BaseServiceResult($"Time slot is not associated with this product");
            }

            if (timeSlot.Active == false || timeSlot.Deleted)
            {
                return new BaseServiceResult($"Time slot is not available");
            }

            if (timeSlot.Date.Date < DateTime.Now.Date)
            {
                return new BaseServiceResult($"Time slot {timeSlot.StartTime.ToString()} in {timeSlot.Date:d} is from the past");
            }

            if (allDayTimeSlotsValid == false && timeSlot.Date.Date == DateTime.Now.Date
                                              && timeSlot.StartTime < DateTime.Now.TimeOfDay)
            {
                return new BaseServiceResult($"Time slot {timeSlot.StartTime.ToString()} in {timeSlot.Date:d} is from the past");
            }

            var actorIds = item.PhotographyDetails.Select(pd => pd.ActorId).Distinct().ToArray();

            var actorEvents = await _actorEventRepository.Table.Where(ae => ae.EventId == item.EventId && ae.Deleted == false).ToListAsync();

            var notAssociatedActors = actorIds.Except(actorEvents.Select(ae => ae.ActorId).Distinct().ToArray()).ToList();

            if (notAssociatedActors.Any())
            {
                return new BaseServiceResult($"Actors with ids {string.Join(',', notAssociatedActors)} are not associated to this event");
            }

            return new BaseServiceResult();
        }

        public async Task<BaseServiceResult> ProcessAddingItemToCartAsync(SubmitShoppingCartItemsApiModel model, bool allDayTimeSlotsValid = false)
        {
            var timeSlotIds = model.Items.Select(item => item.TimeSlotId).Distinct().ToArray();

            var timeSlots = await _timeSlotRepository.Table
                .Where(ts => timeSlotIds.Contains(ts.Id))
                .ToListAsync();

            foreach (var item in model.Items)
            {
                var timeSlotActorValidationResult = await CheckSelectedTimeSlotAndActorsAsync(timeSlots, item, allDayTimeSlotsValid);

                if (timeSlotActorValidationResult.IsSuccess == false)
                {
                    return timeSlotActorValidationResult;
                }

                var customer = await _workContext.GetCurrentCustomerAsync();
                var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;


                var currentTime = DateTime.UtcNow;


                foreach (var photographyDetail in item.PhotographyDetails)
                {
                    var shoppingCartItem =
                        await _shoppingCartItemRepository.Table.Where(sci =>
                                sci.ProductId == item.EventId && sci.CustomerId == customer.Id && sci.ShoppingCartTypeId ==
                                (int)ShoppingCartType.ShoppingCart)
                            .FirstOrDefaultAsync();

                    if (shoppingCartItem == null)
                    {
                        shoppingCartItem = new ShoppingCartItem
                        {
                            ShoppingCartType = ShoppingCartType.ShoppingCart,
                            StoreId = storeId,
                            ProductId = item.EventId,
                            AttributesXml = "",
                            CustomerEnteredPrice = decimal.Zero,
                            Quantity = 1,
                            RentalStartDateUtc = null,
                            RentalEndDateUtc = null,
                            CreatedOnUtc = currentTime,
                            UpdatedOnUtc = currentTime,
                            CustomerId = customer.Id
                        };
                        await _shoppingCartItemRepository.InsertAsync(shoppingCartItem);

                        //! Custom logic
                        await _shoppingCartItemTimeSlotRepository.InsertAsync(new ShoppingCartItemTimeSlot
                        {
                            ShoppingCartItemId = shoppingCartItem.Id,
                            TimeSlotId = item.TimeSlotId,
                            ActorId = photographyDetail.ActorId,
                            CameraManPhotoCount = photographyDetail.CameraManPhotoCount,
                            CustomerMobilePhotoCount = photographyDetail.CustomerMobilePhotoCount
                        });
                        //! end
                    }
                    else
                    {
                        var shoppingCartItemTimeSlot = await _shoppingCartItemTimeSlotRepository.Table.FirstOrDefaultAsync(scit =>
                            scit.ShoppingCartItemId == shoppingCartItem.Id && scit.TimeSlotId == item.TimeSlotId &&
                            scit.ActorId == photographyDetail.ActorId && scit.Deleted == false);

                        if (shoppingCartItemTimeSlot != null)
                        {
                            shoppingCartItemTimeSlot.CameraManPhotoCount += photographyDetail.CameraManPhotoCount;
                            shoppingCartItemTimeSlot.CustomerMobilePhotoCount += photographyDetail.CustomerMobilePhotoCount;
                            await _shoppingCartItemTimeSlotRepository.UpdateAsync(shoppingCartItemTimeSlot);
                        }
                        else
                        {
                            shoppingCartItem.Quantity += 1;
                            await _shoppingCartItemRepository.UpdateAsync(shoppingCartItem);
                            //! Custom logic
                            await _shoppingCartItemTimeSlotRepository.InsertAsync(new ShoppingCartItemTimeSlot
                            {
                                ShoppingCartItemId = shoppingCartItem.Id,
                                TimeSlotId = item.TimeSlotId,
                                ActorId = photographyDetail.ActorId,
                                CameraManPhotoCount = photographyDetail.CameraManPhotoCount,
                                CustomerMobilePhotoCount = photographyDetail.CustomerMobilePhotoCount
                            });
                            //! end
                        }
                    }
                }
            }


            return new BaseServiceResult();
        }

        public async Task<BaseServiceResult> ProcessUpdatingShoppingCartAsync(SubmitShoppingCartItemsApiModel model,
            bool allDayTimeSlotsValid = false)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

            var timeSlotIds = model.Items.Select(item => item.TimeSlotId).Distinct().ToArray();
            var timeSlots = await _timeSlotRepository.Table
                .Where(ts => timeSlotIds.Contains(ts.Id))
                .ToListAsync();

            foreach (var item in model.Items)
            {
                var timeSlotActorValidationResult = await CheckSelectedTimeSlotAndActorsAsync(timeSlots, item, allDayTimeSlotsValid);

                if (timeSlotActorValidationResult.IsSuccess == false)
                {
                    return timeSlotActorValidationResult;
                }

                var currentTime = DateTime.UtcNow;

                var newShoppingCartItemTimeSlotsIds = new List<int>();

                foreach (var photographyDetail in item.PhotographyDetails)
                {
                    ShoppingCartItemTimeSlot shoppingCartItemTimeSlot;

                    var shoppingCartItem =
                        await _shoppingCartItemRepository.Table.Where(sci =>
                                sci.ProductId == item.EventId && sci.CustomerId == customer.Id && sci.ShoppingCartTypeId ==
                                (int)ShoppingCartType.ShoppingCart)
                            .FirstOrDefaultAsync();

                    if (shoppingCartItem == null)
                    {
                        shoppingCartItem = new ShoppingCartItem
                        {
                            ShoppingCartType = ShoppingCartType.ShoppingCart,
                            StoreId = storeId,
                            ProductId = item.EventId,
                            AttributesXml = "",
                            CustomerEnteredPrice = decimal.Zero,
                            Quantity = 1,
                            RentalStartDateUtc = null,
                            RentalEndDateUtc = null,
                            CreatedOnUtc = currentTime,
                            UpdatedOnUtc = currentTime,
                            CustomerId = customer.Id
                        };
                        await _shoppingCartItemRepository.InsertAsync(shoppingCartItem);

                        //! Custom logic
                        shoppingCartItemTimeSlot = new ShoppingCartItemTimeSlot
                        {
                            ShoppingCartItemId = shoppingCartItem.Id,
                            TimeSlotId = item.TimeSlotId,
                            ActorId = photographyDetail.ActorId,
                            CameraManPhotoCount = photographyDetail.CameraManPhotoCount,
                            CustomerMobilePhotoCount = photographyDetail.CustomerMobilePhotoCount
                        };
                        await _shoppingCartItemTimeSlotRepository.InsertAsync(shoppingCartItemTimeSlot);
                        //! end
                    }
                    else
                    {
                        shoppingCartItemTimeSlot = await _shoppingCartItemTimeSlotRepository.Table.FirstOrDefaultAsync(scit =>
                            scit.ShoppingCartItemId == shoppingCartItem.Id && scit.TimeSlotId == item.TimeSlotId &&
                            scit.ActorId == photographyDetail.ActorId);

                        if (shoppingCartItemTimeSlot != null)
                        {
                            shoppingCartItemTimeSlot.CameraManPhotoCount = photographyDetail.CameraManPhotoCount;
                            shoppingCartItemTimeSlot.CustomerMobilePhotoCount = photographyDetail.CustomerMobilePhotoCount;
                            await _shoppingCartItemTimeSlotRepository.UpdateAsync(shoppingCartItemTimeSlot);
                        }
                        else
                        {
                            shoppingCartItem.Quantity += 1;
                            await _shoppingCartItemRepository.UpdateAsync(shoppingCartItem);
                            //! Custom logic
                            shoppingCartItemTimeSlot = new ShoppingCartItemTimeSlot
                            {
                                ShoppingCartItemId = shoppingCartItem.Id,
                                TimeSlotId = item.TimeSlotId,
                                ActorId = photographyDetail.ActorId,
                                CameraManPhotoCount = photographyDetail.CameraManPhotoCount,
                                CustomerMobilePhotoCount = photographyDetail.CustomerMobilePhotoCount
                            };
                            await _shoppingCartItemTimeSlotRepository.InsertAsync(shoppingCartItemTimeSlot);
                            //! end
                        }
                    }

                    newShoppingCartItemTimeSlotsIds.Add(shoppingCartItemTimeSlot.Id);
                }

                //Deletion Logic
                var query =
                    from shoppingCartItemTimeSlot in _shoppingCartItemTimeSlotRepository.Table
                    join shoppingCartItem in _shoppingCartItemRepository.Table on shoppingCartItemTimeSlot.ShoppingCartItemId equals shoppingCartItem
                        .Id
                    where shoppingCartItemTimeSlot.Deleted == false && shoppingCartItem.CustomerId == customer.Id &&
                          shoppingCartItem.ShoppingCartTypeId == (int)ShoppingCartType.ShoppingCart
                    select new { shoppingCartItemTimeSlot, shoppingCartItem };

                var shoppingCartItemTimeSlotsPairs = await query.ToListAsync();

                var toBeDeletedEntities =
                    shoppingCartItemTimeSlotsPairs
                        .Select(x => x.shoppingCartItemTimeSlot)
                        .Where(scit => newShoppingCartItemTimeSlotsIds.Contains(scit.Id) == false).ToList();

                var groupedItems = shoppingCartItemTimeSlotsPairs
                    .GroupBy(p => p.shoppingCartItem.Id)
                    .Select(g => new
                    {
                        ShoppingCartItemId = g.Key,
                        ShoppingCartItem = g.Select(x => x.shoppingCartItem).First(),
                        ShoppingCartItemTimeSlots = g.Select(x => x.shoppingCartItemTimeSlot).ToList()
                    })
                    .ToList();

                foreach (var entity in toBeDeletedEntities)
                {
                    var shoppingCartItem = groupedItems.First(g => g.ShoppingCartItemId == entity.ShoppingCartItemId).ShoppingCartItem;

                    if (shoppingCartItem.Quantity > 1)
                    {
                        shoppingCartItem.Quantity -= 1;
                        await _shoppingCartItemRepository.UpdateAsync(shoppingCartItem);
                    }
                    else
                    {
                        await _shoppingCartService.DeleteShoppingCartItemAsync(shoppingCartItem);
                    }

                    await _shoppingCartItemTimeSlotRepository.DeleteAsync(entity);
                }
            }

            return new BaseServiceResult();
        }

        public async Task<BaseServiceResult> DeleteEventItemFromCartAsync(int id)
        {
            var shoppingCartItem = await _shoppingCartItemRepository.GetByIdAsync(id);
            if (shoppingCartItem is null)
                return new BaseServiceResult("shopping cart item not found!");

            await _shoppingCartItemRepository.DeleteAsync(shoppingCartItem);
            return new BaseServiceResult();
        }

        public async Task<BaseServiceResult> DeleteShoppingCartItemAsync(DeleteItemsFromCartApiModel apiModel)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var timeSlotIds = apiModel.Items.Select(i => i.TimeSlotId);
            var eventIds = apiModel.Items.Select(i => i.EventId);

            var query =
                from shoppingCartItemTimeSlot in _shoppingCartItemTimeSlotRepository.Table
                join shoppingCartItem in _shoppingCartItemRepository.Table on shoppingCartItemTimeSlot.ShoppingCartItemId equals shoppingCartItem
                    .Id
                where timeSlotIds
                          .Contains(shoppingCartItemTimeSlot.TimeSlotId) &&
                      eventIds
                          .Contains(shoppingCartItem.ProductId) &&
                      shoppingCartItem.CustomerId == customer.Id &&
                      shoppingCartItem.ShoppingCartTypeId == (int)ShoppingCartType.ShoppingCart &&
                      shoppingCartItemTimeSlot.Deleted == false
                select new
                {
                    shoppingCartItemTimeSlot,
                    shoppingCartItem,
                };


            var shoppingCartItemTimeSlotPairs = await query.ToListAsync();


            if (shoppingCartItemTimeSlotPairs.Any() == false)
            {
                return new BaseServiceResult("shopping cart item not found!");
            }

            var groupedItems = shoppingCartItemTimeSlotPairs
                .GroupBy(x => x.shoppingCartItem.Id)
                .Select(g => new
                {
                    ShoppingCartItem = g.First().shoppingCartItem,
                    ShoppingCartItemTimeSlot = g.Select(x => x.shoppingCartItemTimeSlot).ToList(),
                })
                .ToList();


            foreach (var item in groupedItems)
            {
                if (item.ShoppingCartItem.Quantity > item.ShoppingCartItemTimeSlot.Count)
                {
                    item.ShoppingCartItem.Quantity -= item.ShoppingCartItemTimeSlot.Count;
                    await _shoppingCartItemRepository.UpdateAsync(item.ShoppingCartItem);
                }
                else
                {
                    await _shoppingCartService.DeleteShoppingCartItemAsync(item.ShoppingCartItem);
                }
            }

            await _shoppingCartItemTimeSlotRepository.DeleteAsync(shoppingCartItemTimeSlotPairs.Select(p => p.shoppingCartItemTimeSlot).ToList());

            return new BaseServiceResult();
        }

        public async Task DeleteAsync(int shoppingCartItemId)
        {
            var shoppingCartItemTimeSlot = await _shoppingCartItemTimeSlotRepository
                .Table.FirstOrDefaultAsync(x => x.ShoppingCartItemId == shoppingCartItemId);

            if (shoppingCartItemTimeSlot is not null)
                await _shoppingCartItemTimeSlotRepository.DeleteAsync(shoppingCartItemTimeSlot);
        }

        public async Task<BaseServiceResult> ValidateShoppingCartAsync(bool allDayTimeSlotsValid = false)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var query =
                from sci in _shoppingCartItemRepository.Table
                join scit in _shoppingCartItemTimeSlotRepository.Table on sci.Id equals scit.ShoppingCartItemId
                join ts in _timeSlotRepository.Table on scit.TimeSlotId equals ts.Id
                where sci.CustomerId == customer.Id && sci.StoreId == currentStore.Id && (ts.Date.Date < DateTime.Now.Date ||
                                                                                          (allDayTimeSlotsValid == false &&
                                                                                           ts.Date.Date == DateTime.Now.Date &&
                                                                                           ts.StartTime < DateTime.Now.TimeOfDay))
                select ts;

            var passedTimeSlots = await query.ToListAsync();

            if (passedTimeSlots.Any())
            {
                var message = string.Join(",",
                    passedTimeSlots.Select(ts => $"Time slot {ts.StartTime.ToString()} in {ts.Date:d} is from the past"));
                return new BaseServiceResult(message);
            }


            return new BaseServiceResult();
        }

        public async Task<List<ShoppingCartApiModel>> GetShoppingCartAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var query =
                from shoppingCartItem in _shoppingCartItemRepository.Table
                where shoppingCartItem.CustomerId == customer.Id &&
                      shoppingCartItem.ShoppingCartTypeId == (int)ShoppingCartType.ShoppingCart
                join shoppingCartItemTimeSlot in _shoppingCartItemTimeSlotRepository.Table on shoppingCartItem.Id equals shoppingCartItemTimeSlot
                    .ShoppingCartItemId
                join product in _productRepository.Table on shoppingCartItem.ProductId equals product.Id
                where product.Published == true && product.Deleted == false && shoppingCartItemTimeSlot.Deleted == false
                join timeSlot in _timeSlotRepository.Table on shoppingCartItemTimeSlot.TimeSlotId equals timeSlot.Id
                join actor in _actorRepository.Table on shoppingCartItemTimeSlot.ActorId equals actor.Id
                select new ShoppingCartApiModel
                {
                    Product = product,
                    ShoppingCartItem = shoppingCartItem,
                    TimeSlot = timeSlot,
                    ShoppingCartItemTimeSlot = shoppingCartItemTimeSlot,
                    Actor = actor,
                };

            return await query.ToListAsync();
        }

        public async Task<decimal> CalculateShoppingCartTotalPriceAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var query =
                from shoppingCartItemTimeSlot in _shoppingCartItemTimeSlotRepository.Table
                join shoppingCartItem in _shoppingCartItemRepository.Table on shoppingCartItemTimeSlot.ShoppingCartItemId equals shoppingCartItem.Id
                join actorEvent in _actorEventRepository.Table on shoppingCartItemTimeSlot.ActorId equals actorEvent.ActorId
                where shoppingCartItem.CustomerId == customer.Id && actorEvent.Deleted == false && actorEvent.EventId == shoppingCartItem.ProductId &&
                      shoppingCartItem.ShoppingCartTypeId == (int)ShoppingCartType.ShoppingCart && shoppingCartItemTimeSlot.Deleted == false
                select
                    new
                    {
                        shoppingCartItemTimeSlot.CameraManPhotoCount,
                        shoppingCartItemTimeSlot.CustomerMobilePhotoCount,
                        actorEvent.CameraManEachPictureCost,
                        actorEvent.CustomerMobileEachPictureCost,
                    };
            var result = await query.ToListAsync();

            return result.Sum(x =>
                (x.CameraManPhotoCount * x.CameraManEachPictureCost) + (x.CustomerMobilePhotoCount * x.CustomerMobileEachPictureCost));
        }

        public async Task<decimal> CalculateShoppingCartPriceAsync(int[] shoppingCartItemIds)
        {
            var query =
                from shoppingCartItemTimeSlot in _shoppingCartItemTimeSlotRepository.Table
                join shoppingCartItem in _shoppingCartItemRepository.Table on shoppingCartItemTimeSlot.ShoppingCartItemId equals shoppingCartItem.Id
                join actorEvent in _actorEventRepository.Table on shoppingCartItemTimeSlot.ActorId equals actorEvent.ActorId
                where shoppingCartItemIds.Contains(shoppingCartItemTimeSlot.ShoppingCartItemId) && actorEvent.EventId == shoppingCartItem.ProductId &&
                      actorEvent.Deleted == false && shoppingCartItemTimeSlot.Deleted == false
                select
                    new
                    {
                        shoppingCartItemTimeSlot.CameraManPhotoCount,
                        shoppingCartItemTimeSlot.CustomerMobilePhotoCount,
                        actorEvent.CameraManEachPictureCost,
                        actorEvent.CustomerMobileEachPictureCost,
                    };
            var result = await query.ToListAsync();

            return result.Sum(x =>
                (x.CameraManPhotoCount * x.CameraManEachPictureCost) + (x.CustomerMobilePhotoCount * x.CustomerMobileEachPictureCost));
        }
    }
}