using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Media;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data.Builders
{
    public class ActorPictureBuilder : NopEntityBuilder<ActorPicture>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(ActorPicture.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(ActorPicture.ActorId)).AsInt32().NotNullable().ForeignKey<Actor>()
                .WithColumn(nameof(ActorPicture.PictureId)).AsInt32().NotNullable().ForeignKey<Picture>()
                .WithColumn(nameof(ActorPicture.DisplayOrder)).AsInt32().WithDefaultValue(1);
        }
    }
}