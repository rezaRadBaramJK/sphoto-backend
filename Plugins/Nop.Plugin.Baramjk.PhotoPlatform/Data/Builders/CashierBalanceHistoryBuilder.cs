using System;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class CashierBalanceHistoryBuilder : NopEntityBuilder<CashierBalanceHistory>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(CashierBalanceHistory.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(CashierBalanceHistory.Amount)).AsDecimal().NotNullable().WithDefaultValue(decimal.Zero)
                .WithColumn(nameof(CashierBalanceHistory.CreatedDateTime)).AsDateTime().WithDefaultValue(DateTime.Now)
                .WithColumn(nameof(CashierBalanceHistory.Deleted)).AsBoolean().WithDefaultValue(false)
                .WithColumn(nameof(CashierBalanceHistory.CashierEventId)).AsInt32().NotNullable().ForeignKey<CashierEvent>()
                .WithColumn(nameof(CashierBalanceHistory.Note)).AsString(500)
                .WithColumn(nameof(CashierBalanceHistory.Type)).AsInt32();
        }
    }
}