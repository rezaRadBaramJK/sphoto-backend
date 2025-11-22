using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Framework.Services.DataBaseUtils;

namespace Nop.Plugin.Baramjk.Framework.Extensions
{
    public static class SchemaMigrationEx
    {
        public static bool DeleteTableIfExists<T>(this Migration migration)
        {
            if (CheckTableExists<T>(migration))
            {
                migration.Delete.Table(GetTableName<T>());
                return true;
            }

            return false;
        }

        public static bool DeleteTableIfExists<T>(this MigrationBase migration)
        {
            if (CheckTableExists<T>(migration))
            {
                var tableUtilsService = EngineContext.Current.Resolve<IDatabaseTableUtilsService>();
                var tableName = GetTableName<T>();
                tableUtilsService.DeleteTableIfExistAsync(tableName).Wait();
                return true;
            }

            return false;
        }

        public static bool CheckTableExists<T>(this MigrationBase migration)
        {
            return migration.Schema.Table(GetTableName<T>()).Exists();
        }

        public static bool CheckColumnExists<TTable>(this MigrationBase migration, string name)
        {
            return migration.Schema.Table(GetTableName<TTable>()).Column(name).Exists();
        }

        public static bool CheckForeignKeyExists<TTable>(this MigrationBase migration, string foreignKeyName)
        {
            return migration.Schema.Table(GetTableName<TTable>())
                .Constraint(foreignKeyName).Exists();
        }

        public static bool BuildTableIfNotExists<T>(this MigrationBase migration,
            IMigrationManager migrationManager)
        {
            if (migration.CheckTableExists<T>())
                return true;
            migrationManager.BuildTable<T>(migration.Create);
            return false;
        }


        public static string GetTableName<T>()
        {
            return NameCompatibilityManager.GetTableName(typeof(T));
        }
    }
}