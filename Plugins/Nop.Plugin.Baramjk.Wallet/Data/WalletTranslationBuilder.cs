using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Wallet.Domain;

namespace Nop.Plugin.Baramjk.Wallet.Data
{
    public class WalletTranslationBuilder : NopEntityBuilder<WalletTranslation>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(WalletTranslation.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(WalletTranslation.CustomerId)).AsInt32()
                .WithColumn(nameof(WalletTranslation.Status)).AsInt32()
                .WithColumn(nameof(WalletTranslation.AmountToPay)).AsDecimal()
                .WithColumn(nameof(WalletTranslation.PaymentId)).AsString().Nullable()
                .WithColumn(nameof(WalletTranslation.InvoiceId)).AsString().Nullable();
        }
    }
}