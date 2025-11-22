using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Domains;
using Nop.Plugin.Baramjk.Wallet.Domain;

namespace Nop.Plugin.Baramjk.Wallet.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2023/09/26 12:12:02:1687541", "Wallet - add Note column to History table.")]
    public class HistoryAddNoteColumnMigration : AutoReversingMigration
    {
        public override void Up()
        {
            var historyTableName = NameCompatibilityManager.GetTableName(typeof(WalletHistory));
            var noteColumnName = nameof(WalletHistory.Note);

            if (Schema.Table(historyTableName).Column(noteColumnName).Exists())
                return;
            
            Alter.Table(historyTableName).AddColumn(noteColumnName).AsString().Nullable();
        }
    }
}