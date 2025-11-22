using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using LinqToDB.Data;
using Nop.Core.Configuration;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Plugin.Baramjk.Framework.Services.Language;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;

namespace Nop.Plugin.Baramjk.Framework.Services.Troubleshoots
{
    public abstract class TroubleshootBase : ITroubleshoot
    {
        protected readonly IRepository<Setting> RepositorySetting;
        protected readonly ISettingService SettingService;
        protected readonly IMigrationManager MigrationManager;
        protected readonly IMigrationInformationLoader MigrationLoader;
        protected readonly IVersionLoader VersionLoader;
        protected readonly ILogger Logger;
        protected readonly IPermissionService PermissionService;
        protected readonly ILocalizationService LocalizationService;
        protected readonly INopDataProvider Provider;
        protected readonly IBaramjkLanguageService BaramjkLanguageService;

        protected TroubleshootBase()
        {
            BaramjkLanguageService = EngineContext.Current.Resolve<IBaramjkLanguageService>();
            Logger = EngineContext.Current.Resolve<ILogger>();
            LocalizationService = EngineContext.Current.Resolve<ILocalizationService>();
            RepositorySetting = EngineContext.Current.Resolve<IRepository<Setting>>();
            SettingService = EngineContext.Current.Resolve<ISettingService>();
            MigrationManager = EngineContext.Current.Resolve<IMigrationManager>();
            MigrationLoader = EngineContext.Current.Resolve<IMigrationInformationLoader>();
            VersionLoader = EngineContext.Current.Resolve<IVersionLoader>();
            PermissionService = EngineContext.Current.Resolve<IPermissionService>();
            Provider = EngineContext.Current.Resolve<INopDataProvider>();
        }

        public abstract Task TroubleshootAsync();

        protected async Task TroubleshootSettingAsync<TSettings>(TSettings defaultSettings)
            where TSettings : ISettings, new()
        {
            var setting = EngineContext.Current.Resolve(typeof(TSettings));
            var name = typeof(TSettings).Name.ToLower() + ".";
            var settingRecords = await RepositorySetting.Table.Where(i => i.Name.StartsWith(name)).ToListAsync();
            var propertyInfos = typeof(TSettings).GetProperties()
                .Where(prop => TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                .ToList();

            if (settingRecords.Count >= propertyInfos.Count)
                return;

            await SettingService.SaveSettingAsync(defaultSettings);
            await LogAsync($"Troubleshoot Setting: {typeof(TSettings).FullName}", setting);
        }

        protected async Task TroubleshootMigrationAsync()
        {
            try
            {
                MigrationManager.ApplyUpMigrations(GetType().Assembly, false);
            }
            catch (Exception e)
            {
                var migrations = MigrationLoader.LoadMigrations();
                var newItems = migrations
                    .Where(i => VersionLoader.VersionInfo.HasAppliedMigration(i.Value.Version) == false)
                    .ToList();

                var errorModel = new
                {
                    e.Message,
                    Items = newItems.Select(i => new
                    {
                        i.Value.Version,
                        i.Value.Description,
                        Name = i.Value.Migration.GetType().ToString()
                    })
                };

                await LogAsync($"Troubleshoot Migration: {GetType().FullName}", errorModel);

                Console.WriteLine(e);
            }
        }

        protected async Task InstallPermissionsAsync(IPermissionProvider permissionProvider)
        {
            await PermissionService.InstallPermissionsAsync(permissionProvider);
        }

        protected async Task AddLocaleResourceAsync(IDictionary<string, string> resources, int? languageId = null)
        {
            if (languageId.HasValue == false)
            {
                if (BaramjkLanguageService == null)
                {
                    await LogAsync("BaramjkLanguageService is null", "");
                    return;
                }
                languageId = await BaramjkLanguageService.GetEnglishLanguageId();
            }
            await LocalizationService.AddLocaleResourceAsync(resources, languageId);
        }

        protected async Task<int> ExecuteNonQueryAsync(string sql, params DataParameter[] parameters)
        {
            return await Provider.ExecuteNonQueryAsync(sql, parameters);
        }

        protected async Task<IList<T>> QueryAsync<T>(string sql, params DataParameter[] parameters)
        {
            return await Provider.QueryAsync<T>(sql, parameters);
        }

        protected async Task LogAsync(string shortMessage, object fullMessage)
        {
            await Logger.InsertLogAsync(LogLevel.Warning, shortMessage,
                Newtonsoft.Json.JsonConvert.SerializeObject(fullMessage));
        }
    }
}