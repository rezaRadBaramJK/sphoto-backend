using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PushNotification.Domain;

namespace Nop.Plugin.Baramjk.PushNotification.Data
{
    public class EventNotificationConfigBuilder: NopEntityBuilder<EventNotificationConfigEntity>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(EventNotificationConfigEntity.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(EventNotificationConfigEntity.EventName)).AsString().NotNullable()
                .WithColumn(nameof(EventNotificationConfigEntity.StatusName)).AsString().Nullable()
                .WithColumn(nameof(EventNotificationConfigEntity.TemplateName)).AsString().Nullable()
                .WithColumn(nameof(EventNotificationConfigEntity.UseSms)).AsBoolean()
                .WithColumn(nameof(EventNotificationConfigEntity.UseFirebase)).AsBoolean()
                .WithColumn(nameof(EventNotificationConfigEntity.UseWhatsApp)).AsBoolean();
        }
    }
}