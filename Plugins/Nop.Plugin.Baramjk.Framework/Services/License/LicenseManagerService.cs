using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Services.License.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.License
{
    public class LicenseManagerService : ILicenseManagerService
    {
        private readonly ILicenseParser _licenseParser;
        private readonly IRepository<PluginLicenseRecord> _repositoryBaramjkLicense;

        public LicenseManagerService(IRepository<PluginLicenseRecord> repositoryBaramjkLicense,
            ILicenseParser licenseParser)
        {
            _repositoryBaramjkLicense = repositoryBaramjkLicense;
            _licenseParser = licenseParser;
        }

        public List<PluginLicenseRecord> GetPluginLicenses()
        {
            var pluginLicenseItems = _repositoryBaramjkLicense.Table
                .ToList();

            return pluginLicenseItems;
        }

        public PluginLicense GetPluginLicense(string pluginName)
        {
            try
            {
                var licenseValue = _repositoryBaramjkLicense.Table
                    .FirstOrDefault(item => item.PluginName == pluginName);
                if (string.IsNullOrEmpty(licenseValue?.License))
                    return null;
                var pluginLicense = ParsLicense(licenseValue?.License);
                return pluginLicense;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task SaveLicense(string pluginName, string license)
        {
            var pluginLicense = ParsLicense(license);
            await SaveLicense(pluginName, pluginLicense);
        }

        public async Task SaveLicense(string pluginName, PluginLicense pluginLicense)
        {
            var licenseRecords = _repositoryBaramjkLicense.Table.Where(item => item.PluginName == pluginName).ToList();
            await _repositoryBaramjkLicense.DeleteAsync(licenseRecords);

            await _repositoryBaramjkLicense.InsertAsync(new PluginLicenseRecord
            {
                License = pluginLicense.License,
                Type = pluginLicense.Type,
                PluginName = pluginName,
                Domains = string.Join(",", pluginLicense.Domains),
                ExpireDateTime = pluginLicense.ExpireDateTime,
                OnCreate = DateTime.Now
            });
        }

        public async Task Delete(int id)
        {
            var licenseRecord = _repositoryBaramjkLicense.Table.FirstOrDefault(item => item.Id == id);

            if (licenseRecord == null)
                return;

            await _repositoryBaramjkLicense.DeleteAsync(licenseRecord);
        }

        public async Task DeleteByToken(string token)
        {
            var licenseRecord = _repositoryBaramjkLicense.Table
                .FirstOrDefault(item => item.License == token);

            if (licenseRecord == null)
                return;

            await _repositoryBaramjkLicense.DeleteAsync(licenseRecord);
        }

        protected PluginLicense ParsLicense(string token)
        {
            var license = _licenseParser.Pars(token);
            return license;
        }

        protected PluginLicense TryPars(string token)
        {
            _licenseParser.TryPars(token, out var license);
            return license;
        }
    }
}