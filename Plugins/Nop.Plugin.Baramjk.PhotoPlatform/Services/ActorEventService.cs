using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ActorEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Event;


namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class ActorEventService
    {
        private readonly IRepository<ActorEvent> _actorEventRepository;
        private readonly IRepository<Actor> _actorRepository;
        private readonly IRepository<ActorPicture> _actorPictureRepository;


        public ActorEventService(IRepository<ActorEvent> actorEventRepository,
            IRepository<Actor> actorRepository,
            IRepository<ActorPicture> actorPictureRepository)
        {
            _actorEventRepository = actorEventRepository;
            _actorRepository = actorRepository;
            _actorPictureRepository = actorPictureRepository;
        }

        public Task<IPagedList<ActorEvent>> GetAllEventActorEventsAsync(int eventId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return _actorEventRepository.Table.Where(ae => ae.EventId == eventId && ae.Deleted == false).ToPagedListAsync(pageIndex, pageSize);
        }

        public Task<ActorEvent> GetByIdAsync(int id)
        {
            return _actorEventRepository.GetByIdAsync(id, includeDeleted: false);
        }

        public Task InsertAsync(ActorEvent actorEvent)
        {
            return _actorEventRepository.InsertAsync(actorEvent);
        }

        public Task InsertAsync(List<ActorEvent> actorEvents)
        {
            return _actorEventRepository.InsertAsync(actorEvents);
        }


        public Task<ActorEvent> GetActorEventAsync(int eventId, int actorId)
        {
            return _actorEventRepository.Table.Where(ae => ae.EventId == eventId && ae.ActorId == actorId && ae.Deleted == false)
                .FirstOrDefaultAsync();
        }

        public Task<List<ActorEvent>> GetActorEventAsync(int eventId, List<int> actorIds)
        {
            return _actorEventRepository.Table.Where(ae => ae.EventId == eventId && actorIds.Contains(ae.ActorId) && ae.Deleted == false)
                .ToListAsync();
        }

        public Task DeleteAsync(ActorEvent actorEvent)
        {
            return _actorEventRepository.DeleteAsync(actorEvent);
        }

        public Task UpdateAsync(ActorEvent actorEvent)
        {
            return _actorEventRepository.UpdateAsync(actorEvent);
        }

        public Task UpdateAsync(IList<ActorEvent> actorEvent)
        {
            return _actorEventRepository.UpdateAsync(actorEvent);
        }


        public Task<List<EventDetailActor>> GetEventActorsDetailsAsync(int eventId)
        {
            var query =
                from actorEvent in _actorEventRepository.Table
                join actor in _actorRepository.Table
                    on actorEvent.ActorId equals actor.Id
                where actorEvent.EventId == eventId && actorEvent.Deleted == false && actor.Deleted == false
                select new EventDetailActor
                {
                    Actor = actor,
                    CameraManEachPictureCost = actorEvent.CameraManEachPictureCost,
                    CustomerMobileEachPictureCost = actorEvent.CustomerMobileEachPictureCost,
                };

            return query.ToListAsync();
        }


        public Task<List<Actor>> GetEventActorsAsync(int eventId)
        {
            var query =
                from actorEvent in _actorEventRepository.Table
                join actor in _actorRepository.Table
                    on actorEvent.ActorId equals actor.Id
                where actorEvent.EventId == eventId && actorEvent.Deleted == false
                select actor;

            return query.ToListAsync();
        }


        public Task<IPagedList<Actor>> GetNotAssociatedActors(int eventId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query =
                from actor in _actorRepository.Table
                join actorEvent in _actorEventRepository.Table on actor.Id equals actorEvent.ActorId into actorEvents
                from ae in actorEvents
                where ae.EventId != eventId && ae.Deleted == false
                select actor;

            return query.ToPagedListAsync(pageIndex, pageSize);
        }


        public Task<IPagedList<ActorEventDetailModel>> GetActorEventsDetailsAsync(int eventId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query =
                from actorEvent in _actorEventRepository.Table
                join actor in _actorRepository.Table on actorEvent.ActorId equals actor.Id
                where actorEvent.EventId == eventId && actorEvent.Deleted == false && actor.Deleted == false
                join actorPicture in _actorPictureRepository.Table on actor.Id equals actorPicture.ActorId into actorPictures
                select new ActorEventDetailModel
                {
                    ActorEvent = actorEvent,
                    Actor = actor,
                    ActorPicture = actorPictures.DefaultIfEmpty().FirstOrDefault(),
                };

            return query.ToPagedListAsync(pageIndex, pageSize);
        }
    }
}