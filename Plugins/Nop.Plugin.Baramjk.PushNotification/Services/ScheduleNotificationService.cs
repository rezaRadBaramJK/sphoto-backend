using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.PushNotification.Models;

namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public class ScheduleNotificationService
    {
        private readonly IRepository<ScheduleNotificationModel> _repository;

        public ScheduleNotificationService(IRepository<ScheduleNotificationModel> repository)
        {
            _repository = repository;
        }

        public async Task InsertAsync(ScheduleNotificationModel scheduleNotificationModel)
        {
            await _repository.InsertAsync(scheduleNotificationModel);
        } 
    }
}