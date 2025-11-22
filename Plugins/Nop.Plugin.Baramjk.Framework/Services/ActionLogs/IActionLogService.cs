using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Domain;

namespace Nop.Plugin.Baramjk.Framework.Services.ActionLogs
{
    public interface IActionLogService
    {
        Task<ActionLog> InsertAsync(ActionLog actionLog);
        Task<ActionLog> UpdateAsync(ActionLog actionLog);
        Task<ActionLog> DeleteAsync(ActionLog actionLog);
        Task<ActionLog> GetByIdAsync(int id);
        Task<bool> Any(string group = "", string action = "", int customerId = 0, int entityId = 0);

        Task<List<ActionLog>> SearchAsync(string group = "", string action = "", int customerId = 0,
            int entityId = 0, string value = "", string meta = "");
    }
}