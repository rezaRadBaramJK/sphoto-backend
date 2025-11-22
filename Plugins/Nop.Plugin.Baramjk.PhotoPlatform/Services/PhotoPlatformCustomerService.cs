using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Tasks;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Tasks;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Configuration;
using Nop.Services.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class PhotoPlatformCustomerService
    {

        private readonly IRewardPointService _rewardPointService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IScheduleTaskService _scheduleTaskService;


        public PhotoPlatformCustomerService(
            IRewardPointService rewardPointService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            ISettingService settingService,
            IScheduleTaskService scheduleTaskService)
        {
            _rewardPointService = rewardPointService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _settingService = settingService;
            _scheduleTaskService = scheduleTaskService;
        }

        public async Task GiveBirthdayRewardsAsync()
        {
            var settings = await _settingService.LoadSettingAsync<PhotoPlatformSettings>();
            if (settings.BirthDayRewardPoints <= 0)
                return;

            var today = DateTime.UtcNow.Date;

            var store = await _storeContext.GetCurrentStoreAsync();

            var customersWithBirthdayToday = await _customerService.GetAllCustomersAsync(
                dayOfBirth: today.Day,
                monthOfBirth: today.Month);

            foreach (var customer in customersWithBirthdayToday)
            {
                var lastRewardDate = await _genericAttributeService.GetAttributeAsync<DateTime?>(
                    customer, DefaultValues.CustomerLastRewardPointsReceivedDateKey);

                if (!lastRewardDate.HasValue || lastRewardDate.Value.AddYears(1) <= today)
                {
                    await _rewardPointService.AddRewardPointsHistoryEntryAsync(
                        customer,
                        settings.BirthDayRewardPoints,
                        store.Id,
                        "Birthday reward"
                    );

                    await _genericAttributeService.SaveAttributeAsync(
                        customer,
                        DefaultValues.CustomerLastRewardPointsReceivedDateKey,
                        today
                    );
                }
            }
        }

        public async Task SubmitScheduleTaskAsync()
        {
            var allTasks = await _scheduleTaskService.GetAllTasksAsync();
            var timeSlotTask = allTasks.FirstOrDefault(item => item.Name == BirthdayRewardPointScheduleTask.TaskName);
            if (timeSlotTask != null)
                return;

            timeSlotTask = new ScheduleTask
            {
                Enabled = true,
                Name = BirthdayRewardPointScheduleTask.TaskName,
                Seconds = 60 * 60 * 24,
                Type =
                    $"{typeof(BirthdayRewardPointScheduleTask).FullName}, {typeof(BirthdayRewardPointScheduleTask).Assembly.GetName().Name}"
            };
            await _scheduleTaskService.InsertTaskAsync(timeSlotTask);
        }


        public async Task DeleteScheduleTaskAsync()
        {
            var allTasks = await _scheduleTaskService.GetAllTasksAsync();
            var timeSlotTask = allTasks.FirstOrDefault(item => item.Name == BirthdayRewardPointScheduleTask.TaskName);
            if (timeSlotTask == null)
                return;

            await _scheduleTaskService.DeleteTaskAsync(timeSlotTask);
        }
    }
}