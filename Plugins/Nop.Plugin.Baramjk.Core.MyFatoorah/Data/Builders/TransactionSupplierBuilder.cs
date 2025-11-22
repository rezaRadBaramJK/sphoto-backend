using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains;
using Nop.Plugin.Baramjk.Framework.Domain;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Data.Builders
{
    public class TransactionSupplierBuilder : NopEntityBuilder<TransactionSupplier> 
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            
            table
                .WithColumn(nameof(TransactionSupplier.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(TransactionSupplier.InvoiceShare)).AsDecimal().NotNullable()
                .WithColumn(nameof(TransactionSupplier.NopSupplierId)).AsInt32().NotNullable().ForeignKey<Supplier>()
                .WithColumn(nameof(TransactionSupplier.TransactionId)).AsInt32().NotNullable().ForeignKey<GatewayPaymentTranslation>()
                ;
        }
    }
}