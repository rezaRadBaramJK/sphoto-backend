using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PushNotification.Domain;

namespace Nop.Plugin.Baramjk.PushNotification.Data
{
    public class CustomerNotifyProfileBuilder : NopEntityBuilder<CustomerNotifyProfileEntity>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(CustomerNotifyProfileEntity.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(CustomerNotifyProfileEntity.CustomerId)).AsInt32()
                .WithColumn(nameof(CustomerNotifyProfileEntity.Sale)).AsBoolean()
                .WithColumn(nameof(CustomerNotifyProfileEntity.Discount)).AsBoolean()
                .WithColumn(nameof(CustomerNotifyProfileEntity.BackInStock)).AsBoolean();
        }
    }
}