using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class EventCountryMappingBuilder : NopEntityBuilder<EventCountryMapping>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(EventCountryMapping.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(EventCountryMapping.EventId)).AsInt32().NotNullable()
                .WithColumn(nameof(EventCountryMapping.CountryId)).AsInt32().NotNullable();
        }
    }
}