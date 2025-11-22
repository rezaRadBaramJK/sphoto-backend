using System.Collections.Generic;
using System.Threading.Tasks;
using LinqToDB.Data;

namespace Nop.Plugin.Baramjk.Framework.Services.DataBaseUtils
{
    public interface IDatabaseUtilsService
    {
        Task<decimal?> GetObjectIdAsync(string name);
        Task<int> ExecuteNonQueryAsync(string sql, params DataParameter[] parameters);
        Task<IList<T>> QueryAsync<T>(string sql, params DataParameter[] parameters);
        Task<T> QueryFirstAsync<T>(string sql, params DataParameter[] parameters);
    }
}