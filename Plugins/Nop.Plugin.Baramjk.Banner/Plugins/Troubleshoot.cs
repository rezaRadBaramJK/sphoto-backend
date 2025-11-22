using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;

namespace Nop.Plugin.Baramjk.Banner.Plugins
{
    public class Troubleshoot : TroubleshootBase
    {
        public override async Task TroubleshootAsync()
        {
            await LogAsync("Start Banner Troubleshoot", "");
            await TroubleshootSettingAsync(BannerPlugin.GetDefaultSetting);
            await AddLocaleResourceAsync(BannerPlugin.GetLocalizationResources);
            await TroubleshootMigrationAsync();
            await InstallPermissionsAsync(new PermissionProvider());
            
            var query = @"
IF COL_LENGTH (N'BannerRecord', N'ProductId') IS NOT NULL
BEGIN
  declare @query nvarchar(max)='
	update [BannerRecord] set [EntityName]=''Product'', EntityId=ProductId where ProductId is not null
	update [BannerRecord] set [EntityName]=''Category'', EntityId=CategoryId where CategoryId is not null
	update [BannerRecord] set [EntityName]=''Vendor'', EntityId=VendorId where VendorId is not null

	ALTER TABLE [BannerRecord]
	DROP COLUMN ProductId

	ALTER TABLE [BannerRecord]
	DROP COLUMN CategoryId

	ALTER TABLE [BannerRecord]
	DROP COLUMN VendorId';

	exec(@query)

END
";
            await ExecuteNonQueryAsync(query);
            await LogAsync("End Banner Troubleshoot", "");
        }
    }
}