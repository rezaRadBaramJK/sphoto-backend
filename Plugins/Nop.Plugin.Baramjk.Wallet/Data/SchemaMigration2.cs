using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Domains;
using Nop.Plugin.Baramjk.Wallet.Domain;

namespace Nop.Plugin.Baramjk.Wallet.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2024/06/08 01:01:02:1687541", "Wallet revert")]
    public class SchemaMigration2 : AutoReversingMigration
    {
        protected IMigrationManager MigrationManager;

        public SchemaMigration2(IMigrationManager migrationManager)
        {
            MigrationManager = migrationManager;
        }

        public override void Up()
        {
            if(Schema.Table(nameof(WalletHistory)).Column("IsReverted").Exists())
                return;
            Alter.Table(nameof(WalletHistory)).AddColumn("IsReverted").AsBoolean().WithDefaultValue(false);
            Alter.Table(nameof(WalletHistory)).AddColumn("TxId").AsGuid().Nullable();
        }
    }
}