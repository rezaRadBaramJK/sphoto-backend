using System.Threading.Tasks;
using LinqToDB;
using Nop.Core.Domain.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.PushNotification.ScheduledTasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public class PushNotificationScheduleTaskService
    {
        private const string TaskName = "Push Notification";
        
        private readonly IRepository<ScheduleTask> _taskRepository;

        public PushNotificationScheduleTaskService(IRepository<ScheduleTask> taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task Init()
        {
            var task = await GetTaskAsync();
            if (task != null)
                return;

            await AddAsync();
        }

        public Task AddAsync()
        {
            return _taskRepository.InsertAsync(new ScheduleTask
            {
                Enabled = true,
                Name = TaskName,
                Seconds = 3600,
                Type = $"{typeof(PushNotificationScheduleTask).FullName}, {typeof(PushNotificationScheduleTask).Assembly.GetName().Name}",
                StopOnError = false
            });
        }
        

        public Task<ScheduleTask> GetTaskAsync()
        {
            return _taskRepository.Table.FirstOrDefaultAsync(t => t.Name == TaskName);
        }
    }
}