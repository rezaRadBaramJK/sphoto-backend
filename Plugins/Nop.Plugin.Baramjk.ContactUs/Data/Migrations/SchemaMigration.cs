using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.ContactUs.Domains;
using Nop.Plugin.Baramjk.Framework.Extensions;

namespace Nop.Plugin.Baramjk.ContactUs.Data.Migrations
{
    [NopMigration("2024/09/01 09:00:00:0000019", "Baramjk.ContactUs - Add base schema")]
    public class SchemaMigration: AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<SubjectEntity>(_migrationManager);
            this.BuildTableIfNotExists<ContactInfoEntity>(_migrationManager);
        }
    }
}