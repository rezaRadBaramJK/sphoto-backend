using System;
using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains.PaymentFeeRule;
using Nop.Plugin.Baramjk.Framework.Extensions;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Data
{
    [NopMigration("2025/07/15 17:00:00:1687541", "Core.MyFatoorah - Base Migration")]
    public class BaseMigration : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public BaseMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            this.BuildTableIfNotExists<PaymentFeeRule>(_migrationManager);
        }
    }

    [NopMigration("2025/07/27 17:00:00:1687541", "Core.MyFatoorah - Add Suppliers Migration")]
    public class AddSuppliersTableMigration : Migration
    {
        private readonly IMigrationManager _migrationManager;

        public AddSuppliersTableMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }


        public override void Up()
        {
            this.BuildTableIfNotExists<Supplier>(_migrationManager);
            this.BuildTableIfNotExists<ProductSupplierMapping>(_migrationManager);
            this.BuildTableIfNotExists<TransactionSupplier>(_migrationManager);
        }

        public override void Down()
        {
            DeleteTablesIfExist(typeof(ProductSupplierMapping));
            DeleteTablesIfExist(typeof(TransactionSupplier));
            DeleteTablesIfExist(typeof(Supplier));
        }

        private void DeleteTablesIfExist(params Type[] types)
        {
            foreach (var type in types)
            {
                var tableName = NameCompatibilityManager.GetTableName(type);
                if (Schema.Table(tableName).Exists())
                    Delete.Table(tableName);
            }
        }
    }

    [NopMigration("2025/07/30 17:00:00:1687534", "Core.MyFatoorah - Add Soft delete to payment fee rule Migration")]
    public class AddSoftDeleteToPaymentFeeMigration : Migration
    {
        public override void Up()
        {
            var table = Schema.Table(NameCompatibilityManager.GetTableName(typeof(PaymentFeeRule)));
            if(table.Exists() == false)
                return;
            
            if(table.Column(nameof(PaymentFeeRule.Deleted)).Exists())
                return;

            Alter.Table(NameCompatibilityManager.GetTableName(typeof(PaymentFeeRule)))
                .AddColumn(nameof(PaymentFeeRule.Deleted))
                .AsBoolean()
                .WithDefaultValue(false)
                .NotNullable();

        }

        public override void Down()
        {
            
        }
    }
}