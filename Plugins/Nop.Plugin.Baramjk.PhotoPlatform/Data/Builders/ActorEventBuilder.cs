using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class ActorEventBuilder : NopEntityBuilder<ActorEvent>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(ActorEvent.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(ActorEvent.ActorId)).AsInt32().NotNullable().ForeignKey<Actor>()
                .WithColumn(nameof(ActorEvent.EventId)).AsInt32().NotNullable().ForeignKey<Product>()
                .WithColumn(nameof(ActorEvent.CameraManEachPictureCost)).AsDecimal()
                .WithColumn(nameof(ActorEvent.CustomerMobileEachPictureCost)).AsDecimal()
                .WithColumn(nameof(ActorEvent.Deleted)).AsBoolean().WithDefaultValue(false)
                .WithColumn(nameof(ActorEvent.CommissionAmount)).AsDecimal().NotNullable().WithDefaultValue(decimal.Zero)
                .WithColumn(nameof(ActorEvent.ProductionShare)).AsDecimal().NotNullable().WithDefaultValue(decimal.Zero)
                .WithColumn(nameof(ActorEvent.ActorShare)).AsDecimal().NotNullable().WithDefaultValue(decimal.Zero)
                .WithColumn(nameof(ActorEvent.DisplayOrder)).AsInt32().WithDefaultValue(0);
        }
    }
}