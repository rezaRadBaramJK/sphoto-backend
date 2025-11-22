using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class ActorBuilder : NopEntityBuilder<Actor>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Actor.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(Actor.Name)).AsString().NotNullable()
                .WithColumn(nameof(Actor.Deleted)).AsBoolean().NotNullable()
                .WithColumn(nameof(Actor.DefaultCameraManEachPictureCost)).AsDecimal()
                .WithColumn(nameof(Actor.DefaultCustomerMobileEachPictureCost)).AsDecimal()
                .WithColumn(nameof(Actor.CustomerId)).AsInt32().NotNullable().ForeignKey<Customer>(onDelete: Rule.None)
                .WithColumn(nameof(Actor.CardNumber)).AsString().Nullable()
                .WithColumn(nameof(Actor.CardHolderName)).AsString().Nullable();
        }
    }
}