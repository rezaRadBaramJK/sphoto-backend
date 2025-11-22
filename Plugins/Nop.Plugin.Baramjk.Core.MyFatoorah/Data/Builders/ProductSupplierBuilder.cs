using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Data.Builders
{
    public class ProductSupplierBuilder : NopEntityBuilder<ProductSupplierMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(ProductSupplierMapping.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(ProductSupplierMapping.ProductId)).AsInt32().NotNullable().ForeignKey<Product>()
                .WithColumn(nameof(ProductSupplierMapping.SupplierId)).AsInt32().NotNullable().ForeignKey<Supplier>()
                ;
        }
    }
}