using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Tasks
{
    public class BirthdayRewardPointScheduleTask : IScheduleTask
    {
        private readonly PhotoPlatformCustomerService _photoPlatformCustomerService;

        public BirthdayRewardPointScheduleTask(PhotoPlatformCustomerService photoPlatformCustomerService)
        {
            _photoPlatformCustomerService = photoPlatformCustomerService;
        }

        public const string TaskName = "Photo Platform Birthday Reward Point Schedule";

        public Task ExecuteAsync()
        {
            Task.Run(_photoPlatformCustomerService.GiveBirthdayRewardsAsync);
            return Task.CompletedTask;
        }
    }
}