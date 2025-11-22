using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PushNotification.Models;

namespace Nop.Plugin.Baramjk.PushNotification.Data
{
    public class ScheduleNotificationModelBuilder : NopEntityBuilder<ScheduleNotificationModel>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ScheduleNotificationModel.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(ScheduleNotificationModel.JobId)).AsString().Nullable()
                .WithColumn(nameof(ScheduleNotificationModel.OnDateTime)).AsDateTime()
                .WithColumn(nameof(ScheduleNotificationModel.Title)).AsString().Nullable();

        }
    }
}