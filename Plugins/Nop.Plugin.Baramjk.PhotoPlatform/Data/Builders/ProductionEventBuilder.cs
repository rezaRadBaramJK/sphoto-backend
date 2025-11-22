using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class ProductionEventBuilder : NopEntityBuilder<ProductionEvent>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(ProductionEvent.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(ProductionEvent.CustomerId)).AsInt32().NotNullable().ForeignKey<Customer>()
                .WithColumn(nameof(ProductionEvent.EventId)).AsInt32().NotNullable().ForeignKey<Product>()
                .WithColumn(nameof(ProductionEvent.Active)).AsBoolean().WithDefaultValue(true);
        }
    }
}