using System.Threading.Tasks;
using Nop.Core.Configuration;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Configuration;

namespace Nop.Plugin.Baramjk.Framework.Extensions
{
    public static class SettingServiceEx
    {
        public static async Task SaveSettingModelAsync<TSettings>(this ISettingService service, object model,
            int storeId = 0) where TSettings : ISettings, new()
        {
            var settings = MapUtils.Map<TSettings>(model);
            await service.SaveSettingAsync(settings);
        }
    }
}