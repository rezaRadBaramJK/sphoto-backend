using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Banner.Domain.AnywhereSliders;

namespace Nop.Plugin.Baramjk.Banner.Data.Mapping.Builders
{
    public class SlideBuilder : NopEntityBuilder<Slide>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn("SliderId").AsInt32().ForeignKey<Slider>().WithColumn("SlideType").AsInt32().NotNullable();
        }
    }
}