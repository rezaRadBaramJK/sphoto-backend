using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class SupervisorEventBuilder : NopEntityBuilder<SupervisorEvent>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(SupervisorEvent.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(SupervisorEvent.CustomerId)).AsInt32().NotNullable().ForeignKey<Customer>()
                .WithColumn(nameof(SupervisorEvent.EventId)).AsInt32().NotNullable().ForeignKey<Product>()
                .WithColumn(nameof(SupervisorEvent.Active)).AsBoolean().WithDefaultValue(true);
        }
    }
}