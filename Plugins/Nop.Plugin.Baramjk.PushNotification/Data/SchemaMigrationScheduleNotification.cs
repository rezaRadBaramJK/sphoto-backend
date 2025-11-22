using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.PushNotification.Domain;
using Nop.Plugin.Baramjk.PushNotification.Models;

namespace Nop.Plugin.Baramjk.PushNotification.Data
{
    [NopMigration("2024/05/27 12:01:02:1687541", "ScheduleNotification schema ")]

    public class SchemaMigrationScheduleNotification: Migration
    {
        protected IMigrationManager MigrationManager;
        public SchemaMigrationScheduleNotification(IMigrationManager migrationManager)
        {
            MigrationManager = migrationManager;
        }
        public override void Up()
        {
            this.BuildTableIfNotExists<ScheduleNotificationModel>(MigrationManager);
        }

        public override void Down()
        {
            Delete.Table(NameCompatibilityManager.GetTableName(typeof(ScheduleNotificationModel)));
        }
    }
}