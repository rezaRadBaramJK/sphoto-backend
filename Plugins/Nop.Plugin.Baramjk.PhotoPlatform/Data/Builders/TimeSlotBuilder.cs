using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class TimeSlotBuilder: NopEntityBuilder<TimeSlot>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(TimeSlot.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(TimeSlot.Date)).AsDate().NotNullable()
                .WithColumn(nameof(TimeSlot.StartTime)).AsTime().NotNullable()
                .WithColumn(nameof(TimeSlot.EndTime)).AsTime().Nullable()
                .WithColumn(nameof(TimeSlot.Active)).AsBoolean().NotNullable()
                .WithColumn(nameof(TimeSlot.CreatedOnUtc)).AsDateTime2().NotNullable()
                .WithColumn(nameof(TimeSlot.Deleted)).AsBoolean().NotNullable()
                .WithColumn(nameof(TimeSlot.EventId)).AsInt32().ForeignKey<Product>()
                .WithColumn(nameof(TimeSlot.Note)).AsString(int.MaxValue).Nullable()
                ;
        }
    }
}