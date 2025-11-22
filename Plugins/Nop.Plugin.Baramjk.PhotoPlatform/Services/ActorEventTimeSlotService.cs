using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.ActorEventTimeSlots;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class ActorEventTimeSlotService
    {
        private readonly IRepository<ActorEventTimeSlot> _actorEventTimeSlotRepository;
        private readonly IRepository<TimeSlot> _timeSlotRepository;
        private readonly IRepository<ActorEvent> _actorEventRepository;
        private readonly IRepository<Actor> _actorRepository;
        private readonly IRepository<ActorPicture> _actorPictureRepository;

        public ActorEventTimeSlotService(IRepository<ActorEventTimeSlot> actorEventTimeSlotRepository,
            IRepository<TimeSlot> timeSlotRepository,
            IRepository<ActorEvent> actorEventRepository,
            IRepository<Actor> actorRepository,
            IRepository<ActorPicture> actorPictureRepository)
        {
            _actorEventTimeSlotRepository = actorEventTimeSlotRepository;
            _timeSlotRepository = timeSlotRepository;
            _actorEventRepository = actorEventRepository;
            _actorRepository = actorRepository;
            _actorPictureRepository = actorPictureRepository;
        }

        public Task<ActorEventTimeSlot> GetAsync(int actorEventId, int timeSlotId)
        {
            return _actorEventTimeSlotRepository.Table.Where(x => x.TimeSlotId == timeSlotId && x.ActorEventId == actorEventId).FirstOrDefaultAsync();
        }


        public async Task UpdateAsync(ActorEventTimeSlot actorEventTimeSlot)
        {
            await _actorEventTimeSlotRepository.UpdateAsync(actorEventTimeSlot);
        }

        public async Task InsertAsync(ActorEventTimeSlot actorEventTimeSlot)
        {
            await _actorEventTimeSlotRepository.InsertAsync(actorEventTimeSlot);
        }


        public Task<IPagedList<ActorEventTimeSlotsModel>> GetAllAsync(int timeSlotId, int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query =
                from ts in _timeSlotRepository.Table
                join ae in _actorEventRepository.Table on ts.EventId equals ae.EventId
                join a in _actorRepository.Table on ae.ActorId equals a.Id
                join aets in _actorEventTimeSlotRepository.Table
                    on new { ActorEventId = ae.Id, TimeSlotId = ts.Id }
                    equals new { aets.ActorEventId, aets.TimeSlotId }
                    into aetsGroup
                from aets in aetsGroup.DefaultIfEmpty()
                join actorPicture in _actorPictureRepository.Table
                    on a.Id equals actorPicture.ActorId into actorPictures
                where ts.Id == timeSlotId
                      && !ae.Deleted
                      && !a.Deleted
                      && !ts.Deleted
                select new ActorEventTimeSlotsModel
                {
                    Id = aets != null ? aets.Id : 0,
                    ActorName = a.Name,
                    ActorPicture = actorPictures.DefaultIfEmpty().FirstOrDefault(),
                    Active = aets == null || aets.IsDeactivated == false,
                    TimeSlotId = ts.Id,
                    ActorEventId = ae.Id
                };

            return query.Distinct().ToPagedListAsync(pageIndex, pageSize);
        }
    }
}