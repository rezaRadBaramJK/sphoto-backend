using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Framework.Domain;

namespace Nop.Plugin.Baramjk.Core.Data.NopEntityBuilders
{
    public class FavoriteProductBuilder : NopEntityBuilder<FavoriteProduct>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(FavoriteProduct.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(FavoriteProduct.CustomerId)).AsInt32()
                .WithColumn(nameof(FavoriteProduct.ProductId)).AsInt32();
        }
    }
}