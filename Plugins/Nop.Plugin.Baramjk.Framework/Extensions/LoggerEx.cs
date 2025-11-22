using System.Threading.Tasks;
using Nop.Core.Domain.Logging;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.Framework.Extensions
{
    public static class LoggerEx
    {
        public static async Task LogObjectAsync<T>(this ILogger logger, T data, string title = null,
            LogLevel level = LogLevel.Error)
        {
            if (string.IsNullOrEmpty(title))
                title = data.GetType().ToString();

            var serializeObject = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            await logger.InsertLogAsync(level, title, serializeObject, null);
        }
    }
}