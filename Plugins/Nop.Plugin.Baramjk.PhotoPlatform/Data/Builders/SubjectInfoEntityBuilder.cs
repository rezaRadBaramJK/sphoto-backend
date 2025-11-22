using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class SubjectInfoEntityBuilder : NopEntityBuilder<SubjectEntity>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SubjectEntity.Id)).AsInt32().PrimaryKey().Identity().NotNullable()
                .WithColumn(nameof(SubjectEntity.Name)).AsString().Nullable()
                .WithColumn(nameof(SubjectEntity.Deleted)).AsBoolean().WithDefaultValue(false);
        }
    }
}