using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PushNotification.Domain;

namespace Nop.Plugin.Baramjk.PushNotification.Data
{
    public class PushNotificationItemRelCustomerBuilder : NopEntityBuilder<PushNotificationItemRelCustomer>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(PushNotificationItemRelCustomer.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(PushNotificationItemRelCustomer.Token)).AsString(250)
                .WithColumn(nameof(PushNotificationItemRelCustomer.CustomerId)).AsInt32()
                .WithColumn(nameof(PushNotificationItemRelCustomer.PushNotificationItemId)).AsInt32();
        }
    }
}