using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Domains;
using Nop.Plugin.Baramjk.Wallet.Domain;

namespace Nop.Plugin.Baramjk.Wallet.Data
{
    public class WalletHistoryBuilder : NopEntityBuilder<WalletHistory>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(WalletHistory.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(WalletHistory.Amount)).AsDecimal()
                .WithColumn(nameof(WalletHistory.CustomerWalletId)).AsInt32()
                .WithColumn(nameof(WalletHistory.WalletHistoryType)).AsInt32()
                .WithColumn(nameof(WalletHistory.CreateDateTime)).AsDateTime()
                .WithColumn(nameof(WalletHistory.OriginatedEntityId)).AsInt32()
                .WithColumn(nameof(WalletHistory.Redeemed)).AsBoolean()
                .WithColumn(nameof(WalletHistory.RedeemedForEntityId)).AsInt32()
                .WithColumn(nameof(WalletHistory.ExpirationDateTime)).AsDateTime().Nullable();
        }
    }
}