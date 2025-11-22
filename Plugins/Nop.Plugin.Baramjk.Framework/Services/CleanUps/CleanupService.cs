using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LinqToDB.Data;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Services.CleanUps.Abstractions;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Data.Mapping;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;
using StackExchange.Profiling.Internal;

namespace Nop.Plugin.Baramjk.Framework.Services.CleanUps
{
    public class CleanupService
    {
        private readonly ITypeFinder _typeFinder;
        private readonly INopDataProvider _dataProvider;
        
        public CleanupService(
            ITypeFinder typeFinder,
            INopDataProvider dataProvider)
        {
            _typeFinder = typeFinder;
            _dataProvider = dataProvider;
        }


        public async Task CleanupPicturesAsync()
        {
            var (dataConnection, dbConnection) = await GetOpenDbConnectionAsync();
            if (dataConnection == null || dbConnection == null)
                return;

            
            var usablePictureIds = await GetBaramjkPictureIdsAsync(dbConnection);
            var nopPictureIds = await GetNopPictureIdsAsync();
            usablePictureIds.AddRange(nopPictureIds);
            usablePictureIds = usablePictureIds.Distinct().Where(pId => pId > 0).ToList();

            Console.WriteLine(usablePictureIds.ToJson());
            
            await dataConnection.DisposeAsync();
        }
        
        private async Task<List<int>> GetBaramjkPictureIdsAsync(DbConnection dbConnection)
        {
            var cleanUpTypeTypes = _typeFinder.FindClassesOfType<IBaramjkCleanup>().ToArray();
            var usablePictureIds = new List<int>();
            
            foreach (var type in cleanUpTypeTypes)
            {
                if (type.BaseType == null || type.BaseType != typeof(BaseEntity))
                    continue;
                
                var picCleanup = (IBaramjkCleanup)Activator.CreateInstance(type);
                if (picCleanup == null)
                    return new List<int>();
                
                var propertyNames = picCleanup.GetPropertyNames();
                if(propertyNames.Any() == false)
                    return new List<int>();
                
                var currentTypePictureIds=  await GetIdsAsync(type, propertyNames, dbConnection);
                usablePictureIds.AddRange(currentTypePictureIds);
            }
            
            usablePictureIds = usablePictureIds.Distinct().ToList();
            return usablePictureIds;
        }

        private static async Task<List<int>> GetIdsAsync(Type type, string[] propertyNames, DbConnection dbConnection) 
        {
            var query = BuildQuery(type, propertyNames);
            
            await using var command = dbConnection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;

            await using var reader = await command.ExecuteReaderAsync();
            
            var result = new List<int>();
            
            while (await reader.ReadAsync())
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    try
                    {
                        var value = reader.GetInt32(i);
                        if(value > 0)
                            result.Add(value);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            return result;
        }
        
        private async Task<(DataConnection dataConnection, DbConnection dbConnection)> GetOpenDbConnectionAsync()
        {
            var createMethod = _dataProvider.GetType().GetMethod("CreateDataConnectionAsync", BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (createMethod == null)
                return (null, null);

            var createTask = (Task)createMethod.Invoke(_dataProvider, null);
            if (createTask == null)
                return (null, null);

            await createTask.ConfigureAwait(false);
            if (createTask.GetType().GetProperty("Result")?.GetValue(createTask) is not DataConnection dataConnection)
                return (null, null);

            if (dataConnection.Connection is not DbConnection dbConn)
                return (dataConnection, null);

            if (MiniProfiler.Current != null && dbConn is not ProfiledDbConnection)
                dbConn = new ProfiledDbConnection(dbConn, MiniProfiler.Current);

            if (dbConn.State != ConnectionState.Open)
                await dbConn.OpenAsync();

            return (dataConnection, dbConn);
        }

        private static string BuildQuery(Type type, string[] propertyNames)
        {
            var tableName = NameCompatibilityManager.GetTableName(type);
            var columnNames = propertyNames.Select(pn => NameCompatibilityManager.GetColumnName(type, pn));
            var columns = string.Join(",", columnNames);
            var query = $"select {columns} from {tableName}";

            var isSoftDeleted = typeof(ISoftDeletedEntity).IsAssignableFrom(type);
            if (isSoftDeleted)
            {
                var deletedColumnName = NameCompatibilityManager.GetColumnName(type, nameof(ISoftDeletedEntity.Deleted));
                query += $" where {deletedColumnName} = 0";
            }

            return query;
        }


        private async Task<List<int>> GetNopPictureIdsAsync()
        {
            var nopCleanupTypes = _typeFinder.FindClassesOfType<INopCleanup>();
            var nopPictureIds = Array.Empty<int>().AsQueryable();
            
            foreach (var type in nopCleanupTypes)
            {
                var cleanupObject = EngineContext.Current.ResolveUnregistered(type);
                if(cleanupObject == null)
                    continue;
                
                var cleanup = (INopCleanup) cleanupObject;
                var currentCleanupPictureIdsQuery = cleanup.GetPictureIdsQuery();
                nopPictureIds = nopPictureIds.Union(currentCleanupPictureIdsQuery);
            }

            nopPictureIds = nopPictureIds.Where(pId => pId > 0).Distinct();

            return await nopPictureIds.ToListAsync();
        }
    }
}