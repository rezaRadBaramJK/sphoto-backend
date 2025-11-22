using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class ActorEventTimeSlotBuilder : NopEntityBuilder<ActorEventTimeSlot>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(ActorEventTimeSlot.Id)).AsInt32().PrimaryKey().Identity().NotNullable()
                .WithColumn(nameof(ActorEventTimeSlot.ActorEventId)).AsInt32().NotNullable().ForeignKey<ActorEvent>().OnDelete(Rule.None)
                .WithColumn(nameof(ActorEventTimeSlot.TimeSlotId)).AsInt32().NotNullable().ForeignKey<TimeSlot>().OnDelete(Rule.None)
                .WithColumn(nameof(ActorEventTimeSlot.IsDeactivated)).AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }
}