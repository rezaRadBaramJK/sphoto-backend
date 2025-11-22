using AutoMapper;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Actors;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.TimeSlots;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ContactUs.ContactInfos;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.ActorEvent;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Checkout;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Event;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Order;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.TimeSlot;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Order;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Infrastructure.Mapper
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            //events
            CreateMap<EventDetailsViewModel, EventDetail>();
            CreateMap<Product, EventBriefDto>();
            CreateMap<Product, CashierEventBriefDto>();
            CreateMap<Product, EventDto>();
            CreateMap<EventDetail, EventDto>();
            CreateMap<EventDetail, EventBriefDto>();
            CreateMap<EventDetail, BookmarkedEventDto>();
            CreateMap<EventDetail, CashierEventBriefDto>();
            CreateMap<EventDetail, SupervisorEventDto>();
            CreateMap<EventDto, CashierEventDto>();
            //time slots
            CreateMap<TimeSlotViewModel, TimeSlot>();
            CreateMap<TimeSlot, TimeSlotViewModel>();
            CreateMap<TimeSlot, TimeSlotDto>();
            //actors
            CreateMap<Actor, ActorViewModel>();
            CreateMap<ActorViewModel, Actor>();

            //checkout
            CreateMap<CheckoutConfirmModel, CheckoutConfirmModelDto>();

            //actorEvent
            CreateMap<ActorEvent, ActorEventDto>();
            CreateMap<EventDto, ActorEventDto>();

            //order
            CreateMap<Order, OrderDetailDto>();
            CreateMap<Order, CashierOrderDetailDto>();
            CreateMap<OrderDetailsModel, OrderDetailDto>();
            CreateMap<OrderDetailDto, CashierOrderDetailDto>();

            //contact us
            CreateMap<SubmitContactInfoApiParams, ContactInfoEntity>();
        }

        public int Order => 1;
    }
}