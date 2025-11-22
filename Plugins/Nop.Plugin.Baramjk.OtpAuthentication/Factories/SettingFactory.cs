using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Settings;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Types;
using Nop.Services.Localization;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Factories
{
    public class SettingFactory
    {
        private readonly ILocalizationService _localizationService;

        public SettingFactory(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public async Task<OtpAuthenticationSettingsModel> PrepareViewModelAsync(OtpAuthenticationSettings settings)
        {
            var model = MapUtils.Map<OtpAuthenticationSettingsModel>(settings);
            model.SendMethodId = (int) settings.SendMethod;
            model.AvailableMethods = await Enum.GetValues<SendMethodType>()
                .SelectAwait(async t => new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedEnumAsync(t),
                    Value = $"{(int)t}",
                    Selected = t == settings.SendMethod
                })
                .ToListAsync();
            return model;
        }
    }
}