using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.LocationDetector.Domain;

namespace Nop.Plugin.Baramjk.LocationDetector.Data
{
    [NopMigration("2024/08/25 01:02:02:1687545", "LD country location mapping")]

    public class SchemaMigration: AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            _migrationManager.BuildTable<CountryCurrencyMapping>(Create);
        }
    }
}