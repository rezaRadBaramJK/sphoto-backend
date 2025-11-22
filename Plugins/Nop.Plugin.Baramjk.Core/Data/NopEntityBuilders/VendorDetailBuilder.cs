using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Framework.Domain.Vendors;

namespace Nop.Plugin.Baramjk.Core.Data.NopEntityBuilders
{
    public class VendorDetailBuilder: NopEntityBuilder<VendorDetail>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(VendorDetail.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(VendorDetail.VendorId)).AsInt32().ForeignKey<Vendor>()
                .WithColumn(nameof(VendorDetail.StartTime)).AsTime().NotNullable()
                .WithColumn(nameof(VendorDetail.EndTime)).AsTime().Nullable()
                .WithColumn(nameof(VendorDetail.OffDaysOfWeekIds)).AsString().Nullable()
                .WithColumn(nameof(VendorDetail.IsAvailable)).AsBoolean().Nullable();
        }
    }
}