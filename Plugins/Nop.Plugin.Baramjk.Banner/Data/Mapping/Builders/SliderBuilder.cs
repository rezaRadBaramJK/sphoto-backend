using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Banner.Domain.AnywhereSliders;

namespace Nop.Plugin.Baramjk.Banner.Data.Mapping.Builders
{
    public class SliderBuilder : NopEntityBuilder<Slider>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn("SystemName").AsString(400).NotNullable();
        }
    }
}