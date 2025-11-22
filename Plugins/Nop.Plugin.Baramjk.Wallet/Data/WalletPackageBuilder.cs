using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Wallet.Domain;

namespace Nop.Plugin.Baramjk.Wallet.Data
{
    public class WalletPackageBuilder : NopEntityBuilder<WalletPackage>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(WalletPackage.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(WalletPackage.Name)).AsString();
        }
    }
}