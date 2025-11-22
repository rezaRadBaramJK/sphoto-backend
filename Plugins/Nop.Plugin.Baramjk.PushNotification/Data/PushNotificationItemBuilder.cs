using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PushNotification.Domain;

namespace Nop.Plugin.Baramjk.PushNotification.Data
{
    public class PushNotificationItemBuilder : NopEntityBuilder<PushNotificationItem>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(PushNotificationItem.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(PushNotificationItem.Title)).AsString()
                .WithColumn(nameof(PushNotificationItem.Body)).AsString()
                .WithColumn(nameof(PushNotificationItem.Image)).AsString()
                .WithColumn(nameof(PushNotificationItem.NotificationType)).AsString()
                .WithColumn(nameof(PushNotificationItem.Link)).AsString().Nullable()
                .WithColumn(nameof(PushNotificationItem.Code)).AsString().Nullable()
                .WithColumn(nameof(PushNotificationItem.ExtraData)).AsString().Nullable()
                .WithColumn(nameof(PushNotificationItem.Data)).AsString()
                .WithColumn(nameof(PushNotificationItem.NotificationPlatform)).AsInt32().Nullable()
                .WithColumn(nameof(PushNotificationItem.NotificationScopeType)).AsInt32()
                .WithColumn(nameof(PushNotificationItem.OnDateTime)).AsDateTime();
        }
    }
}