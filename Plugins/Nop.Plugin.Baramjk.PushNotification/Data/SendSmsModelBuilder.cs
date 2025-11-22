using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PushNotification.Domain;
using Nop.Plugin.Baramjk.PushNotification.Models;

namespace Nop.Plugin.Baramjk.PushNotification.Data
{
    public class SendSmsModelBuilder : NopEntityBuilder<SendSmsModel>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SendSmsModel.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(SendSmsModel.CustomerId)).AsInt32().Nullable()
                .WithColumn(nameof(SendSmsModel.Receptor)).AsString()
                .WithColumn(nameof(SendSmsModel.DateTime)).AsDateTime()
                .WithColumn(nameof(SendSmsModel.Status)).AsInt32()
                .WithColumn(nameof(SendSmsModel.Text)).AsString();
        }
    }
}