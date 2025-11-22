using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Domain;

namespace Nop.Plugin.Baramjk.Framework.Services.ActionLogs
{
    public class ActionLogService : IActionLogService
    {
        private readonly IRepository<ActionLog> _repositoryActionLog;

        public ActionLogService(IRepository<ActionLog> repositoryActionLog)
        {
            _repositoryActionLog = repositoryActionLog;
        }

        public async Task<ActionLog> InsertAsync(ActionLog actionLog)
        {
            await _repositoryActionLog.InsertAsync(actionLog);
            return actionLog;
        }

        public async Task<ActionLog> UpdateAsync(ActionLog actionLog)
        {
            await _repositoryActionLog.UpdateAsync(actionLog);
            return actionLog;
        }

        public async Task<ActionLog> DeleteAsync(ActionLog actionLog)
        {
            await _repositoryActionLog.DeleteAsync(actionLog);
            return actionLog;
        }

        public async Task<ActionLog> GetByIdAsync(int id)
        {
            return await _repositoryActionLog.Table.FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<bool> Any(string group = "", string action = "", int customerId = 0, int entityId = 0)
        {
            return await _repositoryActionLog.Table.AnyAsync(item =>
                (group == string.Empty || item.Group == group)
                && (action == string.Empty || item.Action == action)
                && (customerId == 0 || item.CustomerId == customerId)
                && (entityId == 0 || item.EntityId == entityId)
            );
        }

        public async Task<List<ActionLog>> SearchAsync(string group = "", string action = "", int customerId = 0,
            int entityId = 0, string value = "", string meta = "")
        {
            return await _repositoryActionLog.Table.Where(item =>
                (group == string.Empty || item.Group == group)
                && (action == string.Empty || item.Action == action)
                && (customerId == 0 || item.CustomerId == customerId)
                && (entityId == 0 || item.EntityId == entityId)
                && (value == string.Empty || item.Value == value)
                && (meta == string.Empty || item.Meta == meta)
            ).ToListAsync();
        }
    }
}