using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class EventDetailsBuilder : NopEntityBuilder<EventDetail>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(EventDetail.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(EventDetail.TermsAndConditions)).AsCustom("NVARCHAR(MAX)").Nullable()
                .WithColumn(nameof(EventDetail.StartDate)).AsDate().NotNullable()
                .WithColumn(nameof(EventDetail.EndDate)).AsDate().NotNullable()
                .WithColumn(nameof(EventDetail.StartTime)).AsTime().NotNullable()
                .WithColumn(nameof(EventDetail.EndTime)).AsTime().NotNullable()
                .WithColumn(nameof(EventDetail.TimeSlotDuration)).AsInt32().NotNullable()
                .WithColumn(nameof(EventDetail.EventId)).AsInt32().ForeignKey<Product>()
                .WithColumn(nameof(EventDetail.ActorShare)).AsDecimal().NotNullable()
                .WithColumn(nameof(EventDetail.ProductionShare)).AsDecimal().NotNullable()
                .WithColumn(nameof(EventDetail.PhotoShootShare)).AsDecimal().NotNullable()
                .WithColumn(nameof(EventDetail.PhotoPrice)).AsDecimal().NotNullable()
                .WithColumn(nameof(EventDetail.Note)).AsString().Nullable()
                .WithColumn(nameof(EventDetail.LocationUrlTitle)).AsString().Nullable()
                .WithColumn(nameof(EventDetail.LocationUrl)).AsString().Nullable();
        }
    }
}