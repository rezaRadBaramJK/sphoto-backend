using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.PushNotification.Domain;
using Nop.Plugin.Baramjk.PushNotification.Models;

namespace Nop.Plugin.Baramjk.PushNotification.Data
{
    [NopMigration("2023/10/15 12:01:02:1687541", "PushNotification schema  v2")]
    public class SchemaMigration : Migration
    {
        private readonly IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<PushNotificationToken>(_migrationManager);
            this.BuildTableIfNotExists<PushNotificationItem>(_migrationManager);
            this.BuildTableIfNotExists<PushNotificationItemRelCustomer>(_migrationManager);
            this.BuildTableIfNotExists<SendSmsModel>(_migrationManager);
            this.BuildTableIfNotExists<CustomerNotifyProfileEntity>(_migrationManager);
        }

        public override void Down()
        {
            Delete.Table(NameCompatibilityManager.GetTableName(typeof(PushNotificationToken)));
            Delete.Table(NameCompatibilityManager.GetTableName(typeof(PushNotificationItem)));
            Delete.Table(NameCompatibilityManager.GetTableName(typeof(PushNotificationItemRelCustomer)));
            Delete.Table(NameCompatibilityManager.GetTableName(typeof(SendSmsModel)));
        }
    }
    
    [NopMigration("2024/08/11 12:01:02:1687541", "PushNotification - Add 'EventNotificationConfigMigration' table.")]
    public class EventNotificationConfigMigration : AutoReversingMigration
    {
        
        private readonly IMigrationManager _migrationManager;

        public EventNotificationConfigMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<EventNotificationConfigEntity>(_migrationManager);
        }
    }
    
    [NopMigration("2024/08/12 12:01:02:1687541", "PushNotification - Add 'TemplateName' To 'EventNotificationConfig' table.")]
    public class AddTemplateNameToEventNotificationConfigMigration : AutoReversingMigration
    {
        
        private readonly IMigrationManager _migrationManager;

        public AddTemplateNameToEventNotificationConfigMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            var table = Schema.Table(nameof(EventNotificationConfigEntity));
            
            if(table.Exists() == false)
                this.BuildTableIfNotExists<EventNotificationConfigEntity>(_migrationManager);

            if (table.Column(nameof(EventNotificationConfigEntity.TemplateName)).Exists() == false)
                Alter.Table(nameof(EventNotificationConfigEntity))
                    .AddColumn(nameof(EventNotificationConfigEntity.TemplateName))
                    .AsString()
                    .Nullable();

        }
    }
}