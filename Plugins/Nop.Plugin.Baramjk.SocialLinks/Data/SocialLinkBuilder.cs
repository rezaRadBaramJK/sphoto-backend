using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.SocialLinks.Domain;

namespace Nop.Plugin.Baramjk.SocialLinks.Data
{
    public class SocialLinkBuilder : NopEntityBuilder<SocialLink>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SocialLink.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(SocialLink.Name)).AsString()
                .WithColumn(nameof(SocialLink.ImageId)).AsString()
                .WithColumn(nameof(SocialLink.Link)).AsString().Nullable()
                .WithColumn(nameof(SocialLink.Category)).AsInt32()
                .WithColumn(nameof(SocialLink.Priority)).AsInt32()
                .WithColumn(nameof(SocialLink.ShowInFooter)).AsBoolean()
                .WithColumn(nameof(SocialLink.ShowInWidget)).AsBoolean()
                .WithColumn(nameof(SocialLink.ShowInProductDetails)).AsBoolean().WithDefaultValue(false)
                .WithColumn(nameof(SocialLink.SocialSharePrefix)).AsString().Nullable()
    
                ;
        }
    }
}