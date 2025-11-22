using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Domains;
using Nop.Plugin.Baramjk.Wallet.Domain;

namespace Nop.Plugin.Baramjk.Wallet.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2021/12/13 01:01:02:1687541", "Wallet base schema 2")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager MigrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            MigrationManager = migrationManager;
        }

        public override void Up()
        {
            MigrationManager.BuildTable<Framework.Services.Wallets.Domains.Wallet>(Create);
            MigrationManager.BuildTable<WalletHistory>(Create);
            MigrationManager.BuildTable<WalletItemPackage>(Create);
            MigrationManager.BuildTable<WalletPackage>(Create);
            MigrationManager.BuildTable<WalletTranslation>(Create);
            MigrationManager.BuildTable<WithdrawRequest>(Create);
        }
    }
}