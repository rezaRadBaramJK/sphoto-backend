using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.Framework.Services.DataBaseUtils
{
    public class DatabaseTableUtilsService : IDatabaseTableUtilsService
    {
        private readonly IDatabaseUtilsService _databaseUtilsService;

        public DatabaseTableUtilsService(IDatabaseUtilsService databaseUtilsService)
        {
            _databaseUtilsService = databaseUtilsService;
        }

        public async Task<bool> CheckTableExistAsync(string name)
        {
            return await _databaseUtilsService.GetObjectIdAsync(name) > 0;
        }

        public async Task<int> DeleteTableIfExistAsync(string name)
        {
            var query = $@"IF OBJECT_ID(N'{name}', N'U') IS NOT NULL  
                               DROP TABLE {name};  
                            GO";

            return await _databaseUtilsService.ExecuteNonQueryAsync(query);
        }
    }
}