using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Framework.Domain;

namespace Nop.Plugin.Baramjk.Core.Data.NopEntityBuilders
{
    public class FavoriteVendorBuilder : NopEntityBuilder<FavoriteVendor>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(FavoriteVendor.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(FavoriteVendor.CustomerId)).AsInt32()
                .WithColumn(nameof(FavoriteVendor.VendorId)).AsInt32();
        }
    }
}