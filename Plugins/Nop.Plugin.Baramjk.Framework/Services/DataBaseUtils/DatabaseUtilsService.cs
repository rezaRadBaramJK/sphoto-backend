using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Data;
using Nop.Data;

namespace Nop.Plugin.Baramjk.Framework.Services.DataBaseUtils
{
    public class DatabaseUtilsService : IDatabaseUtilsService
    {
        protected readonly INopDataProvider _provider;

        public DatabaseUtilsService(INopDataProvider provider)
        {
            _provider = provider;
        }

        public async Task<decimal?> GetObjectIdAsync(string name)
        {
            var result = await QueryFirstAsync<ValueResult<decimal?>>($"select Object_ID('{name}') as Result");
            return result?.Result;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, params DataParameter[] parameters)
        {
            return await _provider.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<IList<T>> QueryAsync<T>(string sql, params DataParameter[] parameters)
        {
            return await _provider.QueryAsync<T>(sql, parameters);
        }

        public async Task<T> QueryFirstAsync<T>(string sql, params DataParameter[] parameters)
        {
            return (await _provider.QueryAsync<T>(sql, parameters)).FirstOrDefault();
        }
    }
}