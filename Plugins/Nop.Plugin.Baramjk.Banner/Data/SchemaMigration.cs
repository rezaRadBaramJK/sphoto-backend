using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Banner.Domain;
using Nop.Plugin.Baramjk.Framework.Extensions;

namespace Nop.Plugin.Baramjk.Banner.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2022/01/27 02:19:02:1687541", "Banner schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<BannerRecord>(_migrationManager);
        }
    }
    
    [NopMigration("2024/09/10 02:19:02:1687541", "Banner - Expiration date time to banner")]
    public class AddExpirationDateTimeToBannerMigration : AutoReversingMigration
    {
        public override void Up()
        {
            var table = Schema.Table(nameof(BannerRecord));
            if(table.Exists() == false)
                return;

            if (table.Column(nameof(BannerRecord.ExpirationDateTime)).Exists() == false)
                Alter.Table(nameof(BannerRecord))
                    .AddColumn(nameof(BannerRecord.ExpirationDateTime))
                    .AsDateTime()
                    .Nullable()
                    .WithDefaultValue(null);
        }
    }
}