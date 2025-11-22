using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class ShoppingCartItemTimeSlotBuilder : NopEntityBuilder<ShoppingCartItemTimeSlot>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(ShoppingCartItemTimeSlot.Id)).AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn(nameof(ShoppingCartItemTimeSlot.ShoppingCartItemId)).AsInt32().NotNullable().ForeignKey<ShoppingCartItem>()
                .WithColumn(nameof(ShoppingCartItemTimeSlot.TimeSlotId)).AsInt32().NotNullable().ForeignKey<TimeSlot>()
                .OnDelete(System.Data.Rule.None)
                .WithColumn(nameof(ShoppingCartItemTimeSlot.ActorId)).AsInt32().NotNullable().ForeignKey<Actor>()
                .WithColumn(nameof(ShoppingCartItemTimeSlot.CameraManPhotoCount)).AsInt32()
                .WithColumn(nameof(ShoppingCartItemTimeSlot.CustomerMobilePhotoCount)).AsInt32()
                .WithColumn(nameof(ShoppingCartItemTimeSlot.Deleted)).AsBoolean().WithDefaultValue(false);
        }
    }
}