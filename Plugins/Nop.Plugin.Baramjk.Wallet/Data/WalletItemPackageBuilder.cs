using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Wallet.Domain;

namespace Nop.Plugin.Baramjk.Wallet.Data
{
    public class WalletItemPackageBuilder : NopEntityBuilder<WalletItemPackage>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(WalletItemPackage.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(WalletItemPackage.Amount)).AsDecimal()
                .WithColumn(nameof(WalletItemPackage.CurrencyCode)).AsString()
                .WithColumn(nameof(WalletItemPackage.WalletPackageId)).AsInt32();
        }
    }
}