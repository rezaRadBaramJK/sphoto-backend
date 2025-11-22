using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class CashierDailyBalanceBuilder : NopEntityBuilder<CashierDailyBalance>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(CashierDailyBalance.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(CashierDailyBalance.CashierEventId)).AsInt32().ForeignKey<CashierEvent>()
                .WithColumn(nameof(CashierDailyBalance.Day)).AsDateTime().NotNullable()
                .WithColumn(nameof(CashierDailyBalance.OpeningFundBalanceAmount)).AsDecimal().WithDefaultValue(0);
        }
    }
}