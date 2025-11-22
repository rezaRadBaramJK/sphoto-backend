using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Helpers;
using DateTimeHelper = Nop.Plugin.Baramjk.PhotoPlatform.Helpers.DateTimeHelper;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class TimeSlotService
    {
        private readonly IRepository<TimeSlot> _timeSlotRepository;

        public TimeSlotService(IRepository<TimeSlot> timeSlotRepository)
        {
            _timeSlotRepository = timeSlotRepository;
        }

        public Task InsertAsync(TimeSlot timeSlot)
        {
            timeSlot.CreatedOnUtc = DateTime.UtcNow;
            return _timeSlotRepository.InsertAsync(timeSlot);
        }

        public Task<TimeSlot> GetByIdAsync(int id)
        {
            return _timeSlotRepository.GetByIdAsync(id);
        }

        public async Task<IPagedList<TimeSlot>> GetByEventIdAsync(
            int eventId,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            return await _timeSlotRepository.Table
                .Where(ts => ts.EventId == eventId && ts.Deleted == false)
                .OrderByDescending(ts => ts.Date)
                .ThenByDescending(ts => ts.StartTime)
                .ToPagedListAsync(pageIndex, pageSize);
        }

        public Task CreateTimeSlotsAsync(EventDetail eventDetail)
        {
            if (eventDetail.TimeSlotDuration == 0)
                return Task.CompletedTask;

            var dates = DateTimeHelper.GetDatesBetween(eventDetail.StartDate, eventDetail.EndDate);
            var times = TimeSpanHelper.GetTimeSpanIntervals(eventDetail.StartTime, eventDetail.EndTime, eventDetail.TimeSlotDuration);

            var interval = TimeSpan.FromMinutes(eventDetail.TimeSlotDuration);
            var maxSqlTime = TimeSpan.FromDays(1).Subtract(TimeSpan.FromTicks(1));
            var timeSlotsToInsert = dates.SelectMany(date =>
                times.Where(t => t.Add(interval) <= maxSqlTime).Select(time => new TimeSlot
                {
                    StartTime = time,
                    EndTime = time.Add(interval),
                    Date = date,
                    Active = true,
                    EventId = eventDetail.EventId,
                    CreatedOnUtc = DateTime.UtcNow,
                })
            ).ToList();

            return _timeSlotRepository.InsertAsync(timeSlotsToInsert);
        }

        public Task UpdateAsync(TimeSlot timeSlot)
        {
            return _timeSlotRepository.UpdateAsync(timeSlot);
        }

        public Task DeleteAsync(TimeSlot timeSlot)
        {
            return _timeSlotRepository.DeleteAsync(timeSlot);
        }

        public Task DeleteByEventIdAsync(int eventId)
        {
            return _timeSlotRepository.DeleteAsync(ts => ts.EventId == eventId);
        }

        public Task<List<TimeSlot>> GetEventTimeSlotsAsync(int eventId, DateTime? date = null)
        {
            var query = _timeSlotRepository.Table
                .Where(ts => ts.EventId == eventId && ts.Deleted == false && ts.Active);

            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(ts => ts.Date.Date == targetDate);
            }

            return query.ToListAsync();
        }

        public Task<List<TimeSlot>> GetEventsTimeSlotsAsync(int[] eventIds, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _timeSlotRepository.Table
                .Where(ts => eventIds.Contains(ts.EventId) && ts.Deleted == false && ts.Active);

            if (fromDate.HasValue)
            {
                var targetDate = fromDate.Value.Date;
                query = query.Where(ts => ts.Date.Date >= targetDate);
            }

            if (toDate.HasValue)
            {
                var targetDate = toDate.Value.Date;
                query = query.Where(ts => ts.Date.Date <= targetDate);
            }

            query = query.OrderBy(x => x.Date).ThenBy(x => x.StartTime);

            return query.ToListAsync();
        }
    }
}