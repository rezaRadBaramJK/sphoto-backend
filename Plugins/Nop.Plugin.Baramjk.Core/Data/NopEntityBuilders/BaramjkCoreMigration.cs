using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Domain.Vendors;
using Nop.Plugin.Baramjk.Framework.Extensions;

namespace Nop.Plugin.Baramjk.Core.Data.NopEntityBuilders
{
    [NopMigration("2022/05/12 01:01:02:1687542", "Baramjk.Core  schema 1")]
    public class BaramjkCoreMigration : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public BaramjkCoreMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<PluginLicenseRecord>(_migrationManager);
            this.BuildTableIfNotExists<ActionLog>(_migrationManager);
            this.BuildTableIfNotExists<FavoriteVendor>(_migrationManager);
            this.BuildTableIfNotExists<FavoriteProduct>(_migrationManager);
        }
    }
    
    [NopMigration("2025/02/22 13:00:00:1687541", "Core - Create Vendor details table")]
    public class CreateVendorDetailsMigration : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public CreateVendorDetailsMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<VendorDetail>(_migrationManager);
        }
    }
}