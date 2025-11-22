using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Framework.Domain;

namespace Nop.Plugin.Baramjk.Core.Data.NopEntityBuilders
{
    public class ActionLogBuilder : NopEntityBuilder<ActionLog>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(ActionLog.Id)).AsInt32().PrimaryKey().Identity();
        }
    }
}