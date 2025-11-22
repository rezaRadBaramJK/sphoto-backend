using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Banner.Domain;

namespace Nop.Plugin.Baramjk.Banner.Data
{
    public class BannerRecordBuilder : NopEntityBuilder<BannerRecord>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(BannerRecord.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(BannerRecord.EntityId)).AsInt32().Nullable()
                .WithColumn(nameof(BannerRecord.EntityName)).AsString().Nullable()
                .WithColumn(nameof(BannerRecord.DisplayOrder)).AsInt32()
                .WithColumn(nameof(BannerRecord.FileName)).AsString()
                .WithColumn(nameof(BannerRecord.Title)).AsString().Nullable()
                .WithColumn(nameof(BannerRecord.Text)).AsString().Nullable()
                .WithColumn(nameof(BannerRecord.Link)).AsString().Nullable()
                .WithColumn(nameof(BannerRecord.AltText)).AsString().Nullable()
                .WithColumn(nameof(BannerRecord.Tag)).AsString().Nullable()
                .WithColumn(nameof(BannerRecord.BannerType)).AsInt32()
                .WithColumn(nameof(BannerRecord.ExpirationDateTime)).AsDateTime().Nullable()
                ;
        }
    }
}