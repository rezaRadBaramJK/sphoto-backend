using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class CashierEventBuilder : NopEntityBuilder<CashierEvent>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(CashierEvent.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(CashierEvent.CustomerId)).AsInt32().NotNullable().ForeignKey<Customer>()
                .WithColumn(nameof(CashierEvent.EventId)).AsInt32().NotNullable().ForeignKey<Product>()
                .WithColumn(nameof(CashierEvent.OpeningFundBalanceAmount)).AsDecimal().NotNullable().WithDefaultValue(decimal.Zero)
                .WithColumn(nameof(CashierEvent.CommissionAmount)).AsDecimal().NotNullable().WithDefaultValue(decimal.Zero)
                .WithColumn(nameof(CashierEvent.IsRefundPermitted)).AsBoolean().WithDefaultValue(true);
        }
    }
}