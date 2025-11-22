using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Baramjk.Framework.Domain;

namespace Nop.Plugin.Baramjk.Core.Data.NopEntityBuilders
{
    public class PluginLicenseRecordBuilder : NopEntityBuilder<PluginLicenseRecord>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(PluginLicenseRecord.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(PluginLicenseRecord.License)).AsString()
                .WithColumn(nameof(PluginLicenseRecord.Type)).AsString()
                .WithColumn(nameof(PluginLicenseRecord.PluginName)).AsString()
                .WithColumn(nameof(PluginLicenseRecord.Domains)).AsString()
                .WithColumn(nameof(PluginLicenseRecord.ExpireDateTime)).AsDateTime()
                .WithColumn(nameof(PluginLicenseRecord.OnCreate)).AsDateTime();
        }
    }
}