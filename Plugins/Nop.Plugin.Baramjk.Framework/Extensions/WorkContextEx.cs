using System.Threading.Tasks;
using Nop.Core;

namespace Nop.Plugin.Baramjk.Framework.Extensions
{
    public static class WorkContextEx
    {
        public static async Task<int> CustomerIdAsync(this IWorkContext context)
        {
            var customer = await context.GetCurrentCustomerAsync();
            return customer.Id;
        }
    }
}