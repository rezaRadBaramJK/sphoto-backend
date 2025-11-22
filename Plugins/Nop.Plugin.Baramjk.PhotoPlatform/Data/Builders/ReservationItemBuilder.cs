using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class ReservationItemBuilder: NopEntityBuilder<ReservationItem>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(ReservationItem.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(ReservationItem.ActorId)).AsInt32().NotNullable().ForeignKey<Actor>()
                .WithColumn(nameof(ReservationItem.EventId)).AsInt32().NotNullable().ForeignKey<Product>()
                .WithColumn(nameof(ReservationItem.OrderId)).AsInt32().NotNullable().ForeignKey<Order>()
                .WithColumn(nameof(ReservationItem.TimeSlotId)).AsInt32().NotNullable().ForeignKey<TimeSlot>().OnDelete(Rule.None)
                .WithColumn(nameof(ReservationItem.ReservationStatus)).AsInt32()
                .WithColumn(nameof(ReservationItem.Queue)).AsInt32()
                .WithColumn(nameof(ReservationItem.CameraManPhotoCount)).AsInt32()
                .WithColumn(nameof(ReservationItem.CustomerMobilePhotoCount)).AsInt32();
        }
    }
}