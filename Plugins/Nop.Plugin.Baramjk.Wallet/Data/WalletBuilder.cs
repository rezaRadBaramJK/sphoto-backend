using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace Nop.Plugin.Baramjk.Wallet.Data
{
    public class WalletBuilder : NopEntityBuilder<Framework.Services.Wallets.Domains.Wallet>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Framework.Services.Wallets.Domains.Wallet.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(Framework.Services.Wallets.Domains.Wallet.Amount)).AsDecimal()
                .WithColumn(nameof(Framework.Services.Wallets.Domains.Wallet.CurrencyCode)).AsString().Nullable()
                .WithColumn(nameof(Framework.Services.Wallets.Domains.Wallet.CurrencyId)).AsInt32()
                .WithColumn(nameof(Framework.Services.Wallets.Domains.Wallet.CustomerId)).AsInt32()
                .WithColumn(nameof(Framework.Services.Wallets.Domains.Wallet.IsLocked)).AsBoolean()
                .WithColumn(nameof(Framework.Services.Wallets.Domains.Wallet.LockAmount)).AsDecimal();
        }
    }
}