using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.SocialLinks.Domain;

namespace Nop.Plugin.Baramjk.SocialLinks.Data
{
    [NopMigration("2022/11/23 19:01:02:1687541", "Social Links Entity v2")]

    public class SchemaMigration: Migration
    {
        protected IMigrationManager MigrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            MigrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<SocialLink>(MigrationManager);
            // MigrationManager.BuildTable<SocialLink>(Update);
   
        }

        public override void Down()
        {
            Delete.Table(NameCompatibilityManager.GetTableName(typeof(SocialLink)));
        }
    }
}