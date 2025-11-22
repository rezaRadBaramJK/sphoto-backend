using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;


namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class ReservationHistoryBuilder : NopEntityBuilder<ReservationHistory>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(ReservationHistory.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(ReservationHistory.LastChangedBy)).AsInt32().NotNullable().ForeignKey<Customer>().OnDelete(Rule.None)
                .WithColumn(nameof(ReservationHistory.ReservationId)).AsInt32().NotNullable().ForeignKey<ReservationItem>().OnDelete(Rule.None)
                .WithColumn(nameof(ReservationHistory.LastModifiedDate)).AsDateTime().NotNullable()
                .WithColumn(nameof(ReservationHistory.Changes)).AsString(int.MaxValue).Nullable();
        }
    }
}