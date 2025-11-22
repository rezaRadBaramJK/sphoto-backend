using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PushNotification.Domain;

namespace Nop.Plugin.Baramjk.PushNotification.Data
{
    public class PushNotificationTokenBuilder : NopEntityBuilder<PushNotificationToken>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(PushNotificationToken.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(PushNotificationToken.CustomerId)).AsInt32()
                .WithColumn(nameof(PushNotificationToken.Token)).AsString()
                .WithColumn(nameof(PushNotificationToken.IsActive)).AsBoolean().WithDefaultValue(true)
                .WithColumn(nameof(PushNotificationToken.Platform)).AsInt32()
                .WithColumn(nameof(PushNotificationToken.LastModify)).AsDateTime();
        }
    }
}