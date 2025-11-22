using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Wallet.Domain;

namespace Nop.Plugin.Baramjk.Wallet.Data
{
    public class WithdrawRequestBuilder : NopEntityBuilder<WithdrawRequest>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(WithdrawRequest.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(WithdrawRequest.Status)).AsBoolean().Nullable()
                .WithColumn(nameof(WithdrawRequest.BankName)).AsString().Nullable()
                .WithColumn(nameof(WithdrawRequest.IBAN)).AsString().Nullable()
                .WithColumn(nameof(WithdrawRequest.AccountNumber)).AsString().Nullable()
                .WithColumn(nameof(WithdrawRequest.CartNumber)).AsString().Nullable()
                ;
        }
    }
}