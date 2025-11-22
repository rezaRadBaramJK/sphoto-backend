using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Extensions;

namespace Nop.Plugin.Baramjk.Core.Data.NopEntityBuilders
{
    [NopMigration("2022/12/07 01:01:02:1687542", "Baramjk.Core schema 2")]
    public class BaramjkCoreMigration2 : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public BaramjkCoreMigration2(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<GatewayPaymentTranslation>(_migrationManager);
        }
    }

    [NopMigration("2025/07/30 20:21:02:1687542", "Baramjk.Core - Add Payment Option And Addition Fee To Transaction")]
    public class AddPaymentOptionAndAdditionFeeToTransaction : Migration
    {
        public override void Up()
        {
            var tableName = NameCompatibilityManager.GetTableName(typeof(GatewayPaymentTranslation));
            var transactionTable =
                Schema.Table(tableName);
            
            if(transactionTable.Exists() == false)
                return;
            
            if (transactionTable.Column(nameof(GatewayPaymentTranslation.PaymentOptionId)).Exists() == false)
            {
                Alter.Table(tableName)
                    .AddColumn(nameof(GatewayPaymentTranslation.PaymentOptionId))
                    .AsInt32()
                    .Nullable()
                    .WithDefaultValue(0);
            }
            
            if (transactionTable.Column(nameof(GatewayPaymentTranslation.PaymentFeeRuleId)).Exists() == false)
            {
                Alter.Table(tableName)
                    .AddColumn(nameof(GatewayPaymentTranslation.PaymentFeeRuleId))
                    .AsInt32()
                    .Nullable()
                    .WithDefaultValue(0);
            }
            
            if (transactionTable.Column(nameof(GatewayPaymentTranslation.PaymentFeeValue)).Exists() == false)
            {
                Alter.Table(tableName)
                    .AddColumn(nameof(GatewayPaymentTranslation.PaymentFeeValue))
                    .AsDecimal()
                    .Nullable()
                    .WithDefaultValue(0);
            }
            
        }
        

        public override void Down()
        {
            
        }
    }
}