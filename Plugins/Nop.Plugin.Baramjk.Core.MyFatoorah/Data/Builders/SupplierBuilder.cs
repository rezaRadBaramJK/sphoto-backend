using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Data.Builders
{
    public class SupplierBuilder: NopEntityBuilder<Supplier>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(Supplier.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(Supplier.Name)).AsString().NotNullable()
                .WithColumn(nameof(Supplier.SupplierCode)).AsInt32().NotNullable()
                .WithColumn(nameof(Supplier.Mobile)).AsString().Nullable()
                .WithColumn(nameof(Supplier.Email)).AsString().Nullable()
                .WithColumn(nameof(Supplier.CommissionValue)).AsDouble().NotNullable()
                .WithColumn(nameof(Supplier.CommissionPercentage)).AsDouble().NotNullable()
                .WithColumn(nameof(Supplier.Status)).AsString().NotNullable()
                .WithColumn(nameof(Supplier.Deleted)).AsBoolean().NotNullable()
                ;
        }
    }
}